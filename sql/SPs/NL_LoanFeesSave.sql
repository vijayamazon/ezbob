SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanFeesSave') IS NOT NULL
	DROP PROCEDURE NL_LoanFeesSave
GO

IF TYPE_ID('NL_LoanFeesList') IS NOT NULL
	DROP TYPE NL_LoanFeesList
GO

CREATE TYPE NL_LoanFeesList AS TABLE (
	[LoanFeeTypeID] INT NULL,
	[LoanID] INT NULL,
	[UserID] INT NULL,
	[Amount] DECIMAL(18, 6) NOT NULL,
	[CreatedTime] DATETIME NOT NULL,
	[AssignTime] DATETIME NOT NULL,
	[DisabledTime] DATETIME NOT NULL,
	[Description] NVARCHAR(MAX) NULL
)
GO

CREATE PROCEDURE NL_LoanFeesSave
@Tbl NL_LoanFeesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanFees (
		[LoanFeeTypeID],
		[LoanID],
		[UserID],
		[Amount],
		[CreatedTime],
		[AssignTime],
		[DisabledTime],
		[Description]
	) SELECT
		[LoanFeeTypeID],
		[LoanID],
		[UserID],
		[Amount],
		[CreatedTime],
		[AssignTime],
		[DisabledTime],
		[Description]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


