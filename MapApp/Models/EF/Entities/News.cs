using System;

namespace MapApp.Models.EF.Entities
{
    public class News
    {
        public string Id { get; set; }
        public string Header { get; set; }
        public string HeaderImg { get; set; }
        public string ShortDescription { get; set; }
        public string Longdescriotion { get; set; }
        public DateTime LoadDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
