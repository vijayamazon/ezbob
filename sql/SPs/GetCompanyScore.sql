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

	SELECT
		Score
	FROM
		CustomerAnalyticsCompany
	WHERE
		CustomerID = @CustomerID
		AND
		IsActive = 1
END
GO
