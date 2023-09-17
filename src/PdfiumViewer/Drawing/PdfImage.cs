using System.Windows.Controls;
using System.Windows.Media;

namespace PdfiumViewer.Drawing
{
    public class PdfImage : Image
    {
        public PdfRenderer Renderer { get; set; }
        public int PageNo {get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (Source != null)
            {
                Renderer.EnsureMarkers();
                Renderer.DrawMarkers(drawingContext, PageNo);
            }
        }
    }
}
