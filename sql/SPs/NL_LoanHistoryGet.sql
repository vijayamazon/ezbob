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
		LoanHistoryID,
		LoanID,
		UserID,
		LoanLegalID,		
		Amount,
		RepaymentIntervalTypeID,
		RepaymentCount,
		InterestRate,
		EventTime,
		[Description],
		RepaymentDate,
		PaymentPerInterval,
		AgreementModel,
		InterestOnlyRepaymentCount,
		[LateFees] ,
		[DistributedFees] ,
		[OutstandingInterest] 
	FROM NL_LoanHistory WHERE LoanID = @LoanID 
END
GO
