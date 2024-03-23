using System;

namespace PdfiumViewer.Core
{
    /// <summary>
    /// Contains text from metadata of the document.
    /// </summary>
    public class PdfInformation
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Subject { get; set; }
        public string Keywords { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string Creator { get; set; }
        public string Producer { get; set; }
        public string Version { get; set; }
        public int PageCount { get; set; }
        public double PageWidth { get; set; }
        public double PageHeight { get; set; }
    }
}
