IF OBJECT_ID('NL_LoansFeesGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoansFeesGet AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_LoansFeesGet]
@LoanID BIGINT,
@LoanFeeID BIGINT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[LoanFeeID]
      ,[LoanID]
      ,[LoanFeeTypeID]
      ,[AssignedByUserID]
      ,[Amount]
      ,[CreatedTime]
      ,[AssignTime]
      ,[DeletedByUserID]
      ,[DisabledTime]
      ,[Notes]
	FROM  [dbo].[NL_LoanFees] WHERE	[LoanID]=@LoanID 
	 and (@LoanFeeID IS NULL OR ([LoanFeeID] = @LoanFeeID))
END

GO