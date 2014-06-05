UPDATE dbo.ReportScheduler
SET Header = 'Date,Client ID,Loan ID,Client Name,Client Email,Earned Interest,Loan Amount,Total Repaid,Principal Repaid,Customer Status,Level'
	, Fields = 'IssueDate,#ClientID,!LoanID,ClientName,ClientEmail,EarnedInterest,LoanAmount,TotalRepaid,PrincipalRepaid,CustomerStatus,{RowLevel'
WHERE Type = 'RPT_EARNED_INTEREST'
GO

UPDATE dbo.ReportScheduler
SET Header = 'Date,Client ID,Loan ID,Client Name,Client Email,Loan Type,Setup Fee,Amount,Period,Planned Interest,Planned Repaid,Total Principal Repaid,Total Interest Repaid,Earned Interest,Expected Interest,Accrued Interest,Total Interest,Total Fees Repaid,Total Charges,Base Interest,Discount Plan,Customer Status,Level'
, Fields = 'Date,!ClientID,!LoanID,ClientName,ClientEmail,LoanTypeName,SetupFee,LoanAmount,Period,PlannedInterest,PlannedRepaid,TotalPrincipalRepaid,TotalInterestRepaid,EarnedInterest,ExpectedInterest,AccruedInterest,TotalInterest,TotalFeesRepaid,TotalCharges,%BaseInterest,DiscountPlan,CustomerStatus,{RowLevel'
WHERE Type = 'RPT_LOANS_GIVEN'
GO

UPDATE dbo.ReportScheduler
SET Header = 'Date,Loan ID,Client ID,Client Email,Client Name,Paid Amount,Principal,Interest,Fees,Rollover,Payment Type,Description,Customer Status,Sum Match,Level'
	, Fields = 'PostDate,!LoanID,!ClientID,ClientEmail,ClientName,Amount,LoanRepayment,Interest,Fees,Rollover,TransactionType,Description,CustomerStatus,{SumMatch,{RowLevel'
WHERE Type = 'RPT_PAYMENTS_RECEIVED'
GO

UPDATE dbo.ReportScheduler
SET Header = 'Issue Date,Client ID,Loan ID,Client Name,Client Email,Loan Status,Issued Amount,Setup Fee,Earned Interest,Earned Fees,Total Repaid (Cash),Balance,Customer Status'
	, Fields = 'IssueDate,#ClientID,!LoanID,ClientName,ClientEmail,LoanStatus,IssuedAmount,SetupFee,EarnedInterest,EarnedFees,CashPaid,Balance,CustomerStatus'
WHERE Type = 'RPT_ACCOUNTING_LOAN_BALANCE'
GO