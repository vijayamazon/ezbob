IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptExperianLimitedCompanyData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptExperianLimitedCompanyData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptExperianLimitedCompanyData]
AS
BEGIN
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
END
GO
