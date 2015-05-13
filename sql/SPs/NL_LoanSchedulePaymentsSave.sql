SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanSchedulePaymentsSave') IS NOT NULL
	DROP PROCEDURE NL_LoanSchedulePaymentsSave
GO

IF TYPE_ID('NL_LoanSchedulePaymentsList') IS NOT NULL
	DROP TYPE NL_LoanSchedulePaymentsList
GO

CREATE TYPE NL_LoanSchedulePaymentsList AS TABLE (
	[LoanScheduleID] INT NOT NULL,
	[PaymentID] INT NOT NULL,
	[PrincipalPaid] DECIMAL(18, 6) NOT NULL,
	[InterestPaid] DECIMAL(18, 6) NOT NULL
)
GO

CREATE PROCEDURE NL_LoanSchedulePaymentsSave
@Tbl NL_LoanSchedulePaymentsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanSchedulePayments (
		[LoanScheduleID],
		[PaymentID],
		[PrincipalPaid],
		[InterestPaid]
	) SELECT
		[LoanScheduleID],
		[PaymentID],
		[PrincipalPaid],
		[InterestPaid]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


