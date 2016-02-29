SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueFindOrSaveEtlCode') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueFindOrSaveEtlCode AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueFindOrSaveEtlCode
@CommunicationCode NVARCHAR(5)
AS
BEGIN
	DECLARE @tbl AS TABLE (CommunicationCode NVARCHAR(5))

	INSERT INTO @Tbl (CommunicationCode) VALUES (@CommunicationCode)

	MERGE INTO
		LogicalGlueEtlCodes dst
	USING
		@tbl src
	ON
		dst.CommunicationCode = src.CommunicationCode
	WHEN MATCHED THEN
		UPDATE SET CommunicationCode = dst.CommunicationCode
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (EtlCode, CommunicationCode, IsHardReject, IsError) VALUES (src.CommunicationCode, src.CommunicationCode, 0, 0)
	OUTPUT
		inserted.EtlCodeID AS Value,
		inserted.EtlCode AS Name,
		inserted.CommunicationCode AS CommunicationCode,
		inserted.IsHardReject,
		inserted.IsError;
END
GO
