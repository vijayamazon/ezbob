IF OBJECT_ID('GetCompanyScore') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanyScore AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCompanyScore
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @MaxScore INT = 0

	SELECT
		@MaxScore = (CASE WHEN MaxScore IS NULL OR MaxScore < Score THEN Score ELSE MaxScore END)
	FROM
		CustomerAnalyticsCompany
	WHERE
		CustomerID = @CustomerID
		AND
		IsActive = 1

	SELECT @MaxScore AS MaxScore
END
GO