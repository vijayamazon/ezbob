IF OBJECT_ID('NL_LoanInterestFreezeGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanInterestFreezeGet AS SELECT 1')
GO

ALTER PROCEDURE NL_LoanInterestFreezeGet
@loanID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
	   [LoanInterestFreezeID]	 
      ,[LoanID]
      ,[StartDate]
      ,[EndDate]
      ,[InterestRate]
      ,[ActivationDate]
      ,[DeactivationDate]
	  ,[OldID]
	FROM
		[dbo].[NL_LoanInterestFreeze]
	WHERE
		[LoanID]=@loanID
END

GO


