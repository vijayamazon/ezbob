IF OBJECT_ID('RptLoanDateScoreNDSPCII') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoanDateScoreNDSPCII AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptLoanDateScoreNDSPCII
AS
BEGIN
	SELECT
		c.Id AS CustomerID,
		e.InsertDate,
		e.ServiceType,
		e.ResponseData,
		e.Id AS ServiceLogID
	FROM
		Customer c
		LEFT JOIN MP_ServiceLog e
			ON c.Id = e.CustomerId
			AND e.ServiceType IN ('Consumer Request', 'E-SeriesLimitedData', 'E-SeriesNonLimitedData')
			AND e.ResponseData IS NOT NULL
	WHERE
		c.IsTest = 0
	ORDER BY
		c.Id,
		e.InsertDate,
		e.ServiceType
END
GO
