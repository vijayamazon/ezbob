UPDATE ReportScheduler SET
	Header = 'Issue Date,Client ID,Loan ID,Client Name,Client Email,Loan Status,Issued Amount,Setup Fee,Earned Interest,Earned Fees,Total Repaid (Cash),Balance',
	Fields = 'IssueDate,#ClientID,!LoanID,ClientName,ClientEmail,LoanStatus,IssuedAmount,SetupFee,EarnedInterest,EarnedFees,CashPaid,Balance'
WHERE
	Type = 'RPT_ACCOUNTING_LOAN_BALANCE'
GO
