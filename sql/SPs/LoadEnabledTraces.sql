IF OBJECT_ID('LoadEnabledTraces') IS NULL
	EXECUTE('CREATE PROCEDURE LoadEnabledTraces AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadEnabledTraces
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Name = TraceName
	FROM
		DecisionTraceNames
	WHERE
		IsEnabled = 1
END
GO
