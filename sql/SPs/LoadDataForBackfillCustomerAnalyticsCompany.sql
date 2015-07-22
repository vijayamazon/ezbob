IF OBJECT_ID('LoadDataForBackfillCustomerAnalyticsCompany') IS NULL
	EXECUTE('CREATE PROCEDURE LoadDataForBackfillCustomerAnalyticsCompany AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadDataForBackfillCustomerAnalyticsCompany
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		c.Id AS CustomerID,
		co.ExperianRefNum AS CompanyRegNum
	FROM
		Customer c
		INNER JOIN Company co ON c.CompanyId = co.Id
	WHERE
		co.ExperianRefNum IS NOT NULL
		AND
		co.ExperianRefNum NOT IN ('NotFound', 'exception')
END
GO
