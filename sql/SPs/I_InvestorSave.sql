SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorSave') IS NOT NULL
	DROP PROCEDURE I_InvestorSave
GO

IF TYPE_ID('I_InvestorList') IS NOT NULL
	DROP TYPE I_InvestorList
GO

CREATE TYPE I_InvestorList AS TABLE (
	[InvestorTypeID] INT NOT NULL,
	[Name] NVARCHAR(255) NULL,
	[IsActive] BIT NOT NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_InvestorSave
@Tbl I_InvestorList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_Investor (
		[InvestorTypeID],
		[Name],
		[IsActive],
		[Timestamp]
	) SELECT
		[InvestorTypeID],
		[Name],
		[IsActive],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


