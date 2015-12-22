SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_SubGradeSave') IS NOT NULL
	DROP PROCEDURE I_SubGradeSave
GO

IF TYPE_ID('I_SubGradeList') IS NOT NULL
	DROP TYPE I_SubGradeList
GO

CREATE TYPE I_SubGradeList AS TABLE (
	[GradeID] INT NOT NULL,
	[Name] NVARCHAR(255) NULL,
	[MinScore] DECIMAL(18, 6) NULL,
	[MaxScore] DECIMAL(18, 6) NULL
)
GO

CREATE PROCEDURE I_SubGradeSave
@Tbl I_SubGradeList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_SubGrade (
		[GradeID],
		[Name],
		[MinScore],
		[MaxScore]
	) SELECT
		[GradeID],
		[Name],
		[MinScore],
		[MaxScore]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


