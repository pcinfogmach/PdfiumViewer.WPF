using System.ComponentModel;

namespace PdfiumViewer.Core
{
    public class LinkClickEventArgs : HandledEventArgs
    {
        /// <summary>
        /// Gets the link that was clicked.
        /// </summary>
        public PdfPageLink Link { get; private set; }
        
        public LinkClickEventArgs(PdfPageLink link)
        {
            Link = link;
        }
    }

    public delegate void LinkClickEventHandler(object sender, LinkClickEventArgs e);
}
