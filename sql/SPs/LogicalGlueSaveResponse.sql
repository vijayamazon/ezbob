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
	[HttpStatus] INT NOT NULL,
	[ResponseStatus] INT NOT NULL,
	[TimeoutSourceID] BIGINT NULL,
	[ErrorMessage] NVARCHAR(MAX) NULL,
	[GradeID] BIGINT NULL,
	[HasEquifaxData] BIT NOT NULL,
	[ParsingExceptionType] NVARCHAR(MAX) NULL,
	[ParsingExceptionMessage] NVARCHAR(MAX) NULL
)
GO

CREATE PROCEDURE LogicalGlueSaveResponse
@Tbl LogicalGlueResponseList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO LogicalGlueResponses (
		[ServiceLogID],
		[ReceivedTime],
		[HttpStatus],
		[ResponseStatus],
		[TimeoutSourceID],
		[ErrorMessage],
		[GradeID],
		[HasEquifaxData],
		[ParsingExceptionType],
		[ParsingExceptionMessage]
	) SELECT
		[ServiceLogID],
		[ReceivedTime],
		[HttpStatus],
		[ResponseStatus],
		[TimeoutSourceID],
		[ErrorMessage],
		[GradeID],
		[HasEquifaxData],
		[ParsingExceptionType],
		[ParsingExceptionMessage]
	FROM
		@Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()

	SELECT @ScopeID AS ID
END
GO
