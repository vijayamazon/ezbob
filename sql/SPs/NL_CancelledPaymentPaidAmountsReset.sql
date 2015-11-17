IF OBJECT_ID('NL_CancelledPaymentPaidAmountsReset') IS NULL
	EXECUTE('CREATE PROCEDURE NL_CancelledPaymentPaidAmountsReset AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_CancelledPaymentPaidAmountsReset]	
	@PaymentID BIGINT 	
AS
BEGIN
	SET NOCOUNT ON;
	
	-- RESET ALL PAID PRINCIPAL, INTEREST (SCHEDULE), FEES PAID AFTER [DeletionTime] of deleted payment. New distribution of paid p, i, f (s) will be recalculated and saved again

	DECLARE @DeletioTime datetime;
	set @DeletioTime=(select [DeletionTime] from [dbo].[NL_Payments] where [PaymentID] = @PaymentID);

	UPDATE [NL_LoanSchedulePayments] SET [PrincipalPaid] = 0, [InterestPaid] = 0 WHERE [PaymentID] in (select PaymentID from [dbo].[NL_Payments] where [PaymentTime] <= @DeletioTime);
	UPDATE [NL_LoanFeePayments] SET [Amount] = 0 WHERE [PaymentID] in (select PaymentID from [dbo].[NL_Payments] where [PaymentTime] <= @DeletioTime);
		
END