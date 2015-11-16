IF OBJECT_ID('NL_LoanSchedulePaymentsUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanSchedulePaymentsUpdate AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_LoanSchedulePaymentsUpdate]
	@LoanSchedulePaymentID BIGINT,
	--@LoanScheduleID BIGINT = NULL,	
	--@PaymentID BIGINT = NULL,	
	@PrincipalPaid DECIMAL(18,6) = NULL,
	@InterestPaid DECIMAL(18,6) = NULL	
AS
BEGIN
	SET NOCOUNT ON;
			
		UPDATE 
			[NL_LoanSchedulePayments]
		SET  
			--[LoanScheduleID] = ISNULL(@LoanScheduleID, LoanScheduleID), 
			--[PaymentID] = ISNULL(@PaymentID, PaymentID), 
			[PrincipalPaid] = ISNULL(@PrincipalPaid, PrincipalPaid),
			[InterestPaid] = ISNULL(@InterestPaid, InterestPaid)					
		WHERE 
			[LoanSchedulePaymentID] = @LoanSchedulePaymentID;	
		
END