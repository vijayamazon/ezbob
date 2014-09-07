namespace EZBob.DatabaseLib.Model.Database.Broker {
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class WhiteLabelProvider {
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Email { get; set; }
		public virtual string Phone { get; set; }
		public virtual string Logo { get; set; }
		public virtual string LogoImageType { get; set; }
		public virtual int LogoWidthPx { get; set; }
		public virtual int LogoHeightPx { get; set; }
		public virtual string LeadingColor { get; set; }
		public virtual string SecondoryColor { get; set; }
		public virtual string FinishWizardText { get; set; }
		public virtual string MobilePhoneTextMessage { get; set; }
		public virtual string FooterText { get; set; }
		public virtual string ConnectorsToEnable { get; set; }
	}

	public class WhiteLabelProviderMap : ClassMap<WhiteLabelProvider> {
		public WhiteLabelProviderMap() {
			Table("WhiteLabelProvider");
			
			Id(x => x.Id);
			Map(x => x.Name).Length(50);
			Map(x => x.Email).Length(300);
			Map(x => x.Phone).Length(20);
			Map(x => x.Logo).Length(int.MaxValue);
			Map(x => x.LogoImageType).Length(30);
			Map(x => x.LogoHeightPx);
			Map(x => x.LogoWidthPx);
			Map(x => x.LeadingColor).Length(30);
			Map(x => x.SecondoryColor).Length(30);
			Map(x => x.FinishWizardText).Length(1000);
			Map(x => x.MobilePhoneTextMessage).Length(160);
			Map(x => x.FooterText).Length(1000);
			Map(x => x.ConnectorsToEnable).Length(1000);
		}
	}

	public class WhiteLabelProviderRepository : NHibernateRepositoryBase<WhiteLabelProvider> {
		public WhiteLabelProviderRepository(ISession session) : base(session) { }

		public WhiteLabelProvider GetByName(string profile) {
			return GetAll().FirstOrDefault(x => x.Name == profile);
		}
	} 
}

