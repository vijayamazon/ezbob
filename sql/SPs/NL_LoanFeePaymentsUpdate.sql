IF OBJECT_ID('NL_LoanFeePaymentsUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanFeePaymentsUpdate AS SELECT 1')
GO


ALTER PROCEDURE [dbo].[NL_LoanFeePaymentsUpdate]
	@LoanFeePaymentID BIGINT,
	@Amount DECIMAL(18,6) = NULL	
AS
BEGIN
	SET NOCOUNT ON;
			
		UPDATE 
			[NL_LoanFeePayments]	
		SET 
			[Amount] = ISNULL(@Amount, Amount)						
		WHERE 
			[LoanFeePaymentID] = @LoanFeePaymentID;		
		
END