IF OBJECT_ID('LoadApprovalConfigs') IS NULL
	EXECUTE('CREATE PROCEDURE LoadApprovalConfigs AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadApprovalConfigs
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Name,
		Value
	FROM
		ConfigurationVariables
	WHERE
		Name LIKE 'AutoApprove%'
END
GO
