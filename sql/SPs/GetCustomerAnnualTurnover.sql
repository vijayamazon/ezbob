IF OBJECT_ID('GetCustomerAnnualTurnover') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerAnnualTurnover AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerAnnualTurnover
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @Turnover NUMERIC(18, 2)

	------------------------------------------------------------------------------

	SELECT TOP 1
		@Turnover = m.AnnualTurnover
	FROM
		MedalCalculations m
	WHERE
		m.CustomerId = @CustomerID
		AND
		m.CalculationTime < @Now

	------------------------------------------------------------------------------

	SELECT Turnover = ISNULL(@Turnover, 0)
END
GO
