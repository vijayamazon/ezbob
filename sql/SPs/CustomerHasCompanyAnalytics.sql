IF OBJECT_ID('CustomerHasCompanyAnalytics') IS NULL
	EXECUTE('CREATE PROCEDURE CustomerHasCompanyAnalytics AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE CustomerHasCompanyAnalytics
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @LineCount INT = 0

	SELECT
		@LineCount = COUNT(*)
	FROM
		CustomerAnalyticsCompany
	WHERE
		CustomerID = @CustomerID
		AND
		IsActive = 1

	SELECT CONVERT(BIT, (CASE @LineCount WHEN 0 THEN 0 ELSE 1 END)) AS HasScore
END
GO