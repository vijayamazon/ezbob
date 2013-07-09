namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;

	public class CustomerRelationsModel
    {
        public DateTime DateTime { get; set; }
        public string User { get; set; }
        public string Action { get; set; }
		public string Status { get; set; }
		public string Comment { get; set; }
		public bool Incoming { get; set; }
    }
}