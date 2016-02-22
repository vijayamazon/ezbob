SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_CustomerLoansGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_CustomerLoansGet AS SELECT 1')
GO

ALTER PROCEDURE NL_CustomerLoansGet
@CustomerID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		l.LoanID,
		l.OfferID,
		l.LoanTypeID,
		l.LoanStatusID,
		l.LoanFormulaID,		
		l.LoanSourceID,		
		l.EzbobBankAccountID,
		l.CreationTime,
		l.Refnum,				
		l.Position,
		l.DateClosed,	
		l.PrimaryLoanID,				
		l.OldLoanID 		
	FROM
		NL_Loans l 
		join vw_NL_LoansCustomer v on v.LoanID = l.LoanID		 
	WHERE
		v.CustomerID = @CustomerID
END
GO

