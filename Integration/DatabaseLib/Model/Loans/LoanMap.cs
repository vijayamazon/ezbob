namespace EZBob.DatabaseLib.Model.Database.Mapping {
	using EZBob.DatabaseLib.Model.Database.Loans;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class LoanMap : ClassMap<Loan> {
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

                .NotFound.Ignore()

		        .ForeignKeyConstraintName("FK_Loan_Schedule")
		        .Cache.ReadWrite()
		        .Region("LongTerm")
		        .ReadWrite();
              
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

			//TODO kill NH to prevent such horrible code
			Map(x => x.IsOpenPlatform)
				.Formula(@"(
						SELECT CASE 
							WHEN ft.FundingTypeID IS NULL THEN 'No' 
							ELSE 'Yes' END 
						FROM
							Loan 
						LEFT JOIN 
							CashRequests cr ON Loan.RequestCashId = cr.Id
						LEFT JOIN 
							I_ProductSubType pst ON pst.ProductSubTypeID = cr.ProductSubTypeID
						LEFT JOIN 
							I_FundingType ft ON ft.FundingTypeID = pst.FundingTypeID 
						WHERE 
							Loan.Id = Id
				)")
				.Not.Insert()
				.Not.Update();

			Map(x => x.InvestorData)
				.Formula(@"(
						SELECT 
							i.Name + ', holds ' +CAST(CAST(opo.InvestmentPercent*100 AS INT) AS VARCHAR) + '% of the loan, ' AS 'data()' 
						FROM
							Loan 
						LEFT JOIN 
							CashRequests cr ON Loan.RequestCashId = cr.Id
						LEFT JOIN 
							I_OpenPlatformOffer opo ON opo.CashRequestID = cr.Id
						LEFT JOIN 
							I_Investor i ON i.InvestorID = opo.InvestorID
						WHERE 
							Loan.Id = Id
						FOR XML PATH('')  
				)")
				.Not.Insert()
				.Not.Update();
		} // constructor
	} // class LoanMap
} // namespace EZBob.DatabaseLib.Model.Database.Mapping