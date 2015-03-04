namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using FluentNHibernate.Mapping;
	using System.Collections.Generic;
	using EzBob.CommonLib.TimePeriodLogic;
	using Marketplaces;

	public class MP_MarketplaceType {
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual System.Guid InternalId { get; set; }
		public virtual string Description { get; set; }
		public virtual bool IsPaymentAccount { get; set; }
		public virtual int UWPriority { get { return 0; } }
		public virtual bool ActiveWizardOnline { get; set; }
		public virtual bool ActiveDashboardOnline { get; set; }
		public virtual bool ActiveWizardOffline { get; set; }
		public virtual bool ActiveDashboardOffline { get; set; }
		public virtual int? PriorityOnline { get; set; }
		public virtual int? PriorityOffline { get; set; }
		public virtual MP_MarketplaceGroup Group { get; set; }
		public virtual string Ribbon { get; set; }
		public virtual bool MandatoryOnline { get; set; }
		public virtual bool MandatoryOffline { get; set; }

		public virtual IEnumerable<IAnalysisDataParameterInfo> GetAggregations(MP_CustomerMarketPlace mp, DateTime? history) {
			return new List<IAnalysisDataParameterInfo>();
		}

		protected virtual DateTime GetRelevantDate(DateTime? history) {
			DateTime now = history ?? DateTime.UtcNow;

			int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
			bool isPreviousMonth = now.Day < daysInMonth - 3;

			DateTime relevantMonth = isPreviousMonth ? now.AddMonths(-1) : now;
			return relevantMonth;
		}

		protected const int MonthsInYear = 12;
	}

	public class MP_MarketplaceTypeMap : ClassMap<MP_MarketplaceType> {

		public MP_MarketplaceTypeMap() {
			Table("MP_MarketplaceType");
			Not.LazyLoad();
			Cache.ReadWrite().Region("Longest").ReadWrite();
			Id(x => x.Id).GeneratedBy.Identity().Column("Id");
			Map(x => x.Name).Column("Name").Not.Nullable().Length(255);
			Map(x => x.InternalId).Not.Nullable();
			Map(x => x.Description);
			Map(x => x.ActiveDashboardOffline);
			Map(x => x.ActiveDashboardOnline);
			Map(x => x.ActiveWizardOffline);
			Map(x => x.ActiveWizardOnline);
			Map(x => x.PriorityOffline).Nullable();
			Map(x => x.PriorityOnline).Nullable();
			Map(x => x.Ribbon).Length(50).Nullable();
			Map(x => x.MandatoryOnline);
			Map(x => x.MandatoryOffline);
			Map(x => x.IsPaymentAccount);
			References(x => x.Group, "GroupId");

			DiscriminateSubClassesOnColumn("").Formula(
				"CASE Name " +
				Integration.ChannelGrabberConfig.Configuration.Instance.GetMarketplaceDiscriminator() +
				" ELSE Name END"
			);
		}
	}

	public class CompanyFilesMarketPlaceType : MP_MarketplaceType {
		public override int UWPriority { get { return 3; } }
	}

	public class CompanyFilesMarketPlaceTypeMap : SubclassMap<CompanyFilesMarketPlaceType> {
		public CompanyFilesMarketPlaceTypeMap() {
			DiscriminatorValue("CompanyFiles");
		}
	}
} // namespace EZBob.DatabaseLib.Model.Database
