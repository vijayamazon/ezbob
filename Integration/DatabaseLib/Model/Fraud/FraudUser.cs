using FluentNHibernate.Mapping;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Fraud
{
    public class FraudUser
    {
        private ISet<FraudAddress> _address = new HashedSet<FraudAddress>();
        private ISet<FraudBankAccount> _bankAccounts = new HashedSet<FraudBankAccount>();
        private ISet<FraudCompany> _companies = new HashedSet<FraudCompany>();
        private ISet<FraudEmail> _emails = new HashedSet<FraudEmail>();
        private ISet<FraudEmailDomain> _domains = new HashedSet<FraudEmailDomain>();
        private ISet<FraudPhone> _phones = new HashedSet<FraudPhone>();
        private ISet<FraudShop> _shops = new HashedSet<FraudShop>();

        public virtual int Id { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }

        public virtual ISet<FraudBankAccount> BankAccounts
        {
            get { return _bankAccounts; }
            set { _bankAccounts = value; }
        }
        public virtual ISet<FraudAddress> Addresses
        {
            get { return _address; }
            set { _address = value; }
        }

        public virtual ISet<FraudCompany> Companies
        {
            get { return _companies; }
            set { _companies = value; }
        }

        public virtual ISet<FraudEmail> Emails
        {
            get { return _emails; }
            set { _emails = value; }
        }

        public virtual ISet<FraudEmailDomain> EmailDomains
        {
            get { return _domains; }
            set { _domains = value; }
        }

        public virtual ISet<FraudPhone> Phones
        {
            get { return _phones; }
            set { _phones = value; }
        }

        public virtual ISet<FraudShop> Shops
        {
            get { return _shops; }
            set { _shops = value; }
        }
    }

    public sealed class FraudUserMap : ClassMap<FraudUser>
    {
        public FraudUserMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("100").Column("Id");
            Map(x => x.FirstName).Length(100);
            Map(x => x.LastName).Length(100);
            HasMany(m => m.BankAccounts)
                .AsSet()
                .KeyColumn("FraudUserId")
                .OrderBy("Id")
                .Inverse()
                .Cascade.All();
            HasMany(m => m.Addresses)
                .AsSet()
                .KeyColumn("FraudUserId")
                .OrderBy("Id")
                .Inverse()
                .Cascade.All();
            HasMany(m => m.Companies)
                .AsSet()
                .KeyColumn("FraudUserId")
                .OrderBy("Id")
                .Inverse()
                .Cascade.All();
            HasMany(m => m.Emails)
                .AsSet()
                .KeyColumn("FraudUserId")
                .OrderBy("Id")
                .Inverse()
                .Cascade.All();
            HasMany(m => m.EmailDomains)
                .AsSet()
                .KeyColumn("FraudUserId")
                .OrderBy("Id")
                .Inverse()
                .Cascade.All();
            HasMany(m => m.Phones)
                .AsSet()
                .KeyColumn("FraudUserId")
                .OrderBy("Id")
                .Inverse()
                .Cascade.All();
            HasMany(m => m.Shops)
                .AsSet()
                .KeyColumn("FraudUserId")
                .OrderBy("Id")
                .Inverse()
                .Cascade.All();
        }
    }
}
