SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_FundTransfersSave') IS NOT NULL
	DROP PROCEDURE NL_FundTransfersSave
GO

IF TYPE_ID('NL_FundTransfersList') IS NOT NULL
	DROP TYPE NL_FundTransfersList
GO

CREATE TYPE NL_FundTransfersList AS TABLE (
	[LoanID] BIGINT NOT NULL,
	[Amount] DECIMAL(18, 6) NOT NULL,
	[TransferTime] DATETIME NOT NULL,
	[IsActive] BIT NOT NULL,
	[LoanTransactionMethodID] INT NOT NULL
)
GO

CREATE PROCEDURE NL_FundTransfersSave
@Tbl NL_FundTransfersList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_FundTransfers (
		[LoanID],
		[Amount],
		[TransferTime],
		[IsActive],		
		[LoanTransactionMethodID]
	) SELECT
		[LoanID],
		[Amount],
		[TransferTime],
		[IsActive],
		[LoanTransactionMethodID]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO