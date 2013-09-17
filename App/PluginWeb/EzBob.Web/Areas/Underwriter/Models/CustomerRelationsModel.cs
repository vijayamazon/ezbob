using EZBob.DatabaseLib.Model.CustomerRelations;

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

	    public static CustomerRelationsModel Create(CustomerRelations customerRelations)
	    {
	        return new CustomerRelationsModel
	        {
	            User = customerRelations.UserName,
	            Action = customerRelations.Action.Name,
	            Status = customerRelations.Status.Name,
	            DateTime = customerRelations.Timestamp,
	            Comment = customerRelations.Comment,
	            Incoming = customerRelations.Incoming
	        };
	    }
    }
}