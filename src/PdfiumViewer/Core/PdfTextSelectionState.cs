namespace PdfiumViewer.Core
{
    public class PdfTextSelectionState
    {
        public int StartPage { get; set; }
        public int StartIndex { get; set; } = -1;
        public int EndPage { get; set; }
        public int EndIndex { get; set; } = -1;

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

        public void  Normalize()
        {
            if (EndPage < 0 || EndIndex < 0) // Special case when only start position is known
                return;

            if (EndPage < StartPage ||
                (StartPage == EndPage && EndIndex < StartIndex))
            {
                // End position is before start position.
                // Swap positions so start is always before end.
                var page = this.StartPage;
                var index = this.StartIndex;
                StartPage = this.EndPage;
                StartIndex = this.EndIndex;
                EndPage = page;
                EndIndex = index;
            }
        }

        public bool IsPositionInside(int page, int pos)
        {
            if (page < StartPage || page > EndPage) return false;
            if (page == StartPage && pos < StartIndex) return false;
            if (page == EndPage && pos > EndIndex) return false;
            return true;
        }

        /// <summary>
        /// Merge two selection region to one. 
        /// Normalize the region before merge.
        /// </summary>
        /// <param name="src1"></param>
        /// <param name="src2"></param>
        /// <returns></returns>
        public static PdfTextSelectionState Merge(PdfTextSelectionState src1, PdfTextSelectionState src2)
        {
            var result = new PdfTextSelectionState();

            if (src1.StartPage < src2.StartPage || 
                (src1.StartPage == src2.StartPage && src1.StartIndex <= src2.StartIndex))
            {
                // src1 is before src2
                result.StartPage = src1.StartPage;
                result.StartIndex = src1.StartIndex;
                result.EndPage = src2.EndPage;
                result.EndIndex = src2.EndIndex;
            }
            else
            {
                // src1 is after src2
                result.StartPage = src2.StartPage;
                result.StartIndex = src2.StartIndex;
                if (src1.EndPage < 0 || src1.EndIndex < 0)
                {
                    // Special case when only start position is known
                    result.EndPage = src1.StartPage;
                    result.EndIndex = src1.StartIndex > 0 ? src1.StartIndex-1 : src1.StartIndex;
                }
                else
                {
                    result.EndPage = src1.EndPage;
                    result.EndIndex = src1.EndIndex;
                }
            }
            return result;
        }
    }
}
