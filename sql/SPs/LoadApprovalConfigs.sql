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
		RowType = 'Cfg',
		Name,
		Value
	FROM
		ConfigurationVariables
	WHERE
		Name LIKE 'AutoApprove%'
	UNION
	SELECT
		RowType = 'Cfg',
		'AutoApprove' + Name,
		Value
	FROM
		ConfigurationVariables
	WHERE
		Name = 'MinLoan'
	UNION
	SELECT
		RowType = 'TraceEnabled',
		Name = TraceName,
		Value = '1'
	FROM
		DecisionTraceNames
	WHERE
		IsEnabled = 1
END
GO
