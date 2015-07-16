IF OBJECT_ID('NL_LoansFeesGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoansFeesGet AS SELECT 1')
GO

ALTER PROCEDURE NL_LoansFeesGet
@loanID INT
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
	FROM
		[ezbob].[dbo].[NL_LoanFees]
	WHERE
		[LoanID]=@loanID
END

GO


