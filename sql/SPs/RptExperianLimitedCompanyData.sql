IF OBJECT_ID('RptExperianLimitedCompanyData') IS NULL
	EXECUTE('CREATE PROCEDURE RptExperianLimitedCompanyData AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptExperianLimitedCompanyData
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		c.Id AS CustomerID,
		l.Id AS LogID,
		ROW_NUMBER() OVER(PARTITION BY c.Id, co.ExperianRefNum ORDER BY l.InsertDate DESC, l.Id DESC) AS RowNum
	INTO
		#cl
	FROM
		Customer c
		INNER JOIN Company co ON co.Id = c.CompanyId 
		INNER JOIN MP_ServiceLog l ON co.ExperianRefNum = l.CompanyRefNum
	WHERE
		c.IsTest = 0
		AND
		co.ExperianRefNum IS NOT NULL
		AND
		co.ExperianRefNum != 'NotFound'
		AND
		c.TypeOfBusiness IN ('Limited', 'LLP')

	------------------------------------------------------------------------------

	SELECT
		#cl.CustomerID AS Id,
		l.ResponseData AS JsonPacket
	FROM
		#cl
		INNER JOIN MP_ServiceLog l ON #cl.LogID = l.Id
	WHERE
		#cl.RowNum = 1
	ORDER BY
		#cl.CustomerID

	------------------------------------------------------------------------------

	DROP TABLE #cl
END
GO
