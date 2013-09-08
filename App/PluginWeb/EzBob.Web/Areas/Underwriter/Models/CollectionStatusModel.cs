namespace EzBob.Web.Areas.Underwriter.Models
{
	using System.Collections.Generic;

    public class CollectionStatusItem
    {
        public int LoanId { get; set; }
        public string LoanRefNumber { get; set; }
    }

    public class CollectionStatusModel
    {
        public int CurrentStatus { get; set; }
        public string CollectionDescription { get; set; }
        public List<CollectionStatusItem> Items { get; set; }
    }
}