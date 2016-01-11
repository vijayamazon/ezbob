IF OBJECT_ID('NL_LoanFeeDisable') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanFeeDisable AS SELECT 1')
GO


ALTER PROCEDURE NL_LoanFeeDisable
	@LoanFeeID BIGINT,	
	@DeletedByUserID INT,	
	@DisabledTime DATE,
	@Notes nvarchar(max)=null
AS
BEGIN

    UPDATE [dbo].[NL_LoanFees] SET [DeletedByUserID]=@DeletedByUserID, [DisabledTime]=@DisabledTime, [Notes] = isnull (@Notes, [Notes]) WHERE [LoanFeeID]= @LoanFeeID

END