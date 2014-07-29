namespace EZBob.DatabaseLib.Model.CustomerRelations {
	using FluentNHibernate.Mapping;

	public class CustomerRelationsMap : ClassMap<CustomerRelations> {
		public CustomerRelationsMap() {
			Table("CustomerRelations");
			Id(x => x.Id);

			Map(x => x.CustomerId);
			Map(x => x.UserName).Length(100);
			Map(x => x.Type).Length(20);
			References(x => x.Action, "ActionId");
			References(x => x.Status, "StatusId");
			References(x => x.Rank, "RankId");
			Map(x => x.Comment).Length(1000);
			Map(x => x.Timestamp);
			Map(x => x.IsBroker);
		} // constructor
	} // class CustomerRelationsMap
} // namespace
