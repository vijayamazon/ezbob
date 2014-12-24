using System;
using EZBob.DatabaseLib.Model.Database;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model
{
    public class PayPointCard
    {
        public virtual int Id { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual DateTime DateAdded { get; set; }
        public virtual string TransactionId { get; set; }
        public virtual string CardNo { get; set; }
        public virtual DateTime? ExpireDate { get; set; }
        public virtual string ExpireDateString { get; set; }
        public virtual string CardHolder { get; set; }
		public virtual bool IsDefaultCard { get; set; }
		public virtual PayPointAccount PayPointAccount { get; set; }
    }

    public class PayPointCardMap : ClassMap<PayPointCard>
    {
        public PayPointCardMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("100");
            References(x => x.Customer, "CustomerId");
            Map(x => x.DateAdded);
            Map(x => x.TransactionId).Length(250);
            Map(x => x.CardNo).Length(50);
            Map(x => x.ExpireDate);
            Map(x => x.ExpireDateString).Length(50);
            Map(x => x.CardHolder).Length(150);
	        Map(x => x.IsDefaultCard);
	        References(x => x.PayPointAccount, "PayPointAccountID");
        }
    }

}