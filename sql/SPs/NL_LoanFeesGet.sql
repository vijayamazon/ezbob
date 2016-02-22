IF OBJECT_ID('NL_LoanFeesGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanFeesGet AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_LoanFeesGet]
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
	  ,[UpdatedByUserID]
	  ,[UpdateTime] 
	  ,[OldFeeID]
	FROM  [dbo].[NL_LoanFees] WHERE	[LoanID]=@LoanID and (@LoanFeeID IS NULL OR ([LoanFeeID] = @LoanFeeID))
END

GO