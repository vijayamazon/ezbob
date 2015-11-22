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
	[GradeA] DECIMAL(18, 6) NOT NULL,
	[GradeB] DECIMAL(18, 6) NOT NULL,
	[GradeC] DECIMAL(18, 6) NOT NULL,
	[GradeD] DECIMAL(18, 6) NOT NULL,
	[GradeE] DECIMAL(18, 6) NOT NULL,
	[GradeF] DECIMAL(18, 6) NOT NULL,
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
		[GradeA],
		[GradeB],
		[GradeC],
		[GradeD],
		[GradeE],
		[GradeF],
		[Timestamp]
	) SELECT
		[InvestorID],
		[ProductTypeID],
		[IsActive],
		[GradeA],
		[GradeB],
		[GradeC],
		[GradeD],
		[GradeE],
		[GradeF],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


