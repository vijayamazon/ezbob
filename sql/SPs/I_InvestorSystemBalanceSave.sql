SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorSystemBalanceSave') IS NOT NULL
	DROP PROCEDURE I_InvestorSystemBalanceSave
GO

IF TYPE_ID('I_InvestorSystemBalanceList') IS NOT NULL
	DROP TYPE I_InvestorSystemBalanceList
GO

CREATE TYPE I_InvestorSystemBalanceList AS TABLE (
	[InvestorBankAccountID] INT NOT NULL,
	[LoanTransactionID] INT NOT NULL,
	[PreviousBalance] DECIMAL(18, 6) NULL,
	[NewBalance] DECIMAL(18, 6) NULL,
	[TransactionAmount] DECIMAL(18, 6) NULL,
	[ServicingFeeAmount] DECIMAL(18, 6) NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_InvestorSystemBalanceSave
@Tbl I_InvestorSystemBalanceList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_InvestorSystemBalance (
		[InvestorBankAccountID],
		[LoanTransactionID],
		[PreviousBalance],
		[NewBalance],
		[TransactionAmount],
		[ServicingFeeAmount],
		[Timestamp]
	) SELECT
		[InvestorBankAccountID],
		[LoanTransactionID],
		[PreviousBalance],
		[NewBalance],
		[TransactionAmount],
		[ServicingFeeAmount],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


