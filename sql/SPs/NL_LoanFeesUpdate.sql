IF OBJECT_ID('NL_LoanFeesUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanFeesUpdate AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[NL_LoanFeesUpdate]
	@LoanFeeID bigint,
	@UpdatedByUserID int ,
	@UpdateTime datetime,
	@Amount decimal(18,6) = null,
	@AssignTime date =null,
	@Notes nvarchar(max)  = null
AS
BEGIN
	
	SET NOCOUNT ON;
	
	DECLARE @validateFee nvarchar(64) = NULL; 

	DECLARE @loanID bigint=(select LoanID from NL_LoanFees where [LoanFeeID] = @LoanFeeID);

	EXEC @validateFee = dbo.udfNL_ValidateFeeSave @LoanID = @loanID, @AssignDate = @AssignTime ; 
	
	if @validateFee is not null
	BEGIN			
		select 'Assign date not valid' as Error ;
		RETURN 
	END

    UPDATE [dbo].[NL_LoanFees]  SET 
		[UpdatedByUserID] = @UpdatedByUserID,
		[UpdateTime]  = @UpdateTime,
		[Amount] = ISNULL(@Amount, [Amount]), 
		[AssignTime] = ISNULL(@AssignTime, [AssignTime]), 
		[Notes] = ISNULL(@Notes, [Notes])
    WHERE [LoanFeeID] = @LoanFeeID ;

END
