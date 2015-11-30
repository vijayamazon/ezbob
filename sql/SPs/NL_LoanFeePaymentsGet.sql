SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanFeePaymentsGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanFeePaymentsGet AS SELECT 1')
GO

ALTER PROCEDURE NL_LoanFeePaymentsGet
@LoanID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		[LoanFeePaymentID], [LoanFeeID], fp.[PaymentID], fp.[Amount], fp.[ResetAmount]
	FROM
		NL_LoanFeePayments fp inner join NL_Payments p on p.PaymentID = fp.PaymentID
	WHERE
		p.LoanID = @LoanID	
		
END
GO
