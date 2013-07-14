namespace Sage
{
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using log4net;

	public class SageDesreializer
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SageDesreializer));

		public static SageSalesInvoice DeserializeSalesInvoice(SageInvoiceSerialization si)
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

			var deserialized = new SageSalesInvoice();

			deserialized.SageId = si.id;
			deserialized.invoice_number = si.invoice_number;
			if (si.status == null)
			{
				deserialized.status = null;
			}
			else
			{
				deserialized.status = si.status.key;
			}
			deserialized.due_date = due_date;
			deserialized.date = date;
			deserialized.void_reason = si.void_reason;
			deserialized.outstanding_amount = outstanding_amount;
			deserialized.total_net_amount = total_net_amount;
			deserialized.total_tax_amount = total_tax_amount;
			deserialized.tax_scheme_period_id = si.tax_scheme_period_id;
			deserialized.carriage = carriage;
			if (si.carriage_tax_code == null)
			{
				deserialized.carriage_tax_code = null;
			}
			else
			{
				deserialized.carriage_tax_code = si.carriage_tax_code.key;
			}
			deserialized.carriage_tax_rate_percentage = carriage_tax_rate_percentage;
			if (si.contact == null)
			{
				deserialized.contact = null;
			}
			else
			{
				deserialized.contact = si.contact.key;
			}
			deserialized.contact_name = si.contact_name;
			deserialized.main_address = si.main_address;
			deserialized.delivery_address = si.delivery_address;
			deserialized.delivery_address_same_as_main = si.delivery_address_same_as_main;
			deserialized.reference = si.reference;
			deserialized.notes = si.notes;
			deserialized.terms_and_conditions = si.terms_and_conditions;
			deserialized.lock_version = si.lock_version;
			deserialized.line_items = GetInvoiceItems(si.line_items);

			return deserialized;
		}

		private static List<SageSalesInvoiceItem> GetInvoiceItems(IEnumerable<SageInvoiceItemSerialization> items)
		{
			var result = new List<SageSalesInvoiceItem>();
			foreach (SageInvoiceItemSerialization si in items)
			{
				decimal quantity, unit_price, net_amount, tax_amount, tax_rate_percentage;
				if (!decimal.TryParse(si.quantity, out quantity)) return null;
				if (!decimal.TryParse(si.unit_price, out unit_price)) return null;
				if (!decimal.TryParse(si.net_amount, out net_amount)) return null;
				if (!decimal.TryParse(si.tax_amount, out tax_amount)) return null;
				if (!decimal.TryParse(si.tax_rate_percentage, out tax_rate_percentage)) return null;
				
				var deserialized = new SageSalesInvoiceItem();
				deserialized.SageId = si.id;
				deserialized.description = si.description;
				deserialized.quantity = quantity;
				deserialized.unit_price = unit_price;
				deserialized.net_amount = net_amount;
				deserialized.tax_amount = tax_amount;
				deserialized.tax_code = si.tax_code == null ? null : si.tax_code.key;
				deserialized.tax_rate_percentage = tax_rate_percentage;
				deserialized.unit_price_includes_tax = si.unit_price_includes_tax;
				deserialized.ledger_account = si.ledger_account == null ? null : si.ledger_account.key;
				deserialized.product_code = si.product_code;
				deserialized.product = si.product == null ? null : si.product.key;
				deserialized.service = si.service == null ? null : si.service.key;
				deserialized.lock_version = si.lock_version;
				result.Add(deserialized);
			}

			return result;
		}
	}
}