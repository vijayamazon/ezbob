﻿namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	using System.Collections.Generic;
	using System.Linq;
	using Model.Marketplaces.Sage;

	public class SageSalesInvoicesConverter
	{
		public static IEnumerable<SageSalesInvoice> GetSageSalesInvoices(IOrderedEnumerable<MP_SageSalesInvoice> dbSalesInvoices)
		{
			var invoices = new List<SageSalesInvoice>(dbSalesInvoices.Select(o => new SageSalesInvoice
				{
					SageId = o.SageId,
					invoice_number = o.invoice_number,
					status = o.StatusId,
					due_date = o.due_date,
					date = o.date,
					void_reason = o.void_reason,
					outstanding_amount = o.outstanding_amount,
					total_net_amount = o.total_net_amount,
					total_tax_amount = o.total_tax_amount,
					tax_scheme_period_id = o.tax_scheme_period_id,
					carriage = o.carriage,
					carriage_tax_code = o.CarriageTaxCodeId,
					carriage_tax_rate_percentage = o.carriage_tax_rate_percentage,
					contact = o.ContactId,
					contact_name = o.contact_name,
					main_address = o.main_address,
					delivery_address = o.delivery_address,
					delivery_address_same_as_main = o.delivery_address_same_as_main,
					reference = o.reference,
					notes = o.notes,
					terms_and_conditions = o.terms_and_conditions,
					lock_version = o.lock_version,
					line_items = GetSageInvoiceItems(o.Items)
				}));

			return invoices;
		}

		private static List<SageSalesInvoiceItem> GetSageInvoiceItems(IEnumerable<MP_SageSalesInvoiceItem> items)
		{
			var result = new List<SageSalesInvoiceItem>();
			foreach (MP_SageSalesInvoiceItem item in items)
			{
				result.Add(new SageSalesInvoiceItem
				{
					SageId = item.SageId,
					description = item.description,
					quantity = item.quantity,
					unit_price = item.unit_price,
					net_amount = item.net_amount,
					tax_amount = item.tax_amount,
					tax_code = item.TaxCodeId,
					tax_rate_percentage = item.tax_rate_percentage,
					unit_price_includes_tax = item.unit_price_includes_tax,
					ledger_account = item.LedgerAccountId,
					product_code = item.product_code,
					product = item.ProductId,
					service = item.ServiceKey,
					lock_version = item.lock_version
				});
			}

			return result;
		}

		public static IEnumerable<SageIncome> GetSageIncomes(IOrderedEnumerable<MP_SageIncome> dbIncomes)
		{
			var incomes = new List<SageIncome>(dbIncomes.Select(o => new SageIncome
			{
				SageId = o.SageId,
				date = o.date,
				invoice_date = o.invoice_date,
				amount = o.amount,
				tax_amount = o.tax_amount,
				gross_amount = o.gross_amount,
				tax_percentage_rate = o.tax_percentage_rate,
				tax_code = o.TaxCodeId,
				tax_scheme_period_id = o.tax_scheme_period_id,
				reference = o.reference,
				contact = o.ContactId,
				source = o.SourceId,
				destination = o.DestinationId,
				payment_method = o.PaymentMethodId,
				voided = o.voided,
				lock_version = o.lock_version
			}));

			return incomes;
		}
	}
}