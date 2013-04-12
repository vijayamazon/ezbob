﻿using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Model;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Loans;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{

    public enum DecisionActions
    {
        Approve, Reject, Escalate, Pending, Waiting
    }

    public class CreditResultDecisionActionsType : EnumStringType<DecisionActions>
    {

    }

    public class DecisionHistory
    {
        public virtual int Id { get; set; }
        public virtual DecisionActions Action { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual string Comment { get; set; }
        public virtual User Underwriter { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual CashRequest CashRequest { get; set; }
        public virtual LoanType LoanType { get; set; }
    }

    public interface IDecisionHistoryRepository : IRepository<DecisionHistory>
    {
        void LogAction(DecisionActions action, string comment, User underwriter, Customer customer);
        IList<DecisionHistory> ByCustomer(Customer customer);
    }

    public class DecisionHistoryRepository : NHibernateRepositoryBase<DecisionHistory>, IDecisionHistoryRepository
    {
        public DecisionHistoryRepository(ISession session) : base(session)
        {
        }

        public void LogAction(DecisionActions action, string comment, User underwriter, Customer customer)
        {
            var cr = customer.LastCashRequest;
            var item = new DecisionHistory()
                           {
                               Date = DateTime.UtcNow,
                               Action = action,
                               Underwriter = underwriter,
                               Customer = customer,
                               Comment = comment,
                               CashRequest = cr,
                               LoanType = cr.LoanType
                           };
            Save(item);
        }

        public IList<DecisionHistory> ByCustomer(Customer customer)
        {
            return GetAll().Where(d => d.Customer.Id == customer.Id).ToList();
        }
    }

    public class DecisionHistoryMap : ClassMap<DecisionHistory>
    {
        public DecisionHistoryMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("100");
            Map(x => x.Date).CustomType<UtcDateTimeType>();
            Map(x => x.Comment).Length(2000);
            References(x => x.Underwriter, "UnderwriterId");
            References(x => x.Customer, "CustomerId");
            References(x => x.CashRequest, "CashRequestId");
            References(x => x.LoanType, "LoanTypeId");
            Map(x => x.Action).CustomType<CreditResultDecisionActionsType>();
        }
    }
}