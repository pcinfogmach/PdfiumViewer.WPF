using PdfiumViewer.Core;
using PdfiumViewer.Drawing;

using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using Size = System.Drawing.Size;

namespace PdfiumViewer
{
    public partial class PdfRenderer
    {
        private bool _isSelectingText = false;
        private PdfMouseState _cachedMouseState = null;

        public PdfTextSelectionState TextSelectionState { get; set; } = null;
        protected bool MousePanningEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the way the viewer should respond to cursor input
        /// </summary>
        [DefaultValue(PdfViewerCursorMode.TextSelection)]
        public PdfViewerCursorMode CursorMode
        {
            get { return _cursorMode; }
            set
            {
                _cursorMode = value;
                MousePanningEnabled = _cursorMode == PdfViewerCursorMode.Pan;
            }
        }
        private PdfViewerCursorMode _cursorMode = PdfViewerCursorMode.TextSelection;

        /// <summary>
        /// Indicates whether the user currently has text selected
        /// </summary>
        public bool IsTextSelected
        {
            get
            {
                var state = TextSelectionState?.GetNormalized();
                if (state == null)
                    return false;

                if (state.EndPage < 0 || state.EndIndex < 0)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Gets the currently selected text
        /// </summary>
        public string SelectedText
        {
            get
            {
                var state = TextSelectionState?.GetNormalized();
                if (state == null)
                    return null;

                var sb = new StringBuilder();
                for (int page = state.StartPage; page <= state.EndPage; page++)
                {
                    int start = 0, end = 0;

                    if (page == state.StartPage)
                        start = state.StartIndex;

                    if (page == state.EndPage)
                        end = (state.EndIndex);
                    else
                        end = Document.CountCharacters(page);

                    if (page != state.StartPage)
                        sb.AppendLine();

                    sb.Append(Document.GetPdfText(new PdfTextSpan(page, start, end - start)));
                }

                return sb.ToString();
            }
        }

        public void SelectAll()
        {
            TextSelectionState = new PdfTextSelectionState()
            {
                StartPage = 0,
                StartIndex = 0,
                EndPage = Document.PageCount - 1,
                EndIndex = Document.CountCharacters(Document.PageCount - 1) - 1
            };

            UpdateAdorner();
        }

        public void SelectCurrentPage()
        {
            TextSelectionState = new PdfTextSelectionState()
            {
                StartPage = PageNo,
                StartIndex = 0,
                EndPage = PageNo,
                EndIndex = Document.CountCharacters(PageNo) - 1
            };

            UpdateAdorner();
        }

        public void CopySelection()
        {
            var text = SelectedText;
            if (text?.Length > 0)
                Clipboard.SetText(text);
        }

        public void DrawTextSelection(DrawingContext graphics, int page, PdfTextSelectionState state)
        {
            if (state == null || state.EndPage < 0 || state.EndIndex < 0)
                return;

            state = state.GetNormalized();

            if (page >= state.StartPage && page <= state.EndPage)
            {
                int start = 0, end = 0;

                if (page == state.StartPage)
                    start = state.StartIndex;

                if (page == state.EndPage)
                    end = (state.EndIndex + 1);
                else
                    end = Document.CountCharacters(page);

                Geometry geometry = null;
                SolidColorBrush brush = new SolidColorBrush(SystemColors.HighlightColor) { Opacity = .7 };
                foreach (var rectangle in Document.GetTextRectangles(page, start, end - start))
                {
                    Rect? bounds = BoundsFromPdf(rectangle);
                    if (bounds is Rect rectBounds)
                    {
                        if (geometry == null)
                        {
                            geometry = new RectangleGeometry(rectBounds);
                        }
                        else
                        {
                            var geometry2 = new RectangleGeometry(rectBounds);
                            geometry = Geometry.Combine(geometry, geometry2, GeometryCombineMode.Union, null);
                        }
                    }
                }
                if (geometry != null)
                    graphics.DrawGeometry(brush, null, geometry);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Handled) return;
            bool handled = true;

            switch (e.Key)
            {
                case Key.A:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        SelectAll();
                    break;

                case Key.C:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        CopySelection();
                    break;

                case Key.Insert:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        CopySelection();
                    break;

                default:
                    handled = false;
                    break;
            }
            e.Handled = handled;
        }

        public bool HandleMouseDownForTextSelection(PdfImage sender, int page, Size viewSize, Point mouseLocation)
        {
            var pdfLocation = PointToPdf(page, viewSize, mouseLocation);
            if (!pdfLocation.IsValid)
                return false;

            var characterIndex = Document.GetCharacterIndexAtPosition(pdfLocation, 4f, 4f);

            if (characterIndex >= 0)
            {
                TextSelectionState = new PdfTextSelectionState()
                {
                    StartPage = pdfLocation.Page,
                    StartIndex = characterIndex,
                    EndPage = -1,
                    EndIndex = -1
                };
                _isSelectingText = true;
                sender.CaptureMouse();
            }
            else
            {
                _isSelectingText = false;
                sender.ReleaseMouseCapture();
                TextSelectionState = null;
            }
            return true;
        }

        public void HandleMouseUpForTextSelection(PdfImage sender)
        {
            _isSelectingText = false;
            sender.ReleaseMouseCapture();
            UpdateAdorner();
        }

        public void HandleMouseMoveForTextSelection(int page, Size viewSize, Point mouseLocation)
        {
            var mouseState = GetMouseState(page, viewSize, mouseLocation);
            if (mouseState.CharacterIndex >= 0)
            {
                Cursor = Cursors.IBeam;
                if (_isSelectingText)
                {
                    TextSelectionState.EndPage = mouseState.PdfLocation.Page;
                    TextSelectionState.EndIndex = mouseState.CharacterIndex;
                    UpdateAdorner();
                }
            }
            else
            {
                Cursor = Cursors.Arrow;
            }
        }

        public bool HandleMouseDoubleClickForTextSelection(PdfImage sender, int page, Size viewSize, Point mouseLocation)
        {
            var pdfLocation = PointToPdf(page, viewSize, mouseLocation);
            if (!pdfLocation.IsValid)
                return false;

            if (Document.GetWordAtPosition(pdfLocation, 4f, 4f, out var word))
            {
                TextSelectionState = new PdfTextSelectionState()
                {
                    StartPage = pdfLocation.Page,
                    EndPage = pdfLocation.Page,
                    StartIndex = word.Offset,
                    EndIndex = word.Offset + word.Length
                };

                UpdateAdorner();
                return true;
            }

            return false;
        }

        public PdfMouseState GetMouseState(int page, Size viewSize, Point mouseLocation)
        {
            // OnMouseMove and OnSetCursor get invoked a lot, often multiple times in succession for the same point.
            // By just caching the mouse state for the last known position we can save a lot of work.

            var currentState = _cachedMouseState;
            if (currentState?.PdfLocation.Page == page && currentState?.MouseLocation == mouseLocation)
                return currentState;

            _cachedMouseState = new PdfMouseState()
            {
                MouseLocation = mouseLocation,
                PdfLocation = PointToPdf(page, viewSize, mouseLocation)
            };

            if (!_cachedMouseState.PdfLocation.IsValid)
                return _cachedMouseState;

            _cachedMouseState.CharacterIndex = Document.GetCharacterIndexAtPosition(_cachedMouseState.PdfLocation, 4f, 4f);

            return _cachedMouseState;
        }

        /// <summary>
        /// Converts client coordinates to PDF coordinates.
        /// </summary>
        /// <param name="location">Client coordinates to get the PDF location for.</param>
        /// <returns>The location in a PDF page or a PdfPoint with IsValid false when the coordinates do not match a PDF page.</returns>
        public PdfPoint PointToPdf(int page, Size viewSize, Point location)
        {
            if (Document == null)
                return PdfPoint.Empty;

            var translated = TranslatePointToPdf(viewSize, Document.PageSizes[page], location);
            translated = Document.PointToPdf(page, new System.Drawing.Point((int)translated.X, (int)translated.Y));
            return new PdfPoint(page, translated);
        }
    }
}
