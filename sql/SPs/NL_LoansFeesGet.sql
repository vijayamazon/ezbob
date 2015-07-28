IF OBJECT_ID('NL_LoansFeesGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoansFeesGet AS SELECT 1')
GO

ALTER PROCEDURE NL_LoansFeesGet
@LoanID INT
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
END

GO


