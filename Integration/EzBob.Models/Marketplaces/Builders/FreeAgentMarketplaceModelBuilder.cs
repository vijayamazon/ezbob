namespace EzBob.Models.Marketplaces.Builders
{
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Marketplaces.FreeAgent;
	using Marketplaces;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Web.Areas.Customer.Models;
	using Web.Areas.Underwriter.Models;

	class FreeAgentMarketplaceModelBuilder : MarketplaceModelBuilder
    {
		private readonly Dictionary<string, FreeAgentExpenseCategory> expenseCategories = new Dictionary<string, FreeAgentExpenseCategory>();

		public FreeAgentMarketplaceModelBuilder(MP_FreeAgentExpenseCategoryRepository freeAgentExpenseCategoryRepository, CustomerMarketPlaceRepository customerMarketplaces)
            : base(customerMarketplaces)
        {
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

				expenseCategories.Add(category.url, category);
			}
        }

        public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model)
        {
			var paymentAccountModel = new PaymentAccountsModel();
			MP_AnalyisisFunctionValue earliestNumOfExpenses = GetEarliestValueFor(mp, "NumOfExpenses");
			MP_AnalyisisFunctionValue earliestNumOfInvoices = GetEarliestValueFor(mp, "NumOfOrders");
			MP_AnalyisisFunctionValue earliestSumOfExpenses = GetEarliestValueFor(mp, "TotalSumOfExpenses");
			MP_AnalyisisFunctionValue earliestSumOfInvoices = GetEarliestValueFor(mp, "TotalSumOfOrders");

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

        protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model)
        {
            model.FreeAgent = BuildFreeAgent(mp);
        }

		private FreeAgentModel BuildFreeAgent(MP_CustomerMarketPlace mp)
		{
			var model = new FreeAgentModel
				{
					Expenses = mp.FreeAgentRequests.SelectMany(freeAgentRequest => freeAgentRequest.Expenses).Select(o => new FreeAgentExpense
						{
							url = o.url,
							user = o.username,
							category = o.category,
							dated_on = o.dated_on,
							currency = o.currency,
							gross_value = o.gross_value,
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
							categoryItem = expenseCategories.ContainsKey(o.category) ? expenseCategories[o.category] :
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
						}).Distinct(new FreeAgentExpenseComparer()),
					Invoices = mp.FreeAgentRequests.SelectMany(freeAgentRequest => freeAgentRequest.Invoices).Select(o => new FreeAgentInvoice
						{
							url = o.url,
							contact = o.contact,
							dated_on = o.dated_on,
							due_on = o.due_on,
							reference = o.reference,
							currency = o.currency,
							exchange_rate = o.exchange_rate,
							net_value = o.net_value,
							total_value = o.total_value,
							paid_value = o.paid_value,
							due_value = o.due_value,
							status = o.status,
							omit_header = o.omit_header,
							payment_terms_in_days = o.payment_terms_in_days,
							paid_on = o.paid_on
						}).Distinct(new FreeAgentInvoiceComparer())
				};

			return model;
		}
    }
}