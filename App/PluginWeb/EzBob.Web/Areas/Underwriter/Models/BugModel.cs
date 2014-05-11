namespace EzBob.Web.Areas.Underwriter.Models
{
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using System;

	public class BugModel
	{
		public int Id { get; set; }
		public int? CustomerId { get; set; }
		public string Type { get; set; }
		public int? MarketPlaceId { get; set; }
		public int? DirectorId { get; set; }
		public DateTime DateOpened { get; set; }
		public DateTime? DateClosed { get; set; }
		public string TextOpened { get; set; }
		public string TextClosed { get; set; }
		public string UnderwriterOpenedName { get; set; }
		public string UnderwriterClosedName { get; set; }
		public int UnderwriterOpenedId { get; set; }
		public int? UnderwriterClosedId { get; set; }
		public string State { get; set; }

		public Bug FromModel(ICustomerRepository customers, IUsersRepository users)
		{
			return new Bug
			{
				Id = Id,
				Customer = CustomerId == null ? null : customers.Load(CustomerId.Value),
				DateOpened = DateOpened,
				DateClosed = DateClosed,
				MarketPlaceId = MarketPlaceId,
				CreditBureauDirectorId = DirectorId,
				TextOpened = TextOpened,
				TextClosed = TextClosed,
				Type = Type,
				UnderwriterClosed = UnderwriterClosedId == null ? null : users.Load(UnderwriterClosedId.Value),
				UnderwriterOpened = users.Load(UnderwriterOpenedId)
			};
		}

		public static BugModel ToModel(Bug bug)
		{
			if (bug == null) return null;
			var bugModel = new BugModel
			{
				Id = bug.Id,
				CustomerId = bug.Customer.Id,
				DateOpened = bug.DateOpened,
				DateClosed = bug.DateClosed,
				MarketPlaceId = bug.MarketPlaceId,
				TextOpened = bug.TextOpened,
				TextClosed = bug.TextClosed,
				Type = bug.Type,
				State = bug.State.ToString(),
				DirectorId = bug.CreditBureauDirectorId
			};

			if (bug.UnderwriterOpened != null)
			{
				bugModel.UnderwriterOpenedId = bug.UnderwriterOpened.Id;
				bugModel.UnderwriterOpenedName = bug.UnderwriterOpened.FullName;
			}
			if (bug.UnderwriterClosed != null)
			{
				bugModel.UnderwriterClosedId = bug.UnderwriterClosed.Id;
				bugModel.UnderwriterClosedName = bug.UnderwriterClosed.FullName;
			}

			return bugModel;
		}
	}
}
