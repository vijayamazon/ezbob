namespace EZBob.DatabaseLib.Model.Database.Broker {
	using FluentNHibernate.Mapping;

	public class BrokerMap : ClassMap<Broker> {
		public BrokerMap() {
			Table("Broker");
			DynamicUpdate();

			Cache.ReadWrite().Region("LongTerm").ReadWrite();

			Id(x => x.ID).GeneratedBy.Assigned().Column("BrokerID");
			Map(x => x.FirmName).Not.Nullable().Length(255);
			Map(x => x.FirmRegNum).Nullable().Length(255);
			Map(x => x.ContactName).Not.Nullable().Length(255);
			Map(x => x.ContactEmail).Not.Nullable().Length(255);
			Map(x => x.ContactMobile).Not.Nullable().Length(255);
			Map(x => x.ContactOtherPhone).Nullable().Length(255);
			Map(x => x.SourceRef).Not.Nullable().Length(255);
			Map(x => x.EstimatedMonthlyClientAmount);
			Map(x => x.Password).Not.Nullable().Length(255);
			References(x => x.WhiteLabel, "WhiteLabelId");
		} // constructor
	} // class BrokerMap
} // namespace EZBob.DatabaseLib.Model.Database.Broker
