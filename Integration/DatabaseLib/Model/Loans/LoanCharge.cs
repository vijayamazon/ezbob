﻿using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Loans
{
    public class LoanCharge
    {
        public virtual int Id { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual decimal AmountPaid { get; set; }
        public virtual string State { get; set; }
        public virtual Database.Loans.Loan Loan { get; set; }
        public virtual ConfigurationVariable ChargesType { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual string Description { get; set; }

        public override string ToString()
        {
            return string.Format("Amount: {0}, Date: {1}", Amount, Date);
        }

        public virtual string GetDescription()
        {
            if (ChargesType == null) return Description;
            return string.IsNullOrEmpty(Description) ? ChargesType.Description : Description;
        }
    }

    public sealed class LoanChargesMap: ClassMap<LoanCharge>
    {
        public LoanChargesMap()
        {
            Id(x=>x.Id).GeneratedBy.HiLo("100");
            Table("LoanCharges");
            Map(x => x.Amount);
            Map(x => x.AmountPaid);
            Map(x => x.State, "`State`");
            References(x => x.ChargesType, "ConfigurationVariableId");
            References(x => x.Loan, "LoanId");
            Map(x => x.Date, "`Date`").CustomType<UtcDateTimeType>();
            Map(x => x.Description);
        }
    }
}

