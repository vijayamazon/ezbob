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
	FundTransferStatusID INT NOT NULL,
	[LoanTransactionMethodID] INT NOT NULL,
	[DeletionTime] datetime NULL,
	[DeletedByUserID] int null
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
		FundTransferStatusID,		
		[LoanTransactionMethodID],
		[DeletionTime],
		[DeletedByUserID]
	) SELECT
		[LoanID],
		[Amount],
		[TransferTime],
		FundTransferStatusID,
		[LoanTransactionMethodID],
		[DeletionTime],
		[DeletedByUserID]
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO