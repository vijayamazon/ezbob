SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanSchedulePaymentsSave') IS NOT NULL
	DROP PROCEDURE NL_LoanSchedulePaymentsSave
GO

IF TYPE_ID('NL_LoanSchedulePaymentsList') IS NOT NULL
	DROP TYPE NL_LoanSchedulePaymentsList
GO

CREATE TYPE NL_LoanSchedulePaymentsList AS TABLE (
	[LoanScheduleID] BIGINT NOT NULL,
	[PaymentID] BIGINT NOT NULL,
	[PrincipalPaid] DECIMAL(18, 6) NOT NULL,
	[InterestPaid] DECIMAL(18, 6) NOT NULL,
	[ResetPrincipalPaid] DECIMAL(18, 6) NULL,
	[ResetInterestPaid] DECIMAL(18, 6) NULL
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
		[InterestPaid],
		[ResetPrincipalPaid],
		[ResetInterestPaid]
	) SELECT
		[LoanScheduleID],
		[PaymentID],
		[PrincipalPaid],
		[InterestPaid],
		[ResetPrincipalPaid],
		[ResetInterestPaid]
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


