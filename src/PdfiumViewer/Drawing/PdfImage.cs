using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

using Size = System.Drawing.Size;

namespace PdfiumViewer.Drawing
{
    public class PdfImage : Image
    {
        public PdfRenderer Renderer { get; set; }
        public int PageNo {get; set; }
        private Adorner _adorner = null;

        public PdfImage() : base()
        {
            ClipToBounds = true;
            Focusable = false;
            PreviewMouseLeftButtonDown += PdfImage_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += PdfImage_PreviewMouseLeftButtonUp;
            PreviewMouseMove += PdfImage_PreviewMouseMove;
        }

        public void AddAdorner()
        {
            // Create an adorner to this Frame
            if (_adorner == null)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(Renderer);
                _adorner = new PdfImageAdorner(this);
                layer?.Add(_adorner);
            }
        }

        public void RemoveAdorner()
        {
            // Remove adorner
            if (_adorner != null)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(Renderer);
                layer?.Remove(_adorner);
                _adorner = null;
            }
        }

        private void PdfImage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Renderer.CursorMode == PdfViewerCursorMode.TextSelection)
            {
                var viewSize = new Size((int)Width, (int)Height);
                var location = e.GetPosition(this);
                if (e.ClickCount == 1)
                {
                    Renderer.HandleMouseDownForTextSelection(this, PageNo, viewSize, location);
                }
                else if (e.ClickCount == 2)
                {
                    Renderer.HandleMouseDoubleClickForTextSelection(this, PageNo, viewSize, location);
                }
            }
        }

        private void PdfImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Renderer.CursorMode == PdfViewerCursorMode.TextSelection)
            {
                Renderer.HandleMouseUpForTextSelection(this);
            }
        }

        private void PdfImage_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Renderer.CursorMode == PdfViewerCursorMode.TextSelection)
            {
                var viewSize = new Size((int)Width, (int)Height);
                var location = e.GetPosition(this);
                Renderer.HandleMouseMoveForTextSelection(this, PageNo, viewSize, location);
            }
        }
    }
}
