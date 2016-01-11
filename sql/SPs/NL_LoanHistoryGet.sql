SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanHistoryGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanHistoryGet AS SELECT 1')
GO

ALTER PROCEDURE NL_LoanHistoryGet
@LoanID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		h.LoanHistoryID,
		h.LoanID,
		h.UserID,
		h.LoanLegalID,		
		h.Amount,
		h.RepaymentIntervalTypeID,
		h.RepaymentCount,
		h.InterestRate,
		h.EventTime,
		h.[Description],
		h.RepaymentDate,
		h.PaymentPerInterval,
		h.AgreementModel,
		h.InterestOnlyRepaymentCount
	FROM
		NL_LoanHistory h
	WHERE
		h.LoanID = @LoanID 
END
GO
