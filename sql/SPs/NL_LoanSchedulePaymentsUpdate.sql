IF OBJECT_ID('NL_LoanSchedulePaymentsUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanSchedulePaymentsUpdate AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_LoanSchedulePaymentsUpdate]
	@LoanSchedulePaymentID BIGINT,	
	@PrincipalPaid DECIMAL(18,6) = NULL,
	@InterestPaid DECIMAL(18,6) = NULL	
AS
BEGIN
	SET NOCOUNT ON;
			
		UPDATE 
			[NL_LoanSchedulePayments]
		SET  			
			[PrincipalPaid] = ISNULL(@PrincipalPaid, PrincipalPaid),
			[InterestPaid] = ISNULL(@InterestPaid, InterestPaid)					
		WHERE 
			[LoanSchedulePaymentID] = @LoanSchedulePaymentID;	
		
END