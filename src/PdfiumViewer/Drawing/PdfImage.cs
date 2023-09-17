using System.Windows.Controls;
using System.Windows.Documents;

namespace PdfiumViewer.Drawing
{
    public class PdfImage : Image
    {
        public PdfRenderer Renderer { get; set; }
        public int PageNo {get; set; }
        private Adorner _adorner = null;

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
    }
}
