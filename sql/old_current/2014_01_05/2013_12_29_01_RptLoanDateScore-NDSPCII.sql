IF OBJECT_ID('RptLoanDateScoreNDSPCII') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoanDateScoreNDSPCII AS SELECT 1')
GO

ALTER PROCEDURE RptLoanDateScoreNDSPCII
AS
	SELECT
		c.Id AS CustomerID,
		e.InsertDate,
		e.ServiceType,
		e.ResponseData
	FROM
		Customer c
		LEFT JOIN MP_ServiceLog e
			ON c.Id = e.CustomerId
			AND e.ServiceType IN ('Consumer Request', 'E-SeriesLimitedData')
			AND e.ResponseData IS NOT NULL
	WHERE
		c.IsTest = 0
	ORDER BY
		c.Id,
		e.InsertDate,
		e.ServiceType
GO
