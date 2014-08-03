namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	using System.Collections.Generic;
	using System.Linq;
	using Model.Marketplaces.Sage;

	public class SageExpendituresConverter
	{
		public static IEnumerable<SageExpenditure> GetSageExpenditures(IOrderedEnumerable<MP_SageExpenditure> dbExpenditures)
		{
			var expenditures = new List<SageExpenditure>(dbExpenditures.Select(o => new SageExpenditure
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
				//payment_method = o.PaymentMethodId,
				voided = o.voided,
				lock_version = o.lock_version
			}));

			return expenditures;
		}
	}
}