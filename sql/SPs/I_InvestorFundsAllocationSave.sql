SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorFundsAllocationSave') IS NOT NULL
	DROP PROCEDURE I_InvestorFundsAllocationSave
GO

IF TYPE_ID('I_InvestorFundsAllocationList') IS NOT NULL
	DROP TYPE I_InvestorFundsAllocationList
GO

CREATE TYPE I_InvestorFundsAllocationList AS TABLE (
	[InvestorBankAccountID] INT NOT NULL,
	[Amount] DECIMAL(18, 6) NULL,
	[AllocationTimestamp] DATETIME NULL,
	[ReleaseTimestamp] DATETIME NULL
)
GO

CREATE PROCEDURE I_InvestorFundsAllocationSave
@Tbl I_InvestorFundsAllocationList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_InvestorFundsAllocation (
		[InvestorBankAccountID],
		[Amount],
		[AllocationTimestamp],
		[ReleaseTimestamp]
	) SELECT
		[InvestorBankAccountID],
		[Amount],
		[AllocationTimestamp],
		[ReleaseTimestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


