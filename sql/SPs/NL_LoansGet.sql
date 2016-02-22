SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoansGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoansGet AS SELECT 1')
GO

ALTER PROCEDURE NL_LoansGet
@LoanID BIGINT
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
		l.PrimaryLoanID,	-- in the case of current loan is an auxiliary loan for other main loan (re-scheduled)			
		l.OldLoanID 		
	FROM
		NL_Loans l 
	WHERE
		LoanID = @LoanID
END
GO

