SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorBankAccountSave') IS NOT NULL
	DROP PROCEDURE I_InvestorBankAccountSave
GO

IF OBJECT_ID('I_InvestorBankAccountUpdate') IS NOT NULL
	DROP PROCEDURE I_InvestorBankAccountUpdate
GO
IF OBJECT_ID('I_InvestorBankAccountUpdateActive') IS NOT NULL
	DROP PROCEDURE I_InvestorBankAccountUpdateActive
GO


IF TYPE_ID('I_InvestorBankAccountList') IS NOT NULL
	DROP TYPE I_InvestorBankAccountList
GO

CREATE TYPE I_InvestorBankAccountList AS TABLE (
	[InvestorBankAccountID] INT NOT NULL,
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
	[UserID] INT NULL,
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
		[UserID],
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
		[UserID],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO

CREATE PROCEDURE I_InvestorBankAccountUpdate
@Tbl I_InvestorBankAccountList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE 
		I_InvestorBankAccount
	SET 
		I_InvestorBankAccount.[InvestorID] = tbl.[InvestorID],
		I_InvestorBankAccount.[InvestorAccountTypeID] = tbl.[InvestorAccountTypeID],
		I_InvestorBankAccount.[BankCode] = tbl.[BankCode],
		I_InvestorBankAccount.[BankAccountName] = tbl.[BankAccountName],
		I_InvestorBankAccount.[BankAccountNumber] = tbl.[BankAccountNumber],
		I_InvestorBankAccount.[Timestamp] = tbl.[Timestamp],
		I_InvestorBankAccount.[UserID] = tbl.[UserID]
	FROM
		I_InvestorBankAccount b 
	INNER JOIN 
		@Tbl tbl 
	ON
		b.InvestorBankAccountID = tbl.InvestorBankAccountID
END
GO


CREATE PROCEDURE I_InvestorBankAccountUpdateActive
@Tbl I_InvestorBankAccountList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE 
		I_InvestorBankAccount
	SET 
		I_InvestorBankAccount.[InvestorID] = tbl.[InvestorID],
		I_InvestorBankAccount.[InvestorAccountTypeID] = tbl.[InvestorAccountTypeID],
		I_InvestorBankAccount.[IsActive] = tbl.[IsActive],
		I_InvestorBankAccount.[Timestamp] = tbl.[Timestamp],
		I_InvestorBankAccount.[UserID] = tbl.[UserID]
	FROM
		I_InvestorBankAccount b 
	INNER JOIN 
		@Tbl tbl 
	ON
		b.InvestorBankAccountID = tbl.InvestorBankAccountID
END
GO

