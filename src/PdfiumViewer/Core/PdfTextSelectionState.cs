namespace PdfiumViewer.Core
{
    public class PdfTextSelectionState
    {
        public int StartPage { get; set; }
        public int StartIndex { get; set; }
        public int EndPage { get; set; }
        public int EndIndex { get; set; }

        public PdfTextSelectionState GetNormalized()
        {
            if (EndPage < 0 || EndIndex < 0) // Special case when only start position is known
                return this;

            if (EndPage < StartPage ||
                (StartPage == EndPage && EndIndex < StartIndex))
            {
                // End position is before start position.
                // Swap positions so start is always before end.

                return new PdfTextSelectionState()
                {
                    StartPage = this.EndPage,
                    StartIndex = this.EndIndex,
                    EndPage = this.StartPage,
                    EndIndex = this.StartIndex
                };
            }

            return this;
        }
    }
}
