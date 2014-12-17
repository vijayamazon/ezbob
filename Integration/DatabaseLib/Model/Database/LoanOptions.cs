namespace EZBob.DatabaseLib.Model.Database {
	using FluentNHibernate.Mapping;

	public class LoanOptions {
		public virtual int Id { get; set; }
		public virtual int LoanId { get; set; }
		public virtual bool AutoPayment { get; set; }
		public virtual bool ReductionFee { get; set; }
		public virtual bool LatePaymentNotification { get; set; }
		public virtual string CaisAccountStatus { get; set; }
		public virtual string ManualCaisFlag { get; set; }
		public virtual bool EmailSendingAllowed { get; set; }
		public virtual bool MailSendingAllowed { get; set; }
		public virtual bool SmsSendingAllowed { get; set; }
	}

	public class LoanOptionsMap : ClassMap<LoanOptions> {
		public LoanOptionsMap() {
			Table("LoanOptions");
			Id(x => x.Id).GeneratedBy.Native().Column("Id");
			Map(x => x.AutoPayment);
			Map(x => x.LoanId);
			Map(x => x.ReductionFee);
			Map(x => x.LatePaymentNotification);
			Map(x => x.CaisAccountStatus);
			Map(x => x.EmailSendingAllowed);
			Map(x => x.MailSendingAllowed);
			Map(x => x.SmsSendingAllowed);
			Map(x => x.ManualCaisFlag);
		}
	}
}
