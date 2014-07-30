#region namespace EZBob.DatabaseLib.Model.Database

namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using Iesi.Collections.Generic;

	#region class Director

	public class Director {
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Middle { get; set; }
		public virtual string Surname { get; set; }
		public virtual DateTime? DateOfBirth { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual Gender Gender { get; set; }
		public virtual DirectorAddressInfo DirectorAddressInfo { get; set; }
		public virtual string Email { get; set; }
		public virtual string Phone { get; set; }
		public virtual Company Company { get; set; }
		public virtual bool? IsShareholder { get; set; }
		public virtual bool? IsDirector { get; set; }
		public virtual int? ExperianConsumerScore { get; set; }
	} // class Director

	#endregion class Director

	#region class DirectorAddressInfo

	public class DirectorAddressInfo {
		#region property NonLimitedDirectorHomeAddressPrev

		public virtual ISet<CustomerAddress> NonLimitedDirectorHomeAddressPrev {
			get { return m_oNonLimitedDirectorHomeAddressPrev; }
			set { m_oNonLimitedDirectorHomeAddressPrev = value; }
		} // NonLimitedDirectorHomeAddressPrev

		private ISet<CustomerAddress> m_oNonLimitedDirectorHomeAddressPrev = new HashedSet<CustomerAddress>();

		#endregion property NonLimitedDirectorHomeAddressPrev

		#region property LimitedDirectorHomeAddress

		public virtual ISet<CustomerAddress> LimitedDirectorHomeAddress {
			get { return m_oLimitedDirectorHomeAddress; }
			set { m_oLimitedDirectorHomeAddress = value; }
		}  // LimitedDirectorHomeAddress

		private ISet<CustomerAddress> m_oLimitedDirectorHomeAddress = new HashedSet<CustomerAddress>();

		#endregion property LimitedDirectorHomeAddress

		#region property NonLimitedDirectorHomeAddress

		public virtual ISet<CustomerAddress> NonLimitedDirectorHomeAddress {
			get { return m_oNonLimitedDirectorHomeAddress; }
			set { m_oNonLimitedDirectorHomeAddress = value; }
		} // NonLimitedDirectorHomeAddress

		private ISet<CustomerAddress> m_oNonLimitedDirectorHomeAddress = new HashedSet<CustomerAddress>();

		#endregion property NonLimitedDirectorHomeAddress

		#region property LimitedDirectorHomeAddressPrev

		public ISet<CustomerAddress> LimitedDirectorHomeAddressPrev {
			get { return m_oLimitedDirectorHomeAddressPrev; }
			set { m_oLimitedDirectorHomeAddressPrev = value; }
		} // LimitedDirectorHomeAddressPrev

		private ISet<CustomerAddress> m_oLimitedDirectorHomeAddressPrev = new HashedSet<CustomerAddress>();

		#endregion property LimitedDirectorHomeAddressPrev

		#region property AllAddresses

		public virtual ISet<CustomerAddress> AllAddresses {
			get { return m_oAllAddresses; }
			set { m_oAllAddresses = value; }
		} // AllAddresses

		private ISet<CustomerAddress> m_oAllAddresses = new HashedSet<CustomerAddress>();

		#endregion property AllAddresses
	} // class DirectorAddressInfo

	#endregion class DirectorAddressInfo
} // namespace

#endregion namespace EZBob.DatabaseLib.Model.Database

#region namespace EZBob.DatabaseLib.Model.Database.Mapping

namespace EZBob.DatabaseLib.Model.Database.Mapping {
	using System;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	#region class DirectorModelMap

	public class DirectorModelMap : ClassMap<Director> {
		public DirectorModelMap() {
			Table("Director");

			Cache.ReadWrite().Region("LongTerm").ReadWrite();

			Id(x => x.Id);

			Map(x => x.Name).Length(512);
			Map(x => x.Middle).Length(512);
			Map(x => x.Surname).Length(512);
			Map(x => x.DateOfBirth);
			Map(x => x.Gender).CustomType<GenderType>();
			References(x => x.Customer, "CustomerId");
			References(x => x.Company, "CompanyId");
			Map(x => x.Email);
			Map(x => x.Phone);
			Map(x => x.IsShareholder);
			Map(x => x.IsDirector);
			Map(x => x.ExperianConsumerScore);

			Component(x => x.DirectorAddressInfo, m => {
				m.HasMany(x => x.LimitedDirectorHomeAddressPrev)
					.AsSet()
					.KeyColumn("directorId")
					.Where("addressType=" + Convert.ToInt32(CustomerAddressType.LimitedDirectorHomeAddressPrev))
					.Cascade.All()
					.Inverse()
					.Cache.ReadWrite().Region("LongTerm").ReadWrite();

				m.HasMany(x => x.NonLimitedDirectorHomeAddressPrev)
					.AsSet()
					.KeyColumn("directorId")
					.Where("addressType=" + Convert.ToInt32(CustomerAddressType.NonLimitedDirectorHomeAddressPrev))
					.Cascade.All()
					.Inverse()
					.Cache.ReadWrite().Region("LongTerm").ReadWrite();

				m.HasMany(x => x.LimitedDirectorHomeAddress)
					.AsSet()
					.KeyColumn("directorId")
					.Where("addressType=" + Convert.ToInt32(CustomerAddressType.LimitedDirectorHomeAddress))
					.Cascade.All()
					.Inverse()
					.Cache.ReadWrite().Region("LongTerm").ReadWrite();

				m.HasMany(x => x.NonLimitedDirectorHomeAddress)
					.AsSet()
					.KeyColumn("directorId")
					.Where("addressType=" + Convert.ToInt32(CustomerAddressType.NonLimitedDirectorHomeAddress))
					.Cascade.All()
					.Inverse()
					.Cache.ReadWrite().Region("LongTerm").ReadWrite();

				m.HasMany(x => x.AllAddresses)
					.AsSet()
					.KeyColumn("directorId")
					.Cascade.All()
					.Inverse()
					.Cache.ReadWrite().Region("LongTerm").ReadWrite();
			});
		} // constructor
	} // class DirectorModelMap

	#endregion class DirectorModelMap

	#region class DirectorRepository

	public class DirectorRepository : NHibernateRepositoryBase<Director> {
		public DirectorRepository(ISession session) : base(session) {
		} // constructor
	} // class DirectorRepository

	#endregion class DirectorRepository
} // namespace

#endregion namespace EZBob.DatabaseLib.Model.Database.Mapping