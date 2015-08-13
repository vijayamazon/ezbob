SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoansGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoansGet AS SELECT 1')
GO

ALTER PROCEDURE NL_LoansGet
@LoanID BIGINT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	--SELECT
	--	l.LoanID,
	--	l.OfferID,
	--	l.LoanTypeID,
	--	l.RepaymentIntervalTypeID,
	--	l.LoanStatusID,
	--	l.EzbobBankAccountID,
	--	l.LoanSourceID,
	--	l.Position,
	--	l.InitialLoanAmount,
	--	l.CreationTime,
	--	l.IssuedTime,
	--	l.RepaymentCount,
	--	l.Refnum,
	--	l.DateClosed,
	--	l.InterestRate,
	--	l.InterestOnlyRepaymentCount,
	--	l.OldLoanID
	--FROM
	--	NL_Loans l
	--WHERE
	--	l.LoanID = @LoanID
	--	AND
	--	(@Now IS NULL OR l.IssuedTime < @Now)
END
GO
