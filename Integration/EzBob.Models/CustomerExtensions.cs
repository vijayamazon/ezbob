namespace EzBob.Web.Areas.Customer.Models
{
    using PayPoint;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using EKM;
    using EZBob.DatabaseLib.Model.Database;
    using AmazonLib;
    using PayPal;
    using eBayLib;
    using Integration.Volusion;
    using Integration.Play;

    public static class CustomerExtensions
    {
        public static IEnumerable<SimpleMarketPlaceModel> GetEbayMarketPlaces(this Customer customer)
        {
            var marketplaces = GetEbayCustomerMarketPlaces(customer).Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return marketplaces;
        }

        public static IEnumerable<MP_CustomerMarketPlace> GetEbayCustomerMarketPlaces(this Customer customer)
        {
            var ebay = new eBayDatabaseMarketPlace();
            return customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == ebay.InternalId);
        }

        public static IEnumerable<MP_CustomerMarketPlace> GetPayPalCustomerMarketPlaces(this Customer customer)
        {
            var paypal = new PayPalDatabaseMarketPlace();
            return customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == paypal.InternalId);
        }

        public static IEnumerable<SimpleMarketPlaceModel> GetPayPalAccountsSimple(this Customer customer)
        {
            return GetPayPalCustomerMarketPlaces(customer).ToList().Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
        }

        public static IEnumerable<PayPalModel> GetPayPalAccounts(this Customer customer)
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

        public static IEnumerable<SimpleMarketPlaceModel> GetAmazonMarketPlaces(this Customer customer)
        {
            var marketplaces = GetAmazonMP(customer);

            var simpleMarketPlaceModels = marketplaces.Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return simpleMarketPlaceModels;
        }

        public static IEnumerable<SimpleMarketPlaceModel> GetEkmShops(this Customer customer) {
	        var oEsi = new EkmServiceInfo();
            var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == oEsi.InternalId);
            var simpleMarketPlaceModels = marketplaces.Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return simpleMarketPlaceModels;
        }

        public static IEnumerable<SimpleMarketPlaceModel> GetVolusionShops(this Customer customer) {
	        var oVsi = new VolusionServiceInfo();
            var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == oVsi.InternalId);
            var simpleMarketPlaceModels = marketplaces.Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return simpleMarketPlaceModels;
        } // GetVolusionShops

        public static IEnumerable<SimpleMarketPlaceModel> GetPlayShops(this Customer customer) {
	        var oVsi = new PlayServiceInfo();
            var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == oVsi.InternalId);
            var simpleMarketPlaceModels = marketplaces.Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return simpleMarketPlaceModels;
        } // GetPlayShops

        public static IEnumerable<SimpleMarketPlaceModel> GetPayPointAccounts(this Customer customer)
        {
            var payPointServiceInfo = new PayPointServiceInfo();
            var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == payPointServiceInfo.InternalId);
            var simpleMarketPlaceModels = marketplaces.Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return simpleMarketPlaceModels;
        }

        public static List<MP_CustomerMarketPlace> GetAmazonMP(this Customer customer)
        {
            var amazon = new AmazonDatabaseMarketPlace();

            var marketplaces = customer.CustomerMarketPlaces
                .Where(mp => mp.Marketplace.InternalId == amazon.InternalId)
                .ToList();
            return marketplaces;
        }
    }
}