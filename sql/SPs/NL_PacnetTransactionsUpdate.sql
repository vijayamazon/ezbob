SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_PacnetTransactionsUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE NL_PacnetTransactionsUpdate AS SELECT 1')
GO

ALTER PROCEDURE  NL_PacnetTransactionsUpdate
	@TrackingNumber NVARCHAR(100),
	@PacnetTransactionStatusID INT,
	@Notes NVARCHAR(MAX),
	@UpdateTime DATETIME,
	@FundTransferActive BIT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [NL_PacnetTransactions] SET
		[PacnetTransactionStatusID] = @PacnetTransactionStatusID,
		[Notes] = @Notes,
		[StatusUpdatedTime] = @UpdateTime
	WHERE
		[TrackingNumber] = @TrackingNumber

	IF @FundTransferActive = 1
	BEGIN
		UPDATE [NL_FundTransfers] SET
			FundTransferStatusID = (SELECT FundTransferStatusID FROM NL_FundTransferStatuses WHERE FundTransferStatus = 'Active')
		FROM
			NL_FundTransfers t
			INNER JOIN NL_PacnetTransactions p ON t.FundTransferID = p.FundTransferID
	END
END
GO
