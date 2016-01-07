SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_PaymentsGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_PaymentsGet AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_PaymentsGet]
@LoanID BIGINT
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
		p.Notes,
		p.PaymentDestination,
		p.LoanID
	FROM		
		[dbo].[NL_Payments] p 		
	WHERE		
		p.LoanID = @LoanID
END
GO