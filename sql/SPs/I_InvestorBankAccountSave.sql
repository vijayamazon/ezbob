SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorBankAccountSave') IS NOT NULL
	DROP PROCEDURE I_InvestorBankAccountSave
GO

IF TYPE_ID('I_InvestorBankAccountList') IS NOT NULL
	DROP TYPE I_InvestorBankAccountList
GO

CREATE TYPE I_InvestorBankAccountList AS TABLE (
	[InvestorID] INT NOT NULL,
	[InvestorAccountTypeID] INT NOT NULL,
	[BankName] NVARCHAR(255) NULL,
	[BankCode] NVARCHAR(255) NULL,
	[BankCountryID] NVARCHAR(255) NULL,
	[BankBranchName] NVARCHAR(255) NULL,
	[BankBranchNumber] NVARCHAR(255) NULL,
	[BankAccountName] NVARCHAR(255) NULL,
	[BankAccountNumber] NVARCHAR(255) NULL,
	[RepaymentKey] NVARCHAR(255) NULL,
	[IsActive] BIT NOT NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_InvestorBankAccountSave
@Tbl I_InvestorBankAccountList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_InvestorBankAccount (
		[InvestorID],
		[InvestorAccountTypeID],
		[BankName],
		[BankCode],
		[BankCountryID],
		[BankBranchName],
		[BankBranchNumber],
		[BankAccountName],
		[BankAccountNumber],
		[RepaymentKey],
		[IsActive],
		[Timestamp]
	) SELECT
		[InvestorID],
		[InvestorAccountTypeID],
		[BankName],
		[BankCode],
		[BankCountryID],
		[BankBranchName],
		[BankBranchNumber],
		[BankAccountName],
		[BankAccountNumber],
		[RepaymentKey],
		[IsActive],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


