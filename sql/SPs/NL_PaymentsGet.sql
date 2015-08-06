SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_PaymentsGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_PaymentsGet AS SELECT 1')
GO

ALTER PROCEDURE NL_PaymentsGet
@LoanID BIGINT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		p.PaymentID,
		p.PaymentMethodID,
		p.PaymentTime,
		p.Amount,
		p.PaymentStatusID,
		p.CreationTime,
		p.CreatedByUserID,
		p.DeletionTime,
		p.DeletedByUserID,
		p.Notes
	FROM
		[dbo].[NL_LoanHistory] h
		INNER JOIN [dbo].[NL_LoanSchedules] lsch ON lsch.LoanHistoryID=h.LoanHistoryID
		INNER JOIN [dbo].[NL_LoanSchedulePayments] sp ON lsch.LoanScheduleID = sp.LoanScheduleID
		INNER JOIN [dbo].[NL_Payments] p
			ON sp.PaymentID = p.PaymentID
			AND p.PaymentStatusID = 1 -- PUT HERE REAL GOOD VALUE
			AND p.DeletionTime IS NULL
		INNER JOIN [dbo].[NL_LoanFeePayments] fp ON fp.PaymentID = p.PaymentID
		INNER JOIN [dbo].[NL_LoanFees] f ON f.LoanFeeID = fp.LoanFeeID
	WHERE
		h.LoanID = @LoanID
END
GO
