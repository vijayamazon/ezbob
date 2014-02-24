namespace EzBob.Models.Marketplaces.Builders
{
	using System;
	using NHibernate;
	using NHibernate.Linq;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Marketplaces.FreeAgent;
	using Marketplaces;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using Scorto.NHibernate.Repository;
	using Web.Areas.Customer.Models;
	using Web.Areas.Underwriter.Models;

	class FreeAgentMarketplaceModelBuilder : MarketplaceModelBuilder
	{
		private readonly Dictionary<string, FreeAgentExpenseCategory> _expenseCategories = new Dictionary<string, FreeAgentExpenseCategory>();

		private readonly ICurrencyConvertor _currencyConverter;

		public FreeAgentMarketplaceModelBuilder(MP_FreeAgentExpenseCategoryRepository freeAgentExpenseCategoryRepository, CurrencyRateRepository currencyRateRepository, ISession session)
			: base(session)
		{
			_currencyConverter = new CurrencyConvertor(currencyRateRepository);

			foreach (MP_FreeAgentExpenseCategory dbCategory in freeAgentExpenseCategoryRepository.GetAll())
			{
				var category = new FreeAgentExpenseCategory
					{
						Id = dbCategory.Id,
						category_group = dbCategory.category_group,
						url = dbCategory.url,
						description = dbCategory.description,
						nominal_code = dbCategory.nominal_code,
						allowable_for_tax = dbCategory.allowable_for_tax,
						tax_reporting_name = dbCategory.tax_reporting_name,
						auto_sales_tax_rate = dbCategory.auto_sales_tax_rate
					};

				_expenseCategories.Add(category.url, category);
			}
		}

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			var paymentAccountModel = new PaymentAccountsModel();
			MP_AnalyisisFunctionValue earliestNumOfExpenses = GetEarliestValueFor(mp, "NumOfExpenses");
			MP_AnalyisisFunctionValue earliestNumOfInvoices = GetEarliestValueFor(mp, "NumOfOrders");
			MP_AnalyisisFunctionValue earliestSumOfExpenses = GetEarliestValueFor(mp, "TotalSumOfExpenses");
			MP_AnalyisisFunctionValue earliestSumOfInvoices = GetEarliestValueFor(mp, "TotalSumOfOrders");
			MP_AnalyisisFunctionValue monthSumOfInvoices = GetMonthValueFor(mp, "TotalSumOfOrders");

			if (monthSumOfInvoices != null && monthSumOfInvoices.ValueFloat.HasValue)
			{
				paymentAccountModel.MonthInPayments = monthSumOfInvoices.ValueFloat.Value;
			}

			if (earliestNumOfExpenses != null && earliestNumOfExpenses.ValueInt.HasValue &&
				earliestNumOfInvoices != null && earliestNumOfInvoices.ValueInt.HasValue)
			{
				paymentAccountModel.TransactionsNumber = earliestNumOfExpenses.ValueInt.Value + earliestNumOfInvoices.ValueInt.Value;
			}
			else
			{
				paymentAccountModel.TransactionsNumber = 0;
			}

			if (earliestSumOfInvoices != null && earliestSumOfInvoices.ValueFloat.HasValue)
			{
				paymentAccountModel.TotalNetInPayments = earliestSumOfInvoices.ValueFloat.Value;
			}
			else
			{
				paymentAccountModel.TotalNetInPayments = 0;
			}

			if (earliestSumOfExpenses != null && earliestSumOfExpenses.ValueFloat.HasValue)
			{
				paymentAccountModel.TotalNetOutPayments = earliestSumOfExpenses.ValueFloat.Value;
			}
			else
			{
				paymentAccountModel.TotalNetOutPayments = 0;
			}

			return paymentAccountModel;
		}
		
		protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			model.FreeAgent = BuildFreeAgent(mp);
		}

		private FreeAgentModel BuildFreeAgent(MP_CustomerMarketPlace mp)
		{
			var dbInvoices = mp.FreeAgentRequests.SelectMany(freeAgentRequest => freeAgentRequest.Invoices).OrderByDescending(invoice => invoice.Request.Id).Distinct(new FreeAgentInvoiceComparer()).OrderByDescending(invoice => invoice.dated_on);
			var dbExpenses = mp.FreeAgentRequests.SelectMany(freeAgentRequest => freeAgentRequest.Expenses).OrderByDescending(expense => expense.Request.Id).Distinct(new FreeAgentExpenseComparer()).OrderByDescending(expense => expense.dated_on);

			var model = new FreeAgentModel
				{
					Expenses = dbExpenses.Select(o => new FreeAgentExpense
					{
						url = o.url,
						user = o.username,
						category = o.category,
						dated_on = o.dated_on,
						currency = o.currency,
						gross_value = (decimal)_currencyConverter.ConvertToBaseCurrency(o.currency, (double)o.gross_value, o.dated_on).Value,
						native_gross_value = o.native_gross_value,
						sales_tax_rate = o.sales_tax_rate,
						sales_tax_value = o.sales_tax_value,
						native_sales_tax_value = o.native_sales_tax_value,
						description = o.description,
						manual_sales_tax_amount = o.manual_sales_tax_amount,
						updated_at = o.updated_at,
						created_at = o.created_at,
						attachment = new FreeAgentExpenseAttachment
						{
							url = o.attachment_url,
							content_src = o.attachment_content_src,
							content_type = o.attachment_content_type,
							file_name = o.attachment_file_name,
							file_size = o.attachment_file_size,
							description = o.attachment_description
						},
						categoryItem = _expenseCategories.ContainsKey(o.category) ? _expenseCategories[o.category] :
							new FreeAgentExpenseCategory
							{
								allowable_for_tax = false,
								auto_sales_tax_rate = string.Empty,
								category_group = string.Empty,
								description = string.Empty,
								Id = 0,
								nominal_code = string.Empty,
								tax_reporting_name = string.Empty,
								url = string.Empty
							}
					}),
					Invoices = dbInvoices.Select(o => new FreeAgentInvoice
					{
						url = o.url,
						contact = o.contact,
						dated_on = o.dated_on,
						due_on = o.due_on,
						reference = o.reference,
						currency = o.currency,
						exchange_rate = o.exchange_rate,
						net_value = (decimal)_currencyConverter.ConvertToBaseCurrency(o.currency, (double)o.net_value, o.dated_on).Value,
						total_value = (decimal)_currencyConverter.ConvertToBaseCurrency(o.currency, (double)o.total_value, o.dated_on).Value,
						paid_value = (decimal)_currencyConverter.ConvertToBaseCurrency(o.currency, (double)o.paid_value, o.dated_on).Value,
						due_value = o.due_value,
						status = o.status,
						omit_header = o.omit_header,
						payment_terms_in_days = o.payment_terms_in_days,
						paid_on = o.paid_on
					})
				};

			return model;
		}

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp)
		{
			var invoices = _session.Query<MP_FreeAgentInvoice>()
				.Where(oi => oi.Request.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.dated_on != null)
				.Select(oi => oi.dated_on);

			var expenses = _session.Query<MP_FreeAgentInvoice>()
				 .Where(oi => oi.Request.CustomerMarketPlace.Id == mp.Id)
				 .Where(oi => oi.dated_on != null)
				 .Select(oi => oi.dated_on);

			DateTime? earliest = null;
			if (invoices.Any())
			{
				earliest = invoices.Min();
			}
			if (expenses.Any() && expenses.Min() < earliest)
			{
				earliest = expenses.Min();
			}

			return earliest;
		}

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp)
		{
			var invoices = _session.Query<MP_FreeAgentInvoice>()
				.Where(oi => oi.Request.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.dated_on != null)
				.Max(oi => oi.dated_on);

			var expenses = _session.Query<MP_FreeAgentInvoice>()
				 .Where(oi => oi.Request.CustomerMarketPlace.Id == mp.Id)
				 .Where(oi => oi.dated_on != null)
				 .Max(oi => oi.dated_on);

			DateTime? latest = null;
			if (invoices.HasValue)
			{
				latest = invoices;
			}
			if ((expenses.HasValue && !latest.HasValue) || (expenses.HasValue && expenses.Value > latest))
			{
				latest = expenses;
			}

			return latest;
		}
	}
}