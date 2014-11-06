IF OBJECT_ID('GetReApprovalConfigs') IS NULL
	EXECUTE('CREATE PROCEDURE GetReApprovalConfigs AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetReApprovalConfigs
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		AutoReApproveMaxNumOfOutstandingLoans = CONVERT(INT, Value)
	FROM
		ConfigurationVariables
	WHERE
		Name = 'AutoReApproveMaxNumOfOutstandingLoans'
END
GO
