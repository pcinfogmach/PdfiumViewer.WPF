using PdfiumViewer.Core;
using PdfiumViewer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;
using Size = System.Drawing.Size;

namespace PdfiumViewer
{
    // ScrollPanel.Properties
    public partial class ScrollPanel : ScrollViewer, IPdfDocument, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public ScrollPanel()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            Effect = new DropShadowEffect()
            {
                BlurRadius = 10,
                Direction = 270,
                RenderingBias = RenderingBias.Performance,
                ShadowDepth = 0
            };
            Panel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            VirtualizingPanel.SetIsVirtualizing(Panel, true);
            VirtualizingPanel.SetVirtualizationMode(Panel, VirtualizationMode.Recycling);
            Content = Panel;
            Dpi = (int)VisualTreeHelper.GetDpi(this).PixelsPerInchX;
        }

        public event EventHandler<int> PageChanged;
        public event EventHandler MouseClick;
        public const double DefaultZoomMin = 0.1;
        public const double DefaultZoomMax = 5;
        public const double DefaultZoomFactor = 1.2;
        protected bool IsDisposed;
        protected const int ScrollChangeLine = 16;
        protected Process CurrentProcess { get; } = Process.GetCurrentProcess();
        protected StackPanel Panel { get; set; }
        protected Thickness FrameSpace { get; set; } = new Thickness(5);
        protected Image Frame1 => Frames?.FirstOrDefault();
        protected Image Frame2 => Frames?.Length > 1 ? Frames[1] : null;
        protected Image[] Frames { get; set; }
        protected int ScrollWidth { get; set; } = 50;
        protected int MouseWheelDelta { get; set; }
        protected long MouseWheelUpdateTime { get; set; }

        /// <summary>
        /// Current document.
        /// </summary>
        public PdfDocument Document {
            get => _document;
            set {
                SetProperty(ref _document, value);
                OnPropertyChanged(nameof(PageCount));
                OnPropertyChanged(nameof(IsDocumentLoaded));
            }
        }
        private PdfDocument _document;

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int PageCount => Document?.PageCount ?? 0;
        
        /// <summary>
        /// Number of the current page.
        /// </summary>
        public int PageNo { get => _pageNo; protected set => SetProperty(ref _pageNo, value); }
        private int _pageNo;

        /// <summary>
        /// Page sizes.
        /// </summary>
        public IList<SizeF> PageSizes => Document?.PageSizes;

        /// <summary>
        /// Size of the current page.
        /// </summary>
        public Size CurrentPageSize { get => _currentPageSize; set => SetProperty(ref _currentPageSize, value); }
        private Size _currentPageSize;

        /// <summary>
        /// Pixel density.
        /// </summary>
        public int Dpi { get => _dpi; set => SetProperty(ref _dpi, value); }
        private int _dpi;

        /// <summary>
        /// Zoom mode (FitHeight, FitWidth or None)
        /// </summary>
        public PdfViewerZoomMode ZoomMode { get; protected set; } = PdfViewerZoomMode.FitHeight;

        /// <summary>
        /// Pdf rendering flags
        /// </summary>
        public PdfRenderFlags Flags { get; set; } = PdfRenderFlags.CorrectFromDpi;

        /// <summary>
        /// Page rotation.
        /// </summary>
        public PdfRotation Rotate { get; set; } = PdfRotation.Rotate0;

        /// <summary>
        /// Page display mode (SinglePageMode, BookMode or ContinuousMode)
        /// </summary>
        public PdfViewerPagesDisplayMode PagesDisplayMode {
            get => _pdfViewerPagesDisplayMode;
            set {
                if (SetProperty(ref _pdfViewerPagesDisplayMode, value))
                {
                    OnPagesDisplayModeChanged();
                }
            }
        }
        private PdfViewerPagesDisplayMode _pdfViewerPagesDisplayMode = PdfViewerPagesDisplayMode.ContinuousMode;

        /// <summary>
        /// Mouse wheel mode (PanAndZoom, Pan or Zoom)
        /// </summary>
        public MouseWheelMode MouseWheelMode { get; set; } = MouseWheelMode.PanAndZoom;

        /// <summary>
        /// Is right to left layout.
        /// </summary>
        public bool IsRightToLeft
        {
            get => Panel.FlowDirection == FlowDirection.RightToLeft;
            set => Panel.FlowDirection = value ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        /// <summary>
        /// Document loaded.
        /// </summary>
        public bool IsDocumentLoaded => Document != null && ActualWidth > 0 && ActualHeight > 0;

        /// <summary>
        /// Current zoom level.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(1.0)]
        public double Zoom { get; set; } = 1.0;

        /// <summary>
        /// Maximum zoom level.
        /// </summary>
        [DefaultValue(DefaultZoomMin)] public double ZoomMin { get; set; } = DefaultZoomMin;

        /// <summary>
        /// Minimum zoom level.
        /// </summary>
        [DefaultValue(DefaultZoomMax)] public double ZoomMax { get; set; } = DefaultZoomMax;

        /// <summary>
        /// Zoom step.
        /// </summary>
        [DefaultValue(DefaultZoomFactor)] public double ZoomFactor { get; set; } = DefaultZoomFactor;

        /// <summary>
        /// Bookmarks.
        /// </summary>
        public PdfBookmarkCollection Bookmarks => Document?.Bookmarks;

        /// <summary>
        /// Scroll to selected page in continous mode
        /// </summary>
        /// <param name="page"></param>
        protected void ScrollToPage(int page)
        {
            if (Frames == null) return;

            if (PagesDisplayMode == PdfViewerPagesDisplayMode.ContinuousMode)
            {
                // scroll to current page
                double verticalOffset = 0.0;
                for (int idx = 0; idx < page; idx++) {
                    verticalOffset += Frames[idx].Height + FrameSpace.Top + FrameSpace.Bottom;
                }
                ScrollToVerticalOffset(verticalOffset);
            }
        }

        protected void OnPageNoChanged()
        {
            PageChanged?.Invoke(this, PageNo);
        }

        protected void OnDpiChanged()
        {
            GotoPage(PageNo);
        }

        protected void OnPagesDisplayModeChanged()
        {
            if (IsDocumentLoaded)
            {
                Panel.Children.Clear();
                Frames = null;

                if (PagesDisplayMode == PdfViewerPagesDisplayMode.SinglePageMode)
                {
                    Frames = new Image[1];
                    Panel.Orientation = Orientation.Horizontal;
                }
                else if (PagesDisplayMode == PdfViewerPagesDisplayMode.BookMode)
                {
                    Frames = new Image[2];
                    Panel.Orientation = Orientation.Horizontal;
                }
                else if (PagesDisplayMode == PdfViewerPagesDisplayMode.ContinuousMode)
                {
                    // frames created at scrolling
                    Frames = new Image[Document.PageCount];
                    Panel.Orientation = Orientation.Vertical;
                }

                for (var i = 0; i < Frames.Length; i++)
                {
                    Frames[i] = Frames[i] ?? new Image { Margin = FrameSpace };

                    var pageSize = CalculatePageSize(i);
                    Frames[i].Width = pageSize.Width;
                    Frames[i].Height = pageSize.Height;

                    Panel.Children.Add(Frames[i]);
                }

                GC.Collect();
                GotoPage(PageNo);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            MouseClick?.Invoke(this, EventArgs.Empty);
        }

        protected void OnFlagsChanged()
        {
            GotoPage(PageNo);
        }

        protected BitmapImage RenderPage(Image frame, int page, int width, int height)
        {
            if (frame == null) return null;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var image = Document.Render(page, (int)(width * 1.2f), (int)(height * 1.2f), Dpi, Dpi, Rotate, Flags);
            BitmapImage bitmapImage;
            using (var memory = new MemoryStream())
            {
                image.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // not a mistake - see below
                bitmapImage.EndInit();
                image.Dispose();
            }
            // Why BitmapCacheOption.OnLoad?
            // It seems counter intuitive, but this flag has two effects:
            // It enables caching if caching is possible, and it causes the load to happen at EndInit().
            // In our case caching is impossible, so all it does it cause the load to happen immediately.

            stopwatch.Stop();
            Debug.WriteLine("RenderPage[" + page + "]: " + stopwatch.ElapsedMilliseconds + " mS, size=" + width + " x " + height);

            //CurrentProcess?.Refresh();
            Dispatcher.Invoke(() =>
            {
                frame.Width = width;
                frame.Height = height;
                frame.Source = bitmapImage;
            });
            GC.Collect();
            return bitmapImage;
        }

        protected Size CalculatePageSize(int? page = null)
        {
            page = page ?? PageNo;
            var isReverse = (Rotate == PdfRotation.Rotate90 || Rotate == PdfRotation.Rotate270);
            var containerWidth = ActualWidth - Padding.Left - Padding.Right - FrameSpace.Left - FrameSpace.Right; // ViewportWidth
            var containerHeight = ActualHeight - Padding.Top - Padding.Bottom - FrameSpace.Top - FrameSpace.Bottom; // ViewportHeight

            if (IsDocumentLoaded && containerWidth > 0 && containerHeight > 0)
            {
                var currentPageSize = Document.GetPageSize(page.Value);
                if (isReverse)
                    currentPageSize = new SizeF(currentPageSize.Height, currentPageSize.Width);

                if (ZoomMode == PdfViewerZoomMode.FitHeight)
                {
                    Zoom = containerHeight / currentPageSize.Height;
                }
                else if (ZoomMode == PdfViewerZoomMode.FitWidth)
                {
                    Zoom = (containerWidth - ScrollWidth) / currentPageSize.Width;
                    if (PagesDisplayMode == PdfViewerPagesDisplayMode.BookMode)
                    {
                        Zoom /= 2;
                    }
                }

                return new Size((int)(currentPageSize.Width * Zoom), (int)(currentPageSize.Height * Zoom));
            }

            return new Size();
        }

        protected void ReleaseFrames(int keepFrom, int keepTo)
        {
            for (var f = 0; f < Frames?.Length; f++)
            {
                var frame = Frames[f];
                if ((f < keepFrom || f > keepTo) && frame.Source != null)
                {
                    frame.Source = null;
                }
            }
            GC.Collect();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            OnPagesDisplayModeChanged();
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            SetMouseWheelDelta(e.Delta);
            
            if (IsDocumentLoaded)
            {
                if (MouseWheelMode == MouseWheelMode.Zoom)
                {
                    e.Handled = true;
                    if (e.Delta > 0)
                        ZoomIn();
                    else
                        ZoomOut();
                }
                else if (PagesDisplayMode != PdfViewerPagesDisplayMode.ContinuousMode)
                {
                    var pageStep = PagesDisplayMode == PdfViewerPagesDisplayMode.BookMode ? 2 : 1;

                    if (ViewportHeight > Frame1.ActualHeight)
                    {
                        if (e.Delta > 0) // prev page
                            PreviousPage();
                        else
                            NextPage();
                    }
                    else if (e.Delta < 0 && VerticalOffset >= ScrollableHeight && PageNo < PageCount - pageStep)
                    {
                        NextPage();
                        ScrollToVerticalOffset(0);
                    }
                    else if (e.Delta > 0 && VerticalOffset <= 0 && PageNo > 0)
                    {
                        PreviousPage();
                        ScrollToVerticalOffset(ScrollableHeight);
                    }
                }
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            bool handled = true;

            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                MouseWheelMode = MouseWheelMode.Zoom;

            switch (e.Key)
            {
                case Key.Up:
                    PerformScroll(ScrollAction.LineUp, Orientation.Vertical);
                    break;

                case Key.Down:
                    PerformScroll(ScrollAction.LineDown, Orientation.Vertical);
                    break;

                case Key.Left:
                    PerformScroll(ScrollAction.LineUp, Orientation.Horizontal);
                    break;

                case Key.Right:
                    PerformScroll(ScrollAction.LineDown, Orientation.Horizontal);
                    break;

                case Key.PageUp:
                    PerformScroll(ScrollAction.PageUp, Orientation.Vertical);
                    break;

                case Key.PageDown:
                    PerformScroll(ScrollAction.PageDown, Orientation.Vertical);
                    break;

                case Key.Home:
                    PerformScroll(ScrollAction.Home, Orientation.Vertical);
                    break;

                case Key.End:
                    PerformScroll(ScrollAction.End, Orientation.Vertical);
                    break;

                case Key.Add:
                case Key.OemPlus:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        ZoomIn();
                    break;

                case Key.Subtract:
                case Key.OemMinus:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        ZoomOut();
                    break;
                default:
                    handled = false;
                    break;
            }
            e.Handled = handled;
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control ||
                e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                MouseWheelMode = MouseWheelMode.Pan;
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            base.OnScrollChanged(e);
            if (IsDocumentLoaded &&
                PagesDisplayMode == PdfViewerPagesDisplayMode.ContinuousMode &&
                Frames != null)
            {
                // Render frame for continous mode
                var startOffset = e.VerticalOffset;
                var height = e.ViewportHeight;
                var pageSize = CalculatePageSize(0);

                var startFrameIndex = (int)(startOffset / (pageSize.Height + FrameSpace.Top + FrameSpace.Bottom));
                var endFrameIndex = (int)((startOffset + height) / (pageSize.Height + FrameSpace.Top + FrameSpace.Bottom));

                PageNo = Math.Min(Math.Max(startFrameIndex, 0), PageCount - 1);
                var endPageIndex = Math.Min(Math.Max(endFrameIndex, 0), PageCount - 1);

                ReleaseFrames(PageNo, endPageIndex);

                for (var page = PageNo; page <= endPageIndex; page++)
                {
                    var frame = Frames[page];
                    if (frame.Source == null) // && frame.IsUserVisible())
                    {
                        RenderPage(frame, page, (int)frame.Width, (int)frame.Height);
                    }
                }
            }
        }

        public void PerformScroll(ScrollAction action, Orientation scrollBar)
        {
            if (scrollBar == Orientation.Vertical)
            {
                switch (action)
                {
                    case ScrollAction.LineUp:
                        if (VerticalOffset > ScrollChangeLine)
                            ScrollToVerticalOffset(VerticalOffset - ScrollChangeLine);
                        break;

                    case ScrollAction.LineDown:
                        if (VerticalOffset < ScrollableHeight - ScrollChangeLine)
                            ScrollToVerticalOffset(VerticalOffset + ScrollChangeLine);
                        break;

                    case ScrollAction.PageUp:
                        if (ZoomMode == PdfViewerZoomMode.FitHeight)
                        {
                            PreviousPage();
                        }
                        else
                        {
                            var offset = ActualHeight - Margin.Top - Margin.Bottom - FrameSpace.Top - FrameSpace.Bottom;
                            if (VerticalOffset > offset)
                                ScrollToVerticalOffset(VerticalOffset - offset);
                            else
                                ScrollToVerticalOffset(0);
                        }
                        break;

                    case ScrollAction.PageDown:
                        if (ZoomMode == PdfViewerZoomMode.FitHeight)
                        {
                            NextPage();
                        }
                        else
                        {
                            var offset = ActualHeight - Margin.Top - Margin.Bottom - FrameSpace.Top - FrameSpace.Bottom;
                            if (VerticalOffset < ScrollableHeight - offset)
                                ScrollToVerticalOffset(VerticalOffset + offset);
                            else
                                ScrollToVerticalOffset(ScrollableHeight);
                        }
                        break;

                    case ScrollAction.Home:
                        ScrollToHome();
                        break;

                    case ScrollAction.End:
                        ScrollToEnd();
                        break;
                }
            }
            else // Horizontal
            {
                switch (action)
                {
                    case ScrollAction.LineUp:
                        if (HorizontalOffset > ScrollChangeLine)
                            ScrollToHorizontalOffset(HorizontalOffset - ScrollChangeLine);
                        break;

                    case ScrollAction.LineDown:
                        if (HorizontalOffset < ScrollableHeight - ScrollChangeLine)
                            ScrollToHorizontalOffset(HorizontalOffset + ScrollChangeLine);
                        break;

                    case ScrollAction.PageUp:
                        var offset = ActualWidth - Margin.Left - Margin.Right - FrameSpace.Left - FrameSpace.Right;
                        if (HorizontalOffset > offset)
                            ScrollToHorizontalOffset(HorizontalOffset - offset);
                        else
                            ScrollToHorizontalOffset(0);
                        break;

                    case ScrollAction.PageDown:
                        offset = ActualWidth - Margin.Left - Margin.Right - FrameSpace.Left - FrameSpace.Right;
                        if (HorizontalOffset < ScrollableWidth - offset)
                            ScrollToHorizontalOffset(HorizontalOffset + offset);
                        else
                            ScrollToHorizontalOffset(ScrollableWidth);
                        break;

                    case ScrollAction.Home:
                        ScrollToHome();
                        break;

                    case ScrollAction.End:
                        ScrollToEnd();
                        break;
                }
            }
        }

        protected void SetMouseWheelDelta(int delta)
        {
            MouseWheelUpdateTime = Environment.TickCount;
            MouseWheelDelta = delta;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(!IsDisposed);
        }
    }
}
