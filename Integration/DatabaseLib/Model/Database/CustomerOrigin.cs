namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public enum CustomerOriginEnum {
		ezbob,
		everline,
		alibaba,
	} // enum CustomerOriginEnum

	public class CustomerOrigin {
		public virtual int CustomerOriginID { get; set; }
		public virtual string Name {
			get { return this.origin.ToString(); }
			set {
				if (!Enum.TryParse(value, true, out this.origin))
					this.origin = CustomerOriginEnum.ezbob;
			} // set
		} // Name
		public virtual string CustomerSite { get; set; }
		public virtual int SearchPriority { get; set; }
		public virtual string UrlNeedle { get; set; }
		public virtual string PhoneNumber { get; set; }
		public virtual string MetaDescription { get; set; }
		public virtual string CustomerCareEmail { get; set; }

		public virtual CustomerOriginEnum GetOrigin() { return this.origin; }

		private CustomerOriginEnum origin;
	} // class CustomerOrigin

	public class CustomerOriginMap : ClassMap<CustomerOrigin> {
		public CustomerOriginMap() {
			Table("CustomerOrigin");
			Id(x => x.CustomerOriginID);
			Map(x => x.Name).Length(20);
			Map(x => x.CustomerSite).Length(255);
			Map(x => x.SearchPriority);
			Map(x => x.UrlNeedle).Length(255);
			Map(x => x.PhoneNumber).Length(32);
			Map(x => x.CustomerCareEmail).Length(255);
			Map(x => x.MetaDescription);
		} // constructor
	} // class CustomerOriginMap

	public class CustomerOriginRepository : NHibernateRepositoryBase<CustomerOrigin> {
		public CustomerOriginRepository(ISession session) : base(session) {}

		public List<CustomerOrigin> GetAllOrdered() {
			return GetAll().OrderByDescending(x => x.SearchPriority).ToList();
		} // GetAllOrdered

		public CustomerOrigin GetByHostname(string hostname) {
			List<CustomerOrigin> all = GetAllOrdered();

			foreach (var co in all)
				if (hostname.Contains(co.UrlNeedle))
					return co;

			return GetDefault();
		} // GetbyHostname

		public CustomerOrigin GetDefault() {
			return GetAll().ToList().First(x => x.GetOrigin() == CustomerOriginEnum.ezbob);
		} // GetDefault
	} // class CustomerOriginRepository
} // namespace
