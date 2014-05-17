IF OBJECT_ID('EuLoanMonthlyInterest') IS NULL
BEGIN
	CREATE TABLE EuLoanMonthlyInterest
	(
		Id INT IDENTITY,
		Start INT NOT NULL,
		[End] INT NOT NULL,
		Value DECIMAL (18, 6) NOT NULL
	)
	
	INSERT INTO EuLoanMonthlyInterest (Start, [End], Value) VALUES (0, 999, 0.02)
	INSERT INTO EuLoanMonthlyInterest (Start, [End], Value) VALUES (1000, 10000000, 0.0175)
END
GO
