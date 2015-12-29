SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorAccountTypeSave') IS NOT NULL
	DROP PROCEDURE I_InvestorAccountTypeSave
GO

IF TYPE_ID('I_InvestorAccountTypeList') IS NOT NULL
	DROP TYPE I_InvestorAccountTypeList
GO

CREATE TYPE I_InvestorAccountTypeList AS TABLE (
	[InvestorAccountTypeID] INT NOT NULL,
	[Name] NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE I_InvestorAccountTypeSave
@Tbl I_InvestorAccountTypeList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_InvestorAccountType (
		[InvestorAccountTypeID],
		[Name]
	) SELECT
		[InvestorAccountTypeID],
		[Name]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


