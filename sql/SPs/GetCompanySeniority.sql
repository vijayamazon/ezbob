IF OBJECT_ID('GetCompanySeniority') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanySeniority AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCompanySeniority
@CustomerID INT, @IsLimited BIT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		IncorporationDate
	FROM
		dbo.udfGetCustomerCompanyAnalytics(@CustomerID, NULL, 0, 0, 0)
END
GO
