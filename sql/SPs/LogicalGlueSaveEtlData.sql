SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueSaveEtlData') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveEtlData
GO

IF TYPE_ID('LogicalGlueEtlDataList') IS NOT NULL
	DROP TYPE LogicalGlueEtlDataList
GO

CREATE TYPE LogicalGlueEtlDataList AS TABLE (
	[ResponseID] BIGINT NOT NULL,
	[EtlCodeID] BIGINT NULL,
	[Message] NVARCHAR(MAX) NULL
)
GO

CREATE PROCEDURE LogicalGlueSaveEtlData
@Tbl LogicalGlueEtlDataList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO LogicalGlueEtlData (
		[ResponseID],
		[EtlCodeID],
		[Message]
	) SELECT
		[ResponseID],
		[EtlCodeID],
		[Message]
	FROM
		@Tbl
END
GO
