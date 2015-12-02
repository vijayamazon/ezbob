SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueSaveModelOutput') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveModelOutput
GO

IF TYPE_ID('LogicalGlueModelOutputList') IS NOT NULL
	DROP TYPE LogicalGlueModelOutputList
GO

CREATE TYPE LogicalGlueModelOutputList AS TABLE (
	[ResponseID] BIGINT NOT NULL,
	[ModelID] BIGINT NOT NULL,
	[InferenceResultEncoded] BIGINT NULL,
	[InferenceResultDecoded] NVARCHAR(255) NULL,
	[Score] DECIMAL(18, 6) NULL,
	[Status] NVARCHAR(255) NULL,
	[Exception] NVARCHAR(MAX) NULL,
	[ErrorCode] NVARCHAR(MAX) NULL
)
GO

CREATE PROCEDURE LogicalGlueSaveModelOutput
@Tbl LogicalGlueModelOutputList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ModelOutput (
		[ResponseID],
		[ModelID],
		[InferenceResultEncoded],
		[InferenceResultDecoded],
		[Score],
		[Status],
		[Exception],
		[ErrorCode]
	)
	OUTPUT
		INSERTED.[ModelOutputID],
		INSERTED.[ModelID]
	SELECT
		[ResponseID],
		[ModelID],
		[InferenceResultEncoded],
		[InferenceResultDecoded],
		[Score],
		[Status],
		[Exception],
		[ErrorCode]
	FROM @Tbl
END
GO
