﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EzBob.AmazonLib;
using EzBob.PayPal;
using EzBob.eBayLib;
using NHibernate;
using NHibernate.Linq;
using StructureMap;

namespace EzBob.Web.Areas.Customer.Models
{
    public static class CustomerExtensions
    {
        public static IEnumerable<SimpleMarketPlaceModel> GetEbayMarketPlaces(this EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            var marketplaces = GetEbayCustomerMarketPlaces(customer).Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return marketplaces;
        }

        public static IEnumerable<MP_CustomerMarketPlace> GetEbayCustomerMarketPlaces(this EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            var ebay = new eBayDatabaseMarketPlace();
            return customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == ebay.InternalId);
        }

        public static IEnumerable<MP_CustomerMarketPlace> GetPayPalCustomerMarketPlaces(this EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            var paypal = new PayPalDatabaseMarketPlace();
            return customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == paypal.InternalId);
        }

        public static IEnumerable<SimpleMarketPlaceModel> GetPayPalAccountsSimple(this EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            return GetPayPalCustomerMarketPlaces(customer).ToList().Select(m => new SimpleMarketPlaceModel() { displayName = m.DisplayName });
        }

        public static IEnumerable<PayPalModel> GetPayPalAccounts(this EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            var marketpalces = GetPayPalCustomerMarketPlaces(customer).ToList();
            var res = new List<PayPalModel>();
            foreach (var m in marketpalces)
            {
                var values = RetrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace(m.Id);
                var analisysFunction = values;
                var av = values.Data.FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;

                var tnop = 0.0;
                var tnip = 0.0;
                var tc = 0;
                if (av != null)
                {
                    var tnipN = av.FirstOrDefault(x => x.ParameterName == "Total Net In Payments" && x.CountMonths == av.Max(i => i.CountMonths));
                    var tnopN = av.FirstOrDefault(x => x.ParameterName == "Total Net Out Payments" && x.CountMonths == av.Max(i => i.CountMonths));
                    var tcN = av.FirstOrDefault(x => x.ParameterName == "Transactions Number" && x.CountMonths == av.Max(i => i.CountMonths));

                    if (tnipN != null) tnip = Math.Abs(Convert.ToDouble(tnipN.Value, CultureInfo.InvariantCulture));
                    if (tnopN != null) tnop = Math.Abs(Convert.ToDouble(tnopN.Value, CultureInfo.InvariantCulture));
                    if (tcN != null) tc = Convert.ToInt32(tcN.Value, CultureInfo.InvariantCulture);
                }

                var transactionsMinDate = m.PayPalTransactions.Any() ?
                    m.PayPalTransactions.Min(x => x.TransactionItems.Any() ? x.TransactionItems.Min(y => y.Created) : DateTime.Now) :
                    DateTime.Now;

                var seniority = DateTime.Now - transactionsMinDate;

                var status =
                        (m.UpdatingStart != null && m.UpdatingEnd == null) ? "In progress" :
                        (!String.IsNullOrEmpty(m.UpdateError)) ? "Error" :
                        "Done";

                res.Add(new PayPalModel
                    {
                        displayName = m.DisplayName, 
                        TotalNetInPayments = tnip, 
                        TotalNetOutPayments = tnop, 
                        TranzactionsNumber = tc, 
                        id = m.Id, 
                        Seniority = (seniority.Days * 1.0 / 30).ToString(CultureInfo.InvariantCulture),
                        Status = status
                    });
            }
            return res;
        }

        public static IEnumerable<SimpleMarketPlaceModel> GetAmazonMarketPlaces(this EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            var marketplaces = GetAmazonMP(customer);

            var simpleMarketPlaceModels = marketplaces.Select((m) => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return simpleMarketPlaceModels;
        }

        public static IEnumerable<SimpleMarketPlaceModel> GetEkmShops(this EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Marketplace.Id == 4);
            var simpleMarketPlaceModels = marketplaces.Select((m) => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return simpleMarketPlaceModels;
        }

        public static List<MP_CustomerMarketPlace> GetAmazonMP(this EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            var amazon = new AmazonDatabaseMarketPlace();

            var marketplaces = customer.CustomerMarketPlaces
                .Where(mp => mp.Marketplace.InternalId == amazon.InternalId)
                .ToList();
            return marketplaces;
        }
    }
}