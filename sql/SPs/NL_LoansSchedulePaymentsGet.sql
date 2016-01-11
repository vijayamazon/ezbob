SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanSchedulePaymentsGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanSchedulePaymentsGet AS SELECT 1')
GO

ALTER PROCEDURE NL_LoanSchedulePaymentsGet
@LoanID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		[LoanSchedulePaymentID], [LoanScheduleID], s.[PaymentID], [PrincipalPaid], [InterestPaid], [ResetPrincipalPaid], [ResetInterestPaid]
	FROM
		NL_LoanSchedulePayments s inner join NL_Payments p on p.PaymentID = s.PaymentID
	WHERE
		p.LoanID = @LoanID	
		
END
GO