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
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            PdfImage image = AdornedElement as PdfImage;
            if (image.Source != null)
            {
                Debug.WriteLine("PdfImageAdorner.OnRender[" + image.PageNo + "]");
                Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
                image.Renderer.EnsureMarkers();
                image.Renderer.DrawMarkers(drawingContext, image.PageNo);
            }
        }
    }
}
