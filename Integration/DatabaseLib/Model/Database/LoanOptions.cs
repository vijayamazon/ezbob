namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class LoanOptions {
		public virtual int Id { get; set; }
		public virtual int LoanId { get; set; }
		/// <summary>
		/// If false no auto charging
		/// </summary>
		public virtual bool AutoPayment { get; set; }
		/// <summary>
		/// if has value and today > value and AutoPayment = false no auto charging
		/// </summary>
		public virtual DateTime? StopAutoChargeDate { get; set; }
		/// <summary>
		/// If no sufficient funds error while auto charging occurs try to charge 75% 50% 25% of the amount.
		/// </summary>
		public virtual bool ReductionFee { get; set; }
		public virtual bool LatePaymentNotification { get; set; }
		public virtual string CaisAccountStatus { get; set; }
		public virtual string ManualCaisFlag { get; set; }
		/// <summary>
		/// if false no collection emails are sent to customer
		/// </summary>
		public virtual bool EmailSendingAllowed { get; set; }
		/// <summary>
		/// if false no snail mails are sent to customer
		/// </summary>
		public virtual bool MailSendingAllowed { get; set; }
		/// <summary>
		/// if false no collection sms are sent to customer
		/// </summary>
		public virtual bool SmsSendingAllowed { get; set; }
		/// <summary>
		/// if false no late fees are assigned to the loan
		/// </summary>
		public virtual bool AutoLateFees { get; set; }
		/// <summary>
		/// if AutoLateFees = false and StopAutoLateFeesFromDate has value and StopAutoLateFeesToDate has value 
		/// and today between StopAutoLateFeesFromDate and StopAutoLateFeesToDate then no late fees assigned to the loan
		/// </summary>
		public virtual DateTime? StopLateFeeFromDate { get; set; }
		public virtual DateTime? StopLateFeeToDate { get; set; }
		
		public static LoanOptions GetDefault(int loanID) {
			var options = new LoanOptions {
				AutoPayment = true,
				LatePaymentNotification = true,
				ReductionFee = true,
				EmailSendingAllowed = true,
				MailSendingAllowed = true,
				SmsSendingAllowed = true,
				CaisAccountStatus = "Calculated value",
				LoanId = loanID,
				AutoLateFees = true,
			};

			return options;
		}
	}

	public class LoanOptionsMap : ClassMap<LoanOptions> {
		public LoanOptionsMap() {
			Table("LoanOptions");
			Id(x => x.Id).GeneratedBy.Native().Column("Id");
			
			Map(x => x.LoanId);
			Map(x => x.ReductionFee);
			
			Map(x => x.LatePaymentNotification);
			Map(x => x.CaisAccountStatus);
			Map(x => x.ManualCaisFlag);

			Map(x => x.EmailSendingAllowed);
			Map(x => x.MailSendingAllowed);
			Map(x => x.SmsSendingAllowed);
			
			Map(x => x.AutoPayment);
			Map(x => x.StopAutoChargeDate).CustomType<UtcDateTimeType>();

			Map(x => x.AutoLateFees);
			Map(x => x.StopLateFeeFromDate).CustomType<UtcDateTimeType>();
			Map(x => x.StopLateFeeToDate).CustomType<UtcDateTimeType>();
		}
	}
}
