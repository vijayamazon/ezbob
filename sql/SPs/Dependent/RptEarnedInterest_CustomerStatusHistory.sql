IF OBJECT_ID('RptEarnedInterest_CustomerStatusHistory') IS NULL
	EXECUTE('CREATE PROCEDURE RptEarnedInterest_CustomerStatusHistory AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptEarnedInterest_CustomerStatusHistory
AS
BEGIN
	SET NOCOUNT ON;

	EXECUTE LoadCustomerStatusHistory NULL, NULL
END
GO
