SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanHistoryGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanHistoryGet AS SELECT 1')
GO

ALTER PROCEDURE NL_LoanHistoryGet
@LoanID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		h.LoanHistoryID,
		h.Amount,
		h.RepaymentCount,
		h.InterestRate,
		h.EventTime
	FROM
		NL_LoanHistory h
	WHERE
		h.LoanID = @LoanID
		AND
		(@Now IS NULL OR h.EventTime < @Now)
END
GO
