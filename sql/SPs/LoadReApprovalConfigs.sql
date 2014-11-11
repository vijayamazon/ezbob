IF OBJECT_ID('LoadReApprovalConfigs') IS NULL
	EXECUTE('CREATE PROCEDURE LoadReApprovalConfigs AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadReApprovalConfigs
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Name,
		Value
	FROM
		ConfigurationVariables
	WHERE
		Name LIKE 'AutoReApprove%'
END
GO
