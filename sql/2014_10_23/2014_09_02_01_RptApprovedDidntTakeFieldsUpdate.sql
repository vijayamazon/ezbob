
UPDATE dbo.ReportScheduler
SET Header = 'Customer Id,Full name,Email,Reference Source,Experian Rating,Underwriter decision date,Manager approved sum,Interest rate,Repayment period,Loan type,Had loan, Broker Id'
	, Fields = '#IdCustomer,Fullname,Email,ReferenceSource,ExpirianRating,UnderwriterDecisionDate,ManagerApprovedSum,InterestRate,RepaymentPeriod,LoansType,HadLoan,BrokerID'
WHERE Type='RPT_APPROVED_DIDNT_TAKE'
GO
