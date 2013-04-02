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

        private ISet<CustomerAddress> _addresses = new HashedSet<CustomerAddress>();
        public virtual ISet<CustomerAddress> DirectorAddress
        {
            get { return _addresses; }
            set { _addresses = value; }
        }

        public virtual Customer Customer { get; set; }
        public virtual Gender Gender { get; set; }
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
        }
    }

}