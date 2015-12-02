SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueSaveResponse') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveResponse
GO

IF TYPE_ID('LogicalGlueResponseList') IS NOT NULL
	DROP TYPE LogicalGlueResponseList
GO

CREATE TYPE LogicalGlueResponseList AS TABLE (
	[ServiceLogID] BIGINT NOT NULL,
	[ReceivedTime] DATETIME NOT NULL,
	[BucketID] BIGINT NULL,
	[HasEquifaxData] BIT NOT NULL
)
GO

CREATE PROCEDURE LogicalGlueSaveResponse
@Tbl LogicalGlueResponseList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO Response (
		[ServiceLogID],
		[ReceivedTime],
		[BucketID],
		[HasEquifaxData]
	) SELECT
		[ServiceLogID],
		[ReceivedTime],
		[BucketID],
		[HasEquifaxData]
	FROM
		@Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()

	SELECT @ScopeID AS ID
END
GO
