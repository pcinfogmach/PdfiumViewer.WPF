using PdfiumViewer.Drawing;
using System.Windows;

namespace PdfiumViewer.Core
{
    public class PdfMouseState
    {
        public Point MouseLocation { get; set; }
        public PdfPoint PdfLocation { get; set; }
        public int CharacterIndex { get; set; }
    }
}
