namespace Sage
{
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using log4net;

	public class SageDesreializer
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SageDesreializer));

		public static SageSalesInvoice DeserializeSalesInvoice(SageSalesInvoiceDeserialization si)
		{
			DateTime due_date;
			if (!DateTime.TryParse(si.due_date, out due_date))
			{
				string msg = string.Format("Failed parsing due_date:{0}", si.due_date ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			DateTime date;
			if (!DateTime.TryParse(si.date, out date))
			{
				string msg = string.Format("Failed parsing date:{0}", si.date ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			decimal outstanding_amount, total_net_amount, total_tax_amount, carriage, carriage_tax_rate_percentage;
			if (!decimal.TryParse(si.outstanding_amount, out outstanding_amount))
			{
				string msg = string.Format("Failed parsing outstanding_amount:{0}", si.outstanding_amount ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			if (!decimal.TryParse(si.total_net_amount, out total_net_amount))
			{
				string msg = string.Format("Failed parsing total_net_amount:{0}", si.total_net_amount ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			if (!decimal.TryParse(si.total_tax_amount, out total_tax_amount))
			{
				string msg = string.Format("Failed parsing total_tax_amount:{0}", si.total_tax_amount ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			if (!decimal.TryParse(si.carriage, out carriage))
			{
				string msg = string.Format("Failed parsing carriage:{0}", si.carriage ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			if (!decimal.TryParse(si.carriage_tax_rate_percentage, out carriage_tax_rate_percentage))
			{
				string msg = string.Format("Failed parsing carriage_tax_rate_percentage:{0}", si.carriage_tax_rate_percentage ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			int tax_scheme_period_id;
			int.TryParse(si.tax_scheme_period_id, out tax_scheme_period_id);

			return new SageSalesInvoice
				{
					SageId = si.id,
					invoice_number = si.invoice_number,
					status = si.status == null ? null : si.status.key,
					due_date = due_date,
					date = date,
					void_reason = si.void_reason,
					outstanding_amount = outstanding_amount,
					total_net_amount = total_net_amount,
					total_tax_amount = total_tax_amount,
					tax_scheme_period_id = tax_scheme_period_id,
					carriage = carriage,
					carriage_tax_code = si.carriage_tax_code == null ? null : si.carriage_tax_code.key,
					carriage_tax_rate_percentage = carriage_tax_rate_percentage,
					contact = si.contact == null ? null : si.contact.key,
					contact_name = si.contact_name,
					main_address = si.main_address,
					delivery_address = si.delivery_address,
					delivery_address_same_as_main = si.delivery_address_same_as_main,
					reference = si.reference,
					notes = si.notes,
					terms_and_conditions = si.terms_and_conditions,
					lock_version = si.lock_version,
					line_items = GetSalesInvoiceItems(si.line_items)
				};
		}

		private static List<SageSalesInvoiceItem> GetSalesInvoiceItems(IEnumerable<SageInvoiceItemDeserialization> items)
		{
			var result = new List<SageSalesInvoiceItem>();
			foreach (SageInvoiceItemDeserialization si in items)
			{
				decimal quantity, unit_price, net_amount, tax_amount, tax_rate_percentage;
				if (!decimal.TryParse(si.quantity, out quantity)) return null;
				if (!decimal.TryParse(si.unit_price, out unit_price)) return null;
				if (!decimal.TryParse(si.net_amount, out net_amount)) return null;
				if (!decimal.TryParse(si.tax_amount, out tax_amount)) return null;
				if (!decimal.TryParse(si.tax_rate_percentage, out tax_rate_percentage)) return null;
				
				var deserialized = new SageSalesInvoiceItem
					{
						SageId = si.id,
						description = si.description,
						quantity = quantity,
						unit_price = unit_price,
						net_amount = net_amount,
						tax_amount = tax_amount,
						tax_code = si.tax_code == null ? null : si.tax_code.key,
						tax_rate_percentage = tax_rate_percentage,
						unit_price_includes_tax = si.unit_price_includes_tax,
						ledger_account = si.ledger_account == null ? null : si.ledger_account.key,
						product_code = si.product_code,
						product = si.product == null ? null : si.product.key,
						service = null, //removed because returned as null string
						lock_version = si.lock_version
					};
				result.Add(deserialized);
			}

			return result;
		}

		public static SagePurchaseInvoice DeserializePurchaseInvoice(SagePurchaseInvoiceDeserialization si)
		{
			DateTime due_date;
			if (!DateTime.TryParse(si.due_date, out due_date))
			{
				string msg = string.Format("Failed parsing due_date:{0}", si.due_date ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			DateTime date;
			if (!DateTime.TryParse(si.date, out date))
			{
				string msg = string.Format("Failed parsing date:{0}", si.date ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			decimal outstanding_amount, total_net_amount, total_tax_amount;
			if (!decimal.TryParse(si.outstanding_amount, out outstanding_amount))
			{
				string msg = string.Format("Failed parsing outstanding_amount:{0}", si.outstanding_amount ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			if (!decimal.TryParse(si.total_net_amount, out total_net_amount))
			{
				string msg = string.Format("Failed parsing total_net_amount:{0}", si.total_net_amount ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			if (!decimal.TryParse(si.total_tax_amount, out total_tax_amount))
			{
				string msg = string.Format("Failed parsing total_tax_amount:{0}", si.total_tax_amount ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			int tax_scheme_period_id;
			int.TryParse(si.tax_scheme_period_id, out tax_scheme_period_id);
			return new SagePurchaseInvoice
			{
				SageId = si.id,
				status = si.status == null ? null : si.status.key,
				due_date = due_date,
				date = date,
				void_reason = si.void_reason,
				outstanding_amount = outstanding_amount,
				total_net_amount = total_net_amount,
				total_tax_amount = total_tax_amount,
				tax_scheme_period_id = tax_scheme_period_id,
				contact = si.contact == null ? null : si.contact.key,
				contact_name = si.contact_name,
				main_address = si.main_address,
				delivery_address = si.delivery_address,
				delivery_address_same_as_main = si.delivery_address_same_as_main,
				reference = si.reference,
				notes = si.notes,
				terms_and_conditions = si.terms_and_conditions,
				lock_version = si.lock_version,
				line_items = GetPurchaseInvoiceItems(si.line_items)
			};
		}

		private static List<SagePurchaseInvoiceItem> GetPurchaseInvoiceItems(IEnumerable<SageInvoiceItemDeserialization> items)
		{
			var result = new List<SagePurchaseInvoiceItem>();
			foreach (SageInvoiceItemDeserialization si in items)
			{
				decimal quantity, unit_price, net_amount, tax_amount, tax_rate_percentage;
				if (!decimal.TryParse(si.quantity, out quantity)) return null;
				if (!decimal.TryParse(si.unit_price, out unit_price)) return null;
				if (!decimal.TryParse(si.net_amount, out net_amount)) return null;
				if (!decimal.TryParse(si.tax_amount, out tax_amount)) return null;
				if (!decimal.TryParse(si.tax_rate_percentage, out tax_rate_percentage)) return null;

				var deserialized = new SagePurchaseInvoiceItem
				{
					SageId = si.id,
					description = si.description,
					quantity = quantity,
					unit_price = unit_price,
					net_amount = net_amount,
					tax_amount = tax_amount,
					tax_code = si.tax_code == null ? null : si.tax_code.key,
					tax_rate_percentage = tax_rate_percentage,
					unit_price_includes_tax = si.unit_price_includes_tax,
					ledger_account = si.ledger_account == null ? null : si.ledger_account.key,
					product_code = si.product_code,
					product = si.product == null ? null : si.product.key,
					service = null,
					lock_version = si.lock_version
				};
				result.Add(deserialized);
			}

			return result;
		}

		public static SageIncome DeserializeIncome(SageIncomeDeserialization si)
		{
			DateTime date;
			if (!DateTime.TryParse(si.date, out date))
			{
				string msg = string.Format("Failed parsing date:{0}", si.date ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			DateTime? invoice_date;
			if (si.invoice_date == string.Empty)
			{
				invoice_date = null;
			}
			else
			{
				DateTime invoice_date_tmp;
				if (!DateTime.TryParse(si.invoice_date, out invoice_date_tmp))
				{
					string msg = string.Format("Failed parsing invoice_date:{0}", si.invoice_date ?? string.Empty);
					log.Error(msg);
					throw new Exception(msg);
				}

				invoice_date = invoice_date_tmp;
			}

			decimal amount, tax_amount, gross_amount, tax_percentage_rate;
			if (!decimal.TryParse(si.amount, out amount))
			{
				string msg = string.Format("Failed parsing amount:{0}", si.amount ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			if (!decimal.TryParse(si.tax_amount, out tax_amount))
			{
				string msg = string.Format("Failed parsing tax_amount:{0}", si.tax_amount ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			if (!decimal.TryParse(si.gross_amount, out gross_amount))
			{
				string msg = string.Format("Failed parsing gross_amount:{0}", si.gross_amount ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			if (!decimal.TryParse(si.tax_percentage_rate, out tax_percentage_rate))
			{
				string msg = string.Format("Failed parsing tax_percentage_rate:{0}", si.tax_percentage_rate ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}

			return new SageIncome
			{
				SageId = si.id,
				date = date,
				invoice_date = invoice_date,
				amount = amount,
				tax_amount = tax_amount,
				gross_amount = gross_amount,
				tax_percentage_rate = tax_percentage_rate,
				tax_code = si.tax_code == null ? null : si.tax_code.key,
				tax_scheme_period_id = si.tax_scheme_period_id,
				reference = si.reference,
				contact = si.contact == null ? null : si.contact.key,
				source = si.source == null ? null : si.source.key,
				destination = si.destination == null ? null : si.destination.key,
				//payment_method = si.payment_method == null ? null : si.payment_method.key,
				voided = si.voided,
				lock_version = si.lock_version
			};
		}

		public static SageExpenditure DeserializeExpenditure(SageExpenditureDeserialization si)
		{
			DateTime date;
			if (!DateTime.TryParse(si.date, out date))
			{
				string msg = string.Format("Failed parsing date:{0}", si.date ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			DateTime? invoice_date;
			if (si.invoice_date == string.Empty)
			{
				invoice_date = null;
			}
			else
			{
				DateTime invoice_date_tmp;
				if (!DateTime.TryParse(si.invoice_date, out invoice_date_tmp))
				{
					string msg = string.Format("Failed parsing invoice_date:{0}", si.invoice_date ?? string.Empty);
					log.Error(msg);
					throw new Exception(msg);
				}

				invoice_date = invoice_date_tmp;
			}

			decimal amount, tax_amount, gross_amount, tax_percentage_rate;
			if (!decimal.TryParse(si.amount, out amount))
			{
				string msg = string.Format("Failed parsing amount:{0}", si.amount ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			if (!decimal.TryParse(si.tax_amount, out tax_amount))
			{
				string msg = string.Format("Failed parsing tax_amount:{0}", si.tax_amount ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			if (!decimal.TryParse(si.gross_amount, out gross_amount))
			{
				string msg = string.Format("Failed parsing gross_amount:{0}", si.gross_amount ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}
			if (!decimal.TryParse(si.tax_percentage_rate, out tax_percentage_rate))
			{
				string msg = string.Format("Failed parsing tax_percentage_rate:{0}", si.tax_percentage_rate ?? string.Empty);
				log.Error(msg);
				throw new Exception(msg);
			}

			return new SageExpenditure
			{
				SageId = si.id,
				date = date,
				invoice_date = invoice_date,
				amount = amount,
				tax_amount = tax_amount,
				gross_amount = gross_amount,
				tax_percentage_rate = tax_percentage_rate,
				tax_code = si.tax_code == null ? null : si.tax_code.key,
				tax_scheme_period_id = si.tax_scheme_period_id,
				reference = si.reference,
				contact = si.contact == null ? null : si.contact.key,
				source = si.source == null ? null : si.source.key,
				destination = si.destination == null ? null : si.destination.key,
				//payment_method = si.payment_method == null ? null : si.payment_method.key,
				voided = si.voided,
				lock_version = si.lock_version
			};
		}

		public static SagePaymentStatus DeserializePaymentStatus(SagePaymentStatusDeserialization si)
		{
			return new SagePaymentStatus
			{
				SageId = si.id,
				name = si.name
			};
		}
	}
}