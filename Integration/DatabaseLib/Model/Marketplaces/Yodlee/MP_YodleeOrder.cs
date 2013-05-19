﻿using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using Iesi.Collections.Generic;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Database
{
    public class MP_YodleeOrder
    {
        public MP_YodleeOrder()
        {
            OrderItems = new HashedSet<MP_YodleeOrderItem>();
        }

        public virtual int Id { get; set; }
        public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
        public virtual DateTime Created { get; set; }

        public virtual Iesi.Collections.Generic.ISet<MP_YodleeOrderItem> OrderItems { get; set; }

        public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
    }

    public interface IMP_YodleeOrderRepository : IRepository<MP_YodleeOrder>
    {

    }

    public class MP_YodleeOrderRepository : NHibernateRepositoryBase<MP_YodleeOrder>, IMP_YodleeOrderRepository
    {
        public MP_YodleeOrderRepository(ISession session)
            : base(session)
        {
        }

        public List<MP_YodleeOrderItem> GetOrdersItemsByMakretplaceId(int marketplaceId)
        {
            return _session
                .Query<MP_YodleeOrderItem>()
                .Where(oi => oi.Order.CustomerMarketPlace.Id == marketplaceId)
                .FetchMany(oi => oi.OrderItemBankTransactions)
                .ToList();
        }


    }

    public class MP_YodleeBankTransactionsRepository : NHibernateRepositoryBase<MP_YodleeOrderItemBankTransaction>, IMP_YodleeBankTransactionsRepository
    {
        public MP_YodleeBankTransactionsRepository(ISession session)
            : base(session)
        {
        }

        public List<MP_YodleeOrderItemBankTransaction> GetAllItemsWithItemsID(string itemID)
        {
            return _session
                .Query<MP_YodleeOrderItemBankTransaction>()
                .Where(t => t.transactionId == itemID).ToList();
        }
    }

    public interface IMP_YodleeBankTransactionsRepository : IRepository<MP_YodleeOrderItemBankTransaction>
    {
    }
}
