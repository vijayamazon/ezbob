namespace EzBob.Web.Areas.Underwriter.Models
{
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using System;

	public class CustomerRelationsModel
	{
		public int Id { get; set; }
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
				Id = customerRelations.Id,
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