namespace EZBob.DatabaseLib.Model.Database
{
    using FluentNHibernate.Mapping;
    using NHibernate.Type;

	public class MP_PayPointOrderItemMap : ClassMap<MP_PayPointOrderItem>
	{
		public MP_PayPointOrderItemMap()
		{
			Table("MP_PayPointOrderItem");
			Id(x => x.Id);
			References( x => x.Order, "OrderId" );

            Map(x => x.acquirer, "acquirer").Length(300).Nullable();
            Map(x => x.amount, "amount").Nullable();
            Map(x => x.auth_code, "auth_code").Length(300).Nullable();
            Map(x => x.authorised, "authorised").Length(300).Nullable();
            Map(x => x.card_type, "card_type").Length(300).Nullable();
            Map(x => x.cid, "cid").Length(300).Nullable();
            Map(x => x.classType, "classType").Length(300).Nullable();
            Map(x => x.company_no, "company_no").Length(300).Nullable();
            Map(x => x.country, "country").Length(300).Nullable();
            Map(x => x.currency, "currency").Length(300).Nullable();
            Map(x => x.cv2avs, "cv2avs").Length(300).Nullable();
            Map(x => x.date, "date").CustomType<UtcDateTimeType>().Nullable();
            Map(x => x.deferred, "deferred").Length(300).Nullable();
            Map(x => x.emvValue, "emvValue").Length(300).Nullable();
            Map(x => x.ExpiryDate, "ExpiryDate").CustomType<UtcDateTimeType>().Nullable();
            Map(x => x.fraud_code, "fraud_code").Length(300).Nullable();
            Map(x => x.FraudScore, "FraudScore").Length(300).Nullable();
            Map(x => x.ip, "ip").Length(300).Nullable();
            Map(x => x.lastfive, "lastfive").Length(300).Nullable();
            Map(x => x.merchant_no, "merchant_no").Length(300).Nullable();
            Map(x => x.message, "message").Length(300).Nullable();
            Map(x => x.MessageType, "MessageType").Length(300).Nullable();
            Map(x => x.mid, "mid").Length(300).Nullable();
            Map(x => x.name, "name").Length(300).Nullable();
            Map(x => x.options, "options").Length(300).Nullable();
            Map(x => x.start_date, "start_date").CustomType<UtcDateTimeType>().Nullable();
            Map(x => x.status, "status").Length(300).Nullable();
            Map(x => x.tid, "tid").Length(300).Nullable();
            Map(x => x.trans_id, "trans_id").Length(300).Nullable();
		}
	}
}