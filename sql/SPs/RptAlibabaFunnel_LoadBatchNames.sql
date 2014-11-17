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
		c.Name AS BatchName
	FROM
		AlibabaCampaigns c
	ORDER BY
		c.Name
END
GO
