IF OBJECT_ID('NL_LoanFeeCancel') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanFeeCancel AS SELECT 1')
GO


ALTER PROCEDURE [dbo].[NL_LoanFeeCancel]
	@LoanFeeID BIGINT,	
	@DeletedByUserID INT,	
	@DisabledTime DATE,
	@Notes nvarchar(max)=null
AS
BEGIN

    UPDATE [dbo].[NL_LoanFees] SET [DeletedByUserID]=@DeletedByUserID, [DisabledTime]=@DisabledTime, [Notes] = isnull (@Notes, [Notes]) WHERE [LoanFeeID]= @LoanFeeID;

	UPDATE [dbo].[NL_LoanFeePayments] SET [ResetAmount] = [Amount], [Amount] = 0 WHERE [LoanFeeID] = @LoanFeeID;;          

END