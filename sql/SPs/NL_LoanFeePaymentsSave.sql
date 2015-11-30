SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanFeePaymentsSave') IS NOT NULL
	DROP PROCEDURE NL_LoanFeePaymentsSave
GO

IF TYPE_ID('NL_LoanFeePaymentsList') IS NOT NULL
	DROP TYPE NL_LoanFeePaymentsList
GO

CREATE TYPE NL_LoanFeePaymentsList AS TABLE (
	[LoanFeeID] BIGINT NOT NULL,
	[PaymentID] BIGINT NOT NULL,
	[Amount] DECIMAL(18, 6) NOT NULL,
	[ResetAmount] DECIMAL(18, 6) NULL
)
GO

CREATE PROCEDURE NL_LoanFeePaymentsSave
@Tbl NL_LoanFeePaymentsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanFeePayments (
		[LoanFeeID],
		[PaymentID],
		[Amount],
		[ResetAmount]
	) SELECT
		[LoanFeeID],
		[PaymentID],
		[Amount],
		[ResetAmount] 
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


