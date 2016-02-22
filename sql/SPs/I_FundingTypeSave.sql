SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_FundingTypeSave') IS NOT NULL
	DROP PROCEDURE I_FundingTypeSave
GO

IF TYPE_ID('I_FundingTypeList') IS NOT NULL
	DROP TYPE I_FundingTypeList
GO

CREATE TYPE I_FundingTypeList AS TABLE (
	[FundingTypeID] INT NOT NULL,
	[Name] NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE I_FundingTypeSave
@Tbl I_FundingTypeList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_FundingType (
		[FundingTypeID],
		[Name]
	) SELECT
		[FundingTypeID],
		[Name]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


