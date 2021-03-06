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
	INNER JOIN Company co ON co.Id = c.CompanyId 
	INNER JOIN MP_ExperianDataCache e ON co.ExperianRefNum = e.CompanyRefNumber AND e.JsonPacket LIKE '<%'
WHERE
	c.IsTest = 0
	AND c.TypeOfBusiness IN ('Limited', 'LLP', 'PShip')
ORDER BY
	c.Id


GO

