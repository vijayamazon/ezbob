using System;
using System.Linq;
using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Loans
{
    public class DiscountPlan
    {
        private decimal[] _discounts = new decimal[0];
        private string _valuesStr;
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string ValuesStr {
            get { return _valuesStr; }
            set 
            { 
                _valuesStr = value;
                _discounts = GetDiscounts();
            }
        }

        private decimal[] GetDiscounts()
        {
            if (string.IsNullOrEmpty(ValuesStr)) return new decimal[]{};
            var corrected = ValuesStr.Replace("+", "");
            return corrected.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => decimal.Parse(x)).ToArray();
        }

        public virtual bool IsDefault { get; set; }
        public virtual decimal[] Discounts
        {
            get
            {
                return _discounts;
            }
        }

        public virtual string NameWithPercents
        {
            get
            {
                if (Discounts.Length == 0 || Discounts.All(d => d == 0) || Name == "No Discount") return Name;
                return string.Format("{0}  ({1})", Name, ValuesStr);
            }
        }
    }

    public interface IDiscountPlanRepository : IRepository<DiscountPlan>
    {
        DiscountPlan GetDefault();
    }

    public class DiscountPlanRepository : NHibernateRepositoryBase<DiscountPlan>, IDiscountPlanRepository
    {
        public DiscountPlanRepository(ISession session)
            : base(session)
        {
        }

        public DiscountPlan GetDefault()
        {
            return GetAll().Single(p => p.IsDefault);
        }
    }

    public class DiscountPlanMap : ClassMap<DiscountPlan>
    {
        public DiscountPlanMap()
        {
            Id(x => x.Id).GeneratedBy.Native();
            Map(x => x.Name).Length(512);
            Map(x => x.ValuesStr).Length(2048);
            Map(x => x.IsDefault);
        }
    }
}