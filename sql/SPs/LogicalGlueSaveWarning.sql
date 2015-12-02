SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueSaveWarning') IS NOT NULL
	DROP PROCEDURE LogicalGlueSaveWarning
GO

IF TYPE_ID('LogicalGlueWarningList') IS NOT NULL
	DROP TYPE LogicalGlueWarningList
GO

CREATE TYPE LogicalGlueWarningList AS TABLE (
	[ModelOutputID] BIGINT NOT NULL,
	[Value] NVARCHAR(MAX) NULL,
	[FeatureName] NVARCHAR(255) NULL,
	[MinValue] NVARCHAR(255) NULL,
	[MaxValue] NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE LogicalGlueSaveWarning
@Tbl LogicalGlueWarningList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO Warning (
		[ModelOutputID] ,
		[Value],
		[FeatureName],
		[MinValue],
		[MaxValue]
	) SELECT
		[ModelOutputID] ,
		[Value],
		[FeatureName],
		[MinValue],
		[MaxValue]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO
