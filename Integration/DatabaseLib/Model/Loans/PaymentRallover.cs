namespace EZBob.DatabaseLib.Model.Loans {
	using System;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public enum RolloverStatus {
		New = 0,
		Expired,
		Paid,
		Removed
	}

	public class RolloverStatusType : EnumStringType<RolloverStatus> {
	}

	public class PaymentRollover {
		public virtual RolloverStatus Status { get; set; }
		public virtual int Id { get; set; }
		public virtual LoanScheduleItem LoanSchedule { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual string CreatorName { get; set; }
		public virtual decimal Payment { get; set; }
		public virtual DateTime? PaymentDueDate { get; set; }
		public virtual DateTime? PaymentNewDate { get; set; }
		public virtual DateTime? ExpiryDate { get; set; }
		public virtual DateTime? CustomerConfirmationDate { get; set; }
		public virtual Decimal PaidPaymentAmount { get; set; }
		public virtual int MounthCount { get; set; }

		public override string ToString() {
			return string.Format("PaymentRollover: Created: {0}, Payment: {1}, ScheduleDate: {2}, PaymentDueDate: {3}, PaymentNewDate: {4}, ExpiryDate: {5}," +
				"CustomerConfirmationDate: {6}, PaidPaymentAmount: {7}, MounthCount: {8}", Created, Payment, LoanSchedule.Date, PaymentDueDate, PaymentNewDate,
				ExpiryDate, CustomerConfirmationDate, PaidPaymentAmount, MounthCount);
		}
	}

	public sealed class PaymentRolloverMap : ClassMap<PaymentRollover> {
		public PaymentRolloverMap() {
			Table("PaymentRollover");
			Id(x => x.Id).GeneratedBy.Native();
			References(x => x.LoanSchedule, "LoanScheduleId");
			Map(x => x.Created).CustomType<UtcDateTimeType>();
			Map(x => x.CreatorName);
			Map(x => x.Payment);
			Map(x => x.PaymentDueDate).CustomType<UtcDateTimeType>();
			Map(x => x.PaymentNewDate).CustomType<UtcDateTimeType>();
			Map(x => x.ExpiryDate).CustomType<UtcDateTimeType>();
			Map(x => x.CustomerConfirmationDate).CustomType<UtcDateTimeType>();
			Map(x => x.PaidPaymentAmount);
			Map(x => x.Status).CustomType<RolloverStatusType>();
			Map(x => x.MounthCount).Default("1");
		}
	}
}