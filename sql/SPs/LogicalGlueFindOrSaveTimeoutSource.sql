SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueFindOrSaveTimeoutSource') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueFindOrSaveTimeoutSource AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueFindOrSaveTimeoutSource
@CommunicationCode NVARCHAR(5)
AS
BEGIN
	DECLARE @tbl AS TABLE (CommunicationCode NVARCHAR(5))

	INSERT INTO @Tbl (CommunicationCode) VALUES (@CommunicationCode)

	MERGE INTO
		LogicalGlueTimeoutSources dst
	USING
		@tbl src
	ON
		dst.CommunicationCode = src.CommunicationCode
	WHEN MATCHED THEN
		UPDATE SET CommunicationCode = dst.CommunicationCode
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (TimeoutSource, CommunicationCode) VALUES (src.CommunicationCode, src.CommunicationCode)
	OUTPUT
		inserted.TimeoutSourceID AS Value,
		inserted.TimeoutSource AS Name,
		inserted.CommunicationCode AS CommunicationCode;
END
GO
