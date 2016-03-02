SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueLoadTimeoutSources') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueLoadTimeoutSources AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueLoadTimeoutSources
AS
BEGIN
	SELECT
		Value = TimeoutSourceID,
		Name = TimeoutSource,
		CommunicationCode
	FROM
		LogicalGlueTimeoutSources
END
GO
