SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_PaypointTransactionsSave') IS NOT NULL
	DROP PROCEDURE NL_PaypointTransactionsSave
GO

IF TYPE_ID('NL_PaypointTransactionsList') IS NOT NULL
	DROP TYPE NL_PaypointTransactionsList
GO

CREATE TYPE NL_PaypointTransactionsList AS TABLE (
	[PaymentID] BIGINT NOT NULL,
	[TransactionTime] DATETIME NOT NULL,
	[Amount] DECIMAL(18, 6) NOT NULL,
	[Notes] NVARCHAR(MAX) NULL,
	[PaypointTransactionStatusID] INT NOT NULL,
	[PaypointUniqueID] NVARCHAR(100) NOT NULL, -- extra not needed?
	[PaypointCardID] INT NOT NULL, -- PaypointUniqID is in PayPointCard [TransactionId] filed
	[IP] NVARCHAR(32) NULL
)
GO

CREATE PROCEDURE NL_PaypointTransactionsSave
@Tbl NL_PaypointTransactionsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_PaypointTransactions (
		[PaymentID],
		[TransactionTime],
		[Amount],
		[Notes],
		[PaypointTransactionStatusID],
		[PaypointUniqueID],
		[PaypointCardID],
		[IP]
	) SELECT
		[PaymentID],
		[TransactionTime],
		[Amount],
		[Notes],
		[PaypointTransactionStatusID],
		[PaypointUniqueID],
		[PaypointCardID],
		[IP]
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


