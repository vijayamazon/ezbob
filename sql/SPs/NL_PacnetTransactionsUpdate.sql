SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_PacnetTransactionsUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE NL_PacnetTransactionsUpdate AS SELECT 1')
GO

ALTER PROCEDURE  NL_PacnetTransactionsUpdate
	@TrackingNumber nvarchar(100), 
	@PacnetTransactionStatusID int, 
	@Notes nvarchar(max),
	@UpdateTime datetime,
	@FundTransferActive bit
AS
BEGIN
	
	SET NOCOUNT ON;

	UPDATE [NL_PacnetTransactions] set [PacnetTransactionStatusID] = @PacnetTransactionStatusID, [Notes] = @Notes, [StatusUpdatedTime] = @UpdateTime  WHERE [TrackingNumber] = @TrackingNumber;
	UPDATE [NL_FundTransfers] set [IsActive] = @FundTransferActive;	

END