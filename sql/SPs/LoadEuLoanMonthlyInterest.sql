SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadEuLoanMonthlyInterest') IS NULL
	EXECUTE('CREATE PROCEDURE LoadEuLoanMonthlyInterest AS SELECT 1')
GO

ALTER PROCEDURE LoadEuLoanMonthlyInterest
AS
BEGIN
	SELECT
		[Id],
		[Start],
		[End],
		[Value]
	FROM
		EuLoanMonthlyInterest
	ORDER BY
		[Start],
		[End]
END
GO
