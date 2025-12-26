using System.Data;

namespace Blanquita.Models
{
    public class SearchResult
    {
        public List<DataRow> MatchingRows { get; set; }
        public int TotalRowsScanned { get; set; }
        public TimeSpan SearchDuration { get; set; }
    }
}
