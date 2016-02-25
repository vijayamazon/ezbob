SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_PaypointTransactionsGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_PaypointTransactionsGet AS SELECT 1')
GO

ALTER PROCEDURE NL_PaypointTransactionsGet
@LoanID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		[PaypointTransactionID],
		pp.[PaymentID],
		[TransactionTime],
		pp.[Amount],
		pp.[Notes],
		[PaypointTransactionStatusID],
		[PaypointUniqueID],
		[PaypointCardID],
		[IP]
	FROM [dbo].[NL_PaypointTransactions] pp inner join NL_Payments p on p.PaymentID = pp.PaymentID WHERE p.LoanID = @LoanID	
		
END
GO