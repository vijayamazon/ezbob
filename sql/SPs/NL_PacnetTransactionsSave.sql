SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_PacnetTransactionsSave') IS NOT NULL
	DROP PROCEDURE NL_PacnetTransactionsSave
GO

IF TYPE_ID('NL_PacnetTransactionsList') IS NOT NULL
	DROP TYPE NL_PacnetTransactionsList
GO

CREATE TYPE NL_PacnetTransactionsList AS TABLE (
	[FundTransferID] INT NULL,
	[TransactionTime] DATETIME NULL,
	[Amount] DECIMAL(18, 6) NOT NULL,
	[Notes] NVARCHAR(MAX) NULL,
	[PacnetTransactionStatusID] INT NULL,
	[TrackingNumber] NVARCHAR(100) NULL,
	[StatusUpdatedTime] DATETIME NOT NULL
)
GO

CREATE PROCEDURE NL_PacnetTransactionsSave
@Tbl NL_PacnetTransactionsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_PacnetTransactions (
		[FundTransferID],
		[TransactionTime],
		[Amount],
		[Notes],
		[PacnetTransactionStatusID],
		[TrackingNumber],
		[StatusUpdatedTime]
	) SELECT
		[FundTransferID],
		[TransactionTime],
		[Amount],
		[Notes],
		[PacnetTransactionStatusID],
		[TrackingNumber],
		[StatusUpdatedTime]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


