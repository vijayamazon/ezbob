namespace EZBob.DatabaseLib.Model.Database {
	using FluentNHibernate.Mapping;

	public class CampaignSource {
		public virtual int Id { get; set; }
		public virtual string RTerm { get; set; }

		public virtual Customer Customer { get; set; }
	}

	public class CampaignSourceMap : ClassMap<CampaignSource> {
		public CampaignSourceMap() {
			Table("CampaignSourceRef");
			Id(x => x.Id);
			Map(x => x.RTerm);

			References(x => x.Customer)
				.Column("CustomerId")
				.Unique()
				.Cascade.None();
		}
	}
}
