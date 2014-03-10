IF OBJECT_ID('CrmLoadStatuses') IS NULL
	EXECUTE('CREATE PROCEDURE CrmLoadStatuses AS SELECT 1')
GO

ALTER PROCEDURE CrmLoadStatuses
AS
BEGIN
	SELECT
		Id AS ID,
		Name
	FROM
		CRMStatuses
END
GO
