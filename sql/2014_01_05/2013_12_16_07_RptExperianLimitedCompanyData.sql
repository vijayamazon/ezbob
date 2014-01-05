IF OBJECT_ID('RptExperianLimitedCompanyData') IS NULL
	EXECUTE('CREATE PROCEDURE RptExperianLimitedCompanyData AS SELECT 1')
GO

ALTER PROCEDURE RptExperianLimitedCompanyData
AS
SELECT
	c.Id,
	e.JsonPacket
FROM
	Customer c
	INNER JOIN MP_ExperianDataCache e ON c.LimitedRefNum = e.CompanyRefNumber AND e.JsonPacket LIKE '<%'
WHERE
	c.IsTest = 0
GO
