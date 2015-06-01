SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_ACCOUNTING_LOAN_BALANCE' AND Fields LIKE '%,OutstandingPrincipal,%')
BEGIN
	UPDATE ReportScheduler SET
		Header = 'Issue Date,Client ID,Loan ID,Client Name,Client Email,Loan Status,Issued Amount,Outstanding Principal,Setup Fee,Earned Interest,Outstanding Interest,Earned Fees,Outstanding Fees,Total Repaid (Cash),Balance,Customer Status',
		Fields = 'IssueDate,#ClientID,!LoanID,ClientName,ClientEmail,LoanStatus,IssuedAmount,OutstandingPrincipal,SetupFee,EarnedInterest,OutstandingInterest,EarnedFees,OutstandingFees,CashPaid,Balance,CustomerStatus'
	WHERE
		Type = 'RPT_ACCOUNTING_LOAN_BALANCE'
END
GO
