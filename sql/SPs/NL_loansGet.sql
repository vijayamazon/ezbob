IF OBJECT_ID('NL_LoansGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoansGet AS SELECT 1')
GO

ALTER PROCEDURE NL_LoansGet
@loanID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[LoanID]
      ,[OfferID]
      ,[LoanTypeID]
      ,[RepaymentIntervalTypeID]
      ,[LoanStatusID]
      ,[EzbobBankAccountID]
      ,[LoanSourceID]
      ,[Position]
      ,[InitialLoanAmount]
      ,[CreationTime]
      ,[IssuedTime]
      ,[RepaymentCount]
      ,[Refnum]
      ,[DateClosed]
      ,[InterestRate]
      ,[InterestOnlyRepaymentCount]
      ,[OldLoanID]
	FROM
		[ezbob].[dbo].[NL_Loans]
	WHERE
		[LoanID]=@loanID
END

GO
