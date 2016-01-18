SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_IndexSave') IS NOT NULL
	DROP PROCEDURE I_IndexSave
GO

IF TYPE_ID('I_IndexList') IS NOT NULL
	DROP TYPE I_IndexList
GO

CREATE TYPE I_IndexList AS TABLE (
	[InvestorID] INT NOT NULL,
	[ProductTypeID] INT NOT NULL,
	[IsActive] BIT NOT NULL,
	[GradeAPercent] DECIMAL(18, 6) NOT NULL,
	[GradeAMinScore] DECIMAL(18, 6) NOT NULL,
	[GradeAMaxScore] DECIMAL(18, 6) NOT NULL,
	[GradeBPercent] DECIMAL(18, 6) NOT NULL,
	[GradeBMinScore] DECIMAL(18, 6) NOT NULL,
	[GradeBMaxScore] DECIMAL(18, 6) NOT NULL,
	[GradeCPercent] DECIMAL(18, 6) NOT NULL,
	[GradeCMinScore] DECIMAL(18, 6) NOT NULL,
	[GradeCMaxScore] DECIMAL(18, 6) NOT NULL,
	[GradeDPercent] DECIMAL(18, 6) NOT NULL,
	[GradeDMinScore] DECIMAL(18, 6) NOT NULL,
	[GradeDMaxScore] DECIMAL(18, 6) NOT NULL,
	[GradeEPercent] DECIMAL(18, 6) NOT NULL,
	[GradeEMinScore] DECIMAL(18, 6) NOT NULL,
	[GradeEMaxScore] DECIMAL(18, 6) NOT NULL,
	[GradeFPercent] DECIMAL(18, 6) NOT NULL,
	[GradeFMinScore] DECIMAL(18, 6) NOT NULL,
	[GradeFMaxScore] DECIMAL(18, 6) NOT NULL,
	[GradeGPercent] DECIMAL(18, 6) NOT NULL,
	[GradeGMinScore] DECIMAL(18, 6) NOT NULL,
	[GradeGMaxScore] DECIMAL(18, 6) NOT NULL,
	[GradeHPercent] DECIMAL(18, 6) NOT NULL,
	[GradeHMinScore] DECIMAL(18, 6) NOT NULL,
	[GradeHMaxScore] DECIMAL(18, 6) NOT NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_IndexSave
@Tbl I_IndexList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_Index (
		[InvestorID],
		[ProductTypeID],
		[IsActive],
		[GradeAPercent],
		[GradeAMinScore],
		[GradeAMaxScore],
		[GradeBPercent],
		[GradeBMinScore],
		[GradeBMaxScore],
		[GradeCPercent],
		[GradeCMinScore],
		[GradeCMaxScore],
		[GradeDPercent],
		[GradeDMinScore],
		[GradeDMaxScore],
		[GradeEPercent],
		[GradeEMinScore],
		[GradeEMaxScore],
		[GradeFPercent],
		[GradeFMinScore],
		[GradeFMaxScore],
		[GradeGPercent],
		[GradeGMinScore],
		[GradeGMaxScore],
		[GradeHPercent],
		[GradeHMinScore],
		[GradeHMaxScore],
		[Timestamp]
	) SELECT
		[InvestorID],
		[ProductTypeID],
		[IsActive],
		[GradeAPercent],
		[GradeAMinScore],
		[GradeAMaxScore],
		[GradeBPercent],
		[GradeBMinScore],
		[GradeBMaxScore],
		[GradeCPercent],
		[GradeCMinScore],
		[GradeCMaxScore],
		[GradeDPercent],
		[GradeDMinScore],
		[GradeDMaxScore],
		[GradeEPercent],
		[GradeEMinScore],
		[GradeEMaxScore],
		[GradeFPercent],
		[GradeFMinScore],
		[GradeFMaxScore],
		[GradeGPercent],
		[GradeGMinScore],
		[GradeGMaxScore],
		[GradeHPercent],
		[GradeHMinScore],
		[GradeHMaxScore],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


