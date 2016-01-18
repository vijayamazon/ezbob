SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorBankAccountTransactionSave') IS NOT NULL
	DROP PROCEDURE I_InvestorBankAccountTransactionSave
GO

IF TYPE_ID('I_InvestorBankAccountTransactionList') IS NOT NULL
	DROP TYPE I_InvestorBankAccountTransactionList
GO

CREATE TYPE I_InvestorBankAccountTransactionList AS TABLE (
	[InvestorBankAccountID] INT NOT NULL,
	[PreviousBalance] DECIMAL(18, 6) NULL,
	[NewBalance] DECIMAL(18, 6) NULL,
	[TransactionAmount] DECIMAL(18, 6) NULL,
	[Timestamp] DATETIME NOT NULL,
	[UserID] INT NULL,
	[Comment] NVARCHAR(500) NULL
)
GO

CREATE PROCEDURE I_InvestorBankAccountTransactionSave
@Tbl I_InvestorBankAccountTransactionList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_InvestorBankAccountTransaction (
		[InvestorBankAccountID],
		[PreviousBalance],
		[NewBalance],
		[TransactionAmount],
		[Timestamp],
		[UserID],
		[Comment]
	) SELECT
		[InvestorBankAccountID],
		[PreviousBalance],
		[NewBalance],
		[TransactionAmount],
		[Timestamp],
		[UserID],
		[Comment]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


