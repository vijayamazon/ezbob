IF NOT EXISTS (SELECT 1 FROM LoanType WHERE Id = 3)
BEGIN
	INSERT INTO LoanType (Id, Type, Name, Description, IsDefault, RepaymentPeriod)
	VALUES (3, 'AlibabaLoanType', 'Alibaba Loan', 'Alibaba Loan', 0, 12)
END
ELSE
BEGIN
	IF EXISTS (SELECT 1 FROM LoanType WHERE Id = 3 AND RepaymentPeriod = 6)
		UPDATE LoanType SET RepaymentPeriod = 12 WHERE Id = 3 AND RepaymentPeriod = 6
END
GO
