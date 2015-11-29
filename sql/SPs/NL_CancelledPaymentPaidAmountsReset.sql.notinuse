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
	DECLARE @LoanID bigint;

	select @DeletioTime = [DeletionTime], @LoanID = LoanID from [dbo].[NL_Payments] where [PaymentID] = @PaymentID;

	UPDATE [NL_LoanSchedulePayments] SET [PrincipalPaid] = 0, [InterestPaid] = 0 WHERE [PaymentID] in (select PaymentID from [dbo].[NL_Payments] where [PaymentTime] <= @DeletioTime);
	UPDATE [NL_LoanFeePayments] SET [Amount] = 0 WHERE [PaymentID] in (select PaymentID from [dbo].[NL_Payments] where [PaymentTime] <= @DeletioTime);

	-- OPEN CLOSED LOAN	

	DECLARE @CurrentLoanStatus nvarchar(80) = (select LoanStatusID from [dbo].[NL_Loans] where LoanID = @LoanID);
	DECLARE @LiveStatusID int = (SELECT s.LoanStatusID FROM [dbo].[NL_LoanStatuses] s WHERE [LoanStatus] = 'Live');	

	IF (@CurrentLoanStatus in (SELECT s.LoanStatusID FROM [dbo].[NL_LoanStatuses] s WHERE [LoanStatus] in ('PaidOff', 'WriteOff'))) 
		BEGIN
			--UPDATE [dbo].[NL_Loans] SET [LoanStatusID] = (SELECT s.LoanStatusID FROM [dbo].[NL_LoanStatuses] s WHERE [LoanStatus] = 'Live') WHERE LoanID = @LoanID;
			exec [dbo].[NL_LoanUpdate]
				@LoanID = @LoanID, 
				@LoanStatusID = @LiveStatusID,
				@DateClosed = null
		END
		
END