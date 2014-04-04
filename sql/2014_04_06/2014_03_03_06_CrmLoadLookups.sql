IF OBJECT_ID('CrmLoadActions') IS NULL
	EXECUTE('CREATE PROCEDURE CrmLoadActions AS SELECT 1')
GO

ALTER PROCEDURE CrmLoadActions
AS
	SELECT
		Id AS ID,
		Name
	FROM
		CRMActions
GO

IF OBJECT_ID('CrmLoadStatuses') IS NULL
	EXECUTE('CREATE PROCEDURE CrmLoadStatuses AS SELECT 1')
GO

ALTER PROCEDURE CrmLoadStatuses
AS
	SELECT
		Id AS ID,
		Name
	FROM
		CRMStatuses
GO
