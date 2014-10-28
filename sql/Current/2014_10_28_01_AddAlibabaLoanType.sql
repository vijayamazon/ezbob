IF NOT EXISTS (SELECT 1 FROM LoanType WHERE Id = 3)
BEGIN
	INSERT INTO LoanType (Id, Type, Name, Description, IsDefault, RepaymentPeriod)
	VALUES (3, 'AlibabaLoanType', 'Alibaba Loan', 'Alibaba Loan', 0, 6)
END
GO
