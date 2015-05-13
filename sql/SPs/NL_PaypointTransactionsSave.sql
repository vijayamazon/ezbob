SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_PaypointTransactionsSave') IS NOT NULL
	DROP PROCEDURE NL_PaypointTransactionsSave
GO

IF TYPE_ID('NL_PaypointTransactionsList') IS NOT NULL
	DROP TYPE NL_PaypointTransactionsList
GO

CREATE TYPE NL_PaypointTransactionsList AS TABLE (
	[PaymentID] INT NOT NULL,
	[TransactionTime] DATETIME NOT NULL,
	[Amount] DECIMAL(18, 6) NOT NULL,
	[Notes] NVARCHAR(MAX) NULL,
	[PaypointTransactionStatusID] INT NULL,
	[PaypointUniqID] NVARCHAR(100) NULL,
	[PaypointCardID] INT NOT NULL,
	[IP] NVARCHAR(10) NULL
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
		[PaypointUniqID],
		[PaypointCardID],
		[IP]
	) SELECT
		[PaymentID],
		[TransactionTime],
		[Amount],
		[Notes],
		[PaypointTransactionStatusID],
		[PaypointUniqID],
		[PaypointCardID],
		[IP]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


