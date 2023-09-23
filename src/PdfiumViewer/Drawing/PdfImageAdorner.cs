using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace PdfiumViewer.Drawing
{
    // Adorners must subclass the abstract base class Adorner.
    public class PdfImageAdorner : Adorner
    {
        // Be sure to call the base class constructor.
        public PdfImageAdorner(UIElement adornedElement) : base(adornedElement)
        {
            IsClipEnabled = true;
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            PdfImage image = AdornedElement as PdfImage;
            if (image.Source != null)
            {
                //Debug.WriteLine("PdfImageAdorner.OnRender[" + image.PageNo + "]");
                image.Renderer.EnsureMarkers();
                image.Renderer.DrawMarkers(drawingContext, image.PageNo);

                image.Renderer.DrawTextSelection(drawingContext, image.PageNo, image.Renderer.TextSelectionState);
            }
        }
    }
}
