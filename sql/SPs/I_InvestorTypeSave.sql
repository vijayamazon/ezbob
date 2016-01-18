SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorTypeSave') IS NOT NULL
	DROP PROCEDURE I_InvestorTypeSave
GO

IF TYPE_ID('I_InvestorTypeList') IS NOT NULL
	DROP TYPE I_InvestorTypeList
GO

CREATE TYPE I_InvestorTypeList AS TABLE (
	[InvestorTypeID] INT NOT NULL,
	[Name] NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE I_InvestorTypeSave
@Tbl I_InvestorTypeList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_InvestorType (
		[InvestorTypeID],
		[Name]
	) SELECT
		[InvestorTypeID],
		[Name]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


