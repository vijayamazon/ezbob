using System;
using ApplicationMng.Model;
using FluentNHibernate.Mapping;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
    public class Director
    {
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
    }

    public class DirectorAddressInfo
    {
        private ISet<CustomerAddress> _limitedDirectorHomeAddress = new HashedSet<CustomerAddress>();
        private ISet<CustomerAddress> _limitedDirectorHomeAddressPrev = new HashedSet<CustomerAddress>();
        private ISet<CustomerAddress> _nonLimitedDirectorHomeAddress = new HashedSet<CustomerAddress>();
        private ISet<CustomerAddress> _nonLimitedDirectorHomeAddressPrev = new HashedSet<CustomerAddress>();
        private ISet<CustomerAddress> _allAddresses = new HashedSet<CustomerAddress>();

        public virtual ISet<CustomerAddress> NonLimitedDirectorHomeAddressPrev
        {
            get { return _nonLimitedDirectorHomeAddressPrev; }
            set { _nonLimitedDirectorHomeAddressPrev = value; }
        }

        public virtual ISet<CustomerAddress> LimitedDirectorHomeAddress
        {
            get { return _limitedDirectorHomeAddress; }
            set { _limitedDirectorHomeAddress = value; }
        }

        public virtual ISet<CustomerAddress> NonLimitedDirectorHomeAddress
        {
            get { return _nonLimitedDirectorHomeAddress; }
            set { _nonLimitedDirectorHomeAddress = value; }
        }

        public ISet<CustomerAddress> LimitedDirectorHomeAddressPrev
        {
            get { return _limitedDirectorHomeAddressPrev; }
            set { _limitedDirectorHomeAddressPrev = value; }
        }

        public virtual ISet<CustomerAddress> AllAddresses
        {
            get { return _allAddresses; }
            set { _allAddresses = value; }
        }
    }
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
    public class DirectorModelMap : ClassMap<Director>
    {
        public DirectorModelMap()
        {
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

            Component(x => x.DirectorAddressInfo, m =>
                {
                    m.HasMany(x => x.LimitedDirectorHomeAddressPrev)
                     .AsSet()
                     .KeyColumn("directorId")
                     .Where("addressType=" +
                            Convert.ToInt32(CustomerAddressType.LimitedDirectorHomeAddressPrev))
                     .Cascade.All()
                     .Inverse()
                     .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                    m.HasMany(x => x.NonLimitedDirectorHomeAddressPrev)
                     .AsSet()
                     .KeyColumn("directorId")
                     .Where("addressType=" +
                            Convert.ToInt32(CustomerAddressType.NonLimitedDirectorHomeAddressPrev))
                     .Cascade.All()
                     .Inverse()
                     .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                    m.HasMany(x => x.LimitedDirectorHomeAddress)
                     .AsSet()
                     .KeyColumn("directorId")
                     .Where("addressType=" +
                            Convert.ToInt32(CustomerAddressType.LimitedDirectorHomeAddress))
                     .Cascade.All()
                     .Inverse()
                     .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                    m.HasMany(x => x.NonLimitedDirectorHomeAddress)
                     .AsSet()
                     .KeyColumn("directorId")
                     .Where("addressType=" +
                            Convert.ToInt32(CustomerAddressType.NonLimitedDirectorHomeAddress))
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
        }
    }
}