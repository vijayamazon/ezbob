SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_ProductSubTypeSave') IS NOT NULL
	DROP PROCEDURE I_ProductSubTypeSave
GO

IF TYPE_ID('I_ProductSubTypeList') IS NOT NULL
	DROP TYPE I_ProductSubTypeList
GO

CREATE TYPE I_ProductSubTypeList AS TABLE (
	[ProductTypeID] INT NOT NULL,
	[GradeID] INT NOT NULL,
	[FundingTypeID] INT NULL,
	[OriginID] INT NOT NULL,
	[LoanSourceID] INT NOT NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_ProductSubTypeSave
@Tbl I_ProductSubTypeList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_ProductSubType (
		[ProductTypeID],
		[GradeID],
		[FundingTypeID],
		[OriginID],
		[LoanSourceID],
		[Timestamp]
	) SELECT
		[ProductTypeID],
		[GradeID],
		[FundingTypeID],
		[OriginID],
		[LoanSourceID],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


