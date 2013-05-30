using System.Collections.Generic;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Areas.Underwriter.Models;
using YodleeLib.connector;

namespace EzBob.Models
{
	using NHibernate;
	using Scorto.NHibernate.Repository;

	class YodleeMarketplaceModelBuilder : MarketplaceModelBuilder
    {
        private readonly MP_YodleeOrderRepository _yodleeOrderRepository;
        private readonly ISession _session;

        public YodleeMarketplaceModelBuilder(MP_YodleeOrderRepository yodleeOrderRepository, CustomerMarketPlaceRepository customerMarketplaces, ISession session)
            : base(customerMarketplaces)
        {
            _yodleeOrderRepository = yodleeOrderRepository;
            _session = session;
        }

        public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model)
        {
            return new PaymentAccountsModel();
        }

        protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model)
        {
            model.Yodlee = BuildYodlee(mp);
        }

        private YodleeModel BuildYodlee(MP_CustomerMarketPlace mp)
        {
            List<MP_YodleeOrderItem> yodleeData = null;
            if (mp.Marketplace.InternalId == new YodleeServiceInfo().InternalId)
            {
                yodleeData = _yodleeOrderRepository.GetOrdersItemsByMakretplaceId(mp.Id);
            }

            if (yodleeData == null) return null;

            var model = new YodleeModel();
            var banks = new List<YodleeBankModel>();
            var _currencyConvertor = new CurrencyConvertor(new CurrencyRateRepository(_session));
            foreach (var bank in yodleeData)
            {
                var yodleeBankModel = new YodleeBankModel
                                          {
                                              customName = bank.customName,
                                              customDescription = bank.customDescription,
                                              isDeleted = bank.isDeleted.ToString(),
                                              accountNumber = bank.accountNumber,
                                              accountHolder = bank.accountHolder,
                                              availableBalance = bank.availableBalance.HasValue ? _currencyConvertor.ConvertToBaseCurrency(
                                                  bank.availableBalanceCurrency,
                                                  bank.availableBalance.Value,
                                                  bank.asOfDate).Value.ToString() : null,
                                              term = bank.term,
                                              accountName = bank.accountName,
                                              routingNumber = bank.routingNumber,
                                              accountNicknameAtSrcSite = bank.accountNicknameAtSrcSite,
                                              secondaryAccountHolderName = bank.secondaryAccountHolderName,
                                              accountOpenDate = bank.accountOpenDate.ToString(),
                                              taxesWithheldYtd = bank.taxesWithheldYtd.ToString(),
                                          };
                var transactions = new List<YodleeTransactionModel>();
                foreach (var transaction in bank.OrderItemBankTransactions)
                {
                    var yodleeTransactionModel = new YodleeTransactionModel
                                                     {
                                                         transactionType = transaction.transactionType,
                                                         transactionStatus = transaction.transactionStatus,
                                                         transactionBaseType = transaction.transactionBaseType,
                                                         isDeleted = transaction.isDeleted.ToString(),
                                                         lastUpdated = transaction.lastUpdated.ToString(),
                                                         transactionId = transaction.transactionId,
                                                         transactionDate = transaction.transactionDate.ToString(),
                                                         runningBalance = transaction.runningBalance.ToString(),
                                                         userDescription = transaction.userDescription,
                                                         memo = transaction.memo,
                                                         category = transaction.category,
                                                         postDate = transaction.postDate.ToString(),
                                                         transactionAmount = transaction.transactionAmount.HasValue ? _currencyConvertor.ConvertToBaseCurrency(
                                                             transaction.transactionAmountCurrency,
                                                             transaction.transactionAmount.Value,
                                                             transaction.postDate ?? transaction.transactionDate).Value.ToString() : null,
                                                         description = transaction.description,
                                                     };
                    transactions.Add(yodleeTransactionModel);
                }
                yodleeBankModel.transactions = transactions;
                banks.Add(yodleeBankModel);
            }
            model.banks = banks;
            return model;
        }

    }
}