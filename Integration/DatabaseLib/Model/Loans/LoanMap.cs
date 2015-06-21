namespace EZBob.DatabaseLib.Model.Database.Mapping {
	using EZBob.DatabaseLib.Model.Database.Loans;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class LoanMap : ClassMap<Loans.Loan> {
		public LoanMap() {
			Table("Loan");
			Cache.ReadWrite().Region("LongTerm").ReadWrite();
			Id(x => x.Id).GeneratedBy.Native();
			Map(x => x.Date, "`Date`").CustomType<UtcDateTimeType>();
			Map(x => x.DateClosed, "`DateClosed`").CustomType<UtcDateTimeType>();
			Map(x => x.LoanAmount);
			Map(x => x.Status).CustomType<LoanStatusType>();
			Map(x => x.PaymentStatus).CustomType<PaymentStatusType>();
			Map(x => x.Balance);
			Map(x => x.Principal);
			Map(x => x.Repayments);
			Map(x => x.RepaymentsNum);
			Map(x => x.OnTime);
			Map(x => x.OnTimeNum);

			Map(x => x.Late30);
			Map(x => x.Late30Num);
			Map(x => x.Late60);
			Map(x => x.Late60Num);
			Map(x => x.Late90);
			Map(x => x.Late90Num);
			Map(x => x.Late90Plus);
			Map(x => x.Late90PlusNum);

			Map(x => x.MaxDelinquencyDays);

			Map(x => x.PastDues);
			Map(x => x.PastDuesNum);
			Map(x => x.NextRepayment);
			Map(x => x.Position);
			Map(x => x.Interest);
			Map(x => x.InterestPaid);
			Map(x => x.InterestRate);
			Map(x => x.APR);
			Map(x => x.SetupFee);
			Map(x => x.Fees);
			Map(x => x.FeesPaid);
			Map(x => x.IsDefaulted);
			References(x => x.Customer, "CustomerId");
			HasMany(x => x.Transactions)
				.AsSet()
				.KeyColumn("LoanId")
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.ForeignKeyConstraintName("FK_Loan_Transactions")
				.Cache.ReadWrite().Region("LongTerm").ReadWrite();
			HasMany(x => x.Schedule)
				//.AsList(i => i.Column("Position"))
				.AsBag()
				.KeyColumn("LoanId")
				.OrderBy("Date")
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.ForeignKeyConstraintName("FK_Loan_Schedule")
				.Cache.ReadWrite().Region("LongTerm").ReadWrite();
			Map(x => x.RefNumber, "RefNum").Length(11);
			References(x => x.CashRequest, "RequestCashId");
			HasMany(x => x.Agreements)
			   .AsSet()
			   .KeyColumn("LoanId")
			   .Cascade.AllDeleteOrphan()
			   .Inverse()
			   .Cache.ReadWrite().Region("LongTerm").ReadWrite();
			HasMany(x => x.Charges)
				.AsBag()
				.OrderBy("`Date`")
				.KeyColumn("LoanId")
				.Cascade.AllDeleteOrphan()
				.Inverse();
			HasMany(x => x.ScheduleTransactions)
				.AsBag()
				.KeyColumn("LoanId")
				.Cascade.AllDeleteOrphan()
				.Inverse();
			HasMany(x => x.InterestFreeze)
				.AsBag()
				.KeyColumn("LoanId")
				.Cascade.AllDeleteOrphan()
				.Inverse();
            HasMany(x => x.BrokerCommissions)
                .AsSet()
                .KeyColumn("LoanID")
                .Cascade.AllDeleteOrphan()
                .Inverse();

			Map(x => x.AgreementModel).CustomType("StringClob");
			References(x => x.LoanType, "LoanTypeId");
			Map(x => x.LastReportedCaisStatus, "LastReportedCAISStatus");
			Map(x => x.LastReportedCaisStatusDate, "LastReportedCAISStatusDate").CustomType<UtcDateTimeType>();
			Map(x => x.Modified);
			Map(x => x.InterestDue);
			Map(x => x.LastRecalculation);
			References(x => x.LoanSource, "LoanSourceID");
			Map(x => x.LoanLegalId);

			Map(x => x.CustomerSelectedTerm);

			HasMany(x => x.RemovedOnReschedule).AsBag().KeyColumn("LoanId").Cascade.AllDeleteOrphan().Inverse();

		} // constructor
	} // class LoanMap
} // namespace EZBob.DatabaseLib.Model.Database.Mapping
