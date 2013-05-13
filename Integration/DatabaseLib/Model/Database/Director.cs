using System;
using FluentNHibernate.Mapping;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
    using ApplicationMng.Model;
    public class Director
    {
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }
        public virtual string Middle { get; set; }
        public virtual string Surname { get; set; }

        public virtual DateTime? DateOfBirth { get; set; }

        private ISet<CustomerAddress> _addresses = new HashedSet<CustomerAddress>();

        public virtual ISet<CustomerAddress> DirectorAddress
        {
            get { return _addresses; }
            set { _addresses = value; }
        }

        public virtual Customer Customer { get; set; }
        public virtual Gender Gender { get; set; }
        public virtual DirectorAddressInfo DirectorAddressInfo { get; set; }
    }

    public class DirectorAddressInfo
    {
        private ISet<CustomerAddress> _nonLimitedDirectorHomeAddressPrev = new HashedSet<CustomerAddress>();
        public virtual ISet<CustomerAddress> NonLimitedDirectorHomeAddressPrev
        {
            get { return _nonLimitedDirectorHomeAddressPrev; }
            set { _nonLimitedDirectorHomeAddressPrev = value; }
        }

        private ISet<CustomerAddress> _limitedDirectorHomeAddress = new HashedSet<CustomerAddress>();
        public virtual ISet<CustomerAddress> LimitedDirectorHomeAddress
        {
            get { return _limitedDirectorHomeAddress; }
            set { _limitedDirectorHomeAddress = value; }
        }

        private ISet<CustomerAddress> _nonLimitedDirectorHomeAddress = new HashedSet<CustomerAddress>();
        public virtual ISet<CustomerAddress> NonLimitedDirectorHomeAddress
        {
            get { return _nonLimitedDirectorHomeAddress; }
            set { _nonLimitedDirectorHomeAddress = value; }
        }

        private ISet<CustomerAddress> _limitedDirectorHomeAddressPrev = new HashedSet<CustomerAddress>();
        public ISet<CustomerAddress> LimitedDirectorHomeAddressPrev
        {
            get { return _limitedDirectorHomeAddressPrev; }
            set { _limitedDirectorHomeAddressPrev = value; }
        }
    }
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
    public class DirectorModelMap: ClassMap<Director>
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
            HasManyToMany(x => x.DirectorAddress)
                                .AsSet()
                                .Cascade.All()
                                .Table("DirectorAddressRelation")
                                .ParentKeyColumn("DirectorId")
                                .ChildKeyColumn("addressId");

            Component(x => x.DirectorAddressInfo, m =>
            {
                m.HasManyToMany(x => x.LimitedDirectorHomeAddressPrev)
                    .AsSet()
                    .Cascade.All()
                    .Table("DirectorAddressRelation")
                    .ParentKeyColumn("DirectorId")
                    .ChildKeyColumn("addressId")
                    .ChildWhere("addressType=" + Convert.ToInt32(ApplicationMng.Model.AddressType.LimitedDirectorHomeAddressPrev))
                    .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                m.HasManyToMany(x => x.NonLimitedDirectorHomeAddressPrev)
                    .AsSet()
                    .Cascade.All()
                    .Table("DirectorAddressRelation")
                    .ParentKeyColumn("DirectorId")
                    .ChildKeyColumn("addressId")
                    .ChildWhere("addressType=" + Convert.ToInt32(ApplicationMng.Model.AddressType.NonLimitedDirectorHomeAddressPrev))
                    .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                m.HasManyToMany(x => x.LimitedDirectorHomeAddress)
                    .AsSet()
                    .Cascade.All()
                    .Table("DirectorAddressRelation")
                    .ParentKeyColumn("DirectorId")
                    .ChildKeyColumn("addressId")
                    .ChildWhere("addressType=" + Convert.ToInt32(ApplicationMng.Model.AddressType.LimitedDirectorHomeAddress))
                    .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                m.HasManyToMany(x => x.NonLimitedDirectorHomeAddress)
                    .AsSet()
                    .Cascade.All()
                    .Table("DirectorAddressRelation")
                    .ParentKeyColumn("DirectorId")
                    .ChildKeyColumn("addressId")
                    .ChildWhere("addressType=" + Convert.ToInt32(ApplicationMng.Model.AddressType.NonLimitedDirectorHomeAddress))
                    .Cache.ReadWrite().Region("LongTerm").ReadWrite();
            });
        }
    }
}