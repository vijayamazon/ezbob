IF OBJECT_ID('RptAlibabaFunnel_LoadBatchNames') IS NULL
	EXECUTE('CREATE PROCEDURE RptAlibabaFunnel_LoadBatchNames AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptAlibabaFunnel_LoadBatchNames
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT
		csr.FName AS BatchName
	FROM
		Customer c
		INNER JOIN CampaignSourceRef csr
			ON c.Id = csr.CustomerId
	WHERE
		csr.FName IS NOT NULL
		AND
		c.AlibabaId IS NOT NULL
		AND
		c.IsTest = 0
END
GO
