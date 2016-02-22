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
	[FundingTypeID] INT NULL,
	[OriginID] INT NOT NULL,
	[LoanSourceID] INT NOT NULL,
	[IsRegulated] BIT NOT NULL,
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
		[FundingTypeID],
		[OriginID],
		[LoanSourceID],
		[IsRegulated],
		[Timestamp]
	) SELECT
		[ProductTypeID],
		[FundingTypeID],
		[OriginID],
		[LoanSourceID],
		[IsRegulated],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


