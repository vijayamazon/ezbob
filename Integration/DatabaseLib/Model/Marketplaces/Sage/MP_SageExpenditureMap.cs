namespace EZBob.DatabaseLib.Model.Marketplaces.Sage
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class MP_SageExpenditureMap : ClassMap<MP_SageExpenditure>
	{
		public MP_SageExpenditureMap()
		{
			Table("MP_SageExpenditure");
			Id(x => x.Id);
			References(x => x.Request, "RequestId");
			
			Map(x => x.SageId);
			Map(x => x.date).CustomType<UtcDateTimeType>();
			Map(x => x.invoice_date).CustomType<UtcDateTimeType>();
			Map(x => x.amount);
			Map(x => x.tax_amount);
			Map(x => x.gross_amount);
			Map(x => x.tax_percentage_rate);
			Map(x => x.TaxCodeId);
			Map(x => x.tax_scheme_period_id);
			Map(x => x.reference).Length(250);
			Map(x => x.ContactId);
			Map(x => x.SourceId);
			Map(x => x.DestinationId);
			//Map(x => x.PaymentMethodId);
			Map(x => x.voided);
			Map(x => x.lock_version);
		}
	}
}