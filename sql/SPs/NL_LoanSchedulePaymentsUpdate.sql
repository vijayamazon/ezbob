IF OBJECT_ID('NL_CancelledPaymentPaidAmountsReset') IS NULL
	EXECUTE('CREATE PROCEDURE NL_CancelledPaymentPaidAmountsReset AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_CancelledPaymentPaidAmountsReset]	
	@PaymentID BIGINT 	
AS
BEGIN
	SET NOCOUNT ON;
			
		UPDATE [NL_LoanSchedulePayments] SET  [PrincipalPaid] = 0, [InterestPaid] = 0 WHERE [PaymentID] = @PaymentID;
		UPDATE [NL_LoanFeePayments] SET [Amount] = 0 WHERE [PaymentID] = @PaymentID;
		
END