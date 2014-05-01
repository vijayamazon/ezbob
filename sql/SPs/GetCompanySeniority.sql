IF OBJECT_ID('GetCompanySeniority') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanySeniority AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCompanySeniority
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		IncorporationDate
	FROM
		CustomerAnalyticsCompany
	WHERE
		CustomerID = @CustomerID
		AND
		IsActive = 1
END
GO
