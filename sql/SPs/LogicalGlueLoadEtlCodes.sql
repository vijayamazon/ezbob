SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueLoadEtlCodes') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueLoadEtlCodes AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueLoadEtlCodes
AS
BEGIN
	SELECT
		Value = EtlCodeID,
		Name = EtlCode,
		CommunicationCode,
		IsHardReject,
		IsError
	FROM
		LogicalGlueEtlCodes
END
GO
