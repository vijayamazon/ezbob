SET QUOTED_IDENTIFIER ON
GO

----------------I_InvestorLoad-------------------------------------

IF OBJECT_ID('I_InvestorLoad') IS NULL
	EXECUTE('CREATE PROCEDURE I_InvestorLoad AS SELECT 1')
GO

ALTER PROCEDURE I_InvestorLoad
@InvestorID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'InvestorData' AS DatumType,
		InvestorID,
		InvestorTypeID, 
		Name,
		IsActive,
		Timestamp
	FROM 
		I_Investor
	WHERE 
		InvestorID = @InvestorID
END
GO

----------------I_InvestorContactLoad-------------------------------------

IF OBJECT_ID('I_InvestorContactLoad') IS NULL
	EXECUTE('CREATE PROCEDURE I_InvestorContactLoad AS SELECT 1')
GO

ALTER PROCEDURE I_InvestorContactLoad
@InvestorID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'InvestorContactData' AS DatumType,
		InvestorContactID,
		InvestorID,
		PersonalName,
		LastName,
		Email,
		Role,
		Comment,
		IsPrimary,
		Mobile, 
		OfficePhone,
		IsActive,
		IsGettingAlerts,
		IsGettingReports,
		Timestamp
	FROM 
		I_InvestorContact
	WHERE 
		InvestorID = @InvestorID
END
GO

----------------I_InvestorBankAccountLoad-------------------------------------

IF OBJECT_ID('I_InvestorBankAccountLoad') IS NULL
	EXECUTE('CREATE PROCEDURE I_InvestorBankAccountLoad AS SELECT 1')
GO

ALTER PROCEDURE I_InvestorBankAccountLoad
@InvestorID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'InvestorBankAccountData' AS DatumType,
		InvestorBankAccountID,
		InvestorID,
		InvestorAccountTypeID,
		BankName,
		BankCode,
		BankCountryID,
		BankBranchName,
		BankBranchNumber,
		BankAccountName,
		BankAccountNumber,
		RepaymentKey,
		IsActive, 
		UserID,
		Timestamp
	FROM 
		I_InvestorBankAccount
	WHERE 
		InvestorID = @InvestorID
END
GO

----------------I_InvestorTypeLoad-------------------------------------

IF OBJECT_ID('I_InvestorTypeLoad') IS NULL
	EXECUTE('CREATE PROCEDURE I_InvestorTypeLoad AS SELECT 1')
GO

ALTER PROCEDURE I_InvestorTypeLoad
@InvestorID INT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		'InvestorTypeData' AS DatumType,
		t.InvestorTypeID, 
		t.Name
	FROM 
		I_InvestorType t 
	INNER JOIN 
		I_Investor i ON i.InvestorTypeID = t.InvestorTypeID
	WHERE 
		i.InvestorID = @InvestorID
END
GO

----------------I_InvestorAccountTypeLoad-------------------------------------

IF OBJECT_ID('I_InvestorAccountTypeLoad') IS NULL
	EXECUTE('CREATE PROCEDURE I_InvestorAccountTypeLoad AS SELECT 1')
GO

ALTER PROCEDURE I_InvestorAccountTypeLoad
@InvestorID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'InvestorAccountTypeData' AS DatumType,
		'InvestorBankAccountData' AS ParentType,
		b.InvestorBankAccountID AS ParentID,
		b.InvestorAccountTypeID,
		Name
	FROM 
		I_InvestorAccountType t 
	INNER JOIN 
		I_InvestorBankAccount b ON b.InvestorAccountTypeID = t.InvestorAccountTypeID
	WHERE 
		b.InvestorID = @InvestorID
END
GO

----------------I_InvestorLoadFull-------------------------------------

IF OBJECT_ID('I_InvestorLoadFull') IS NULL
	EXECUTE('CREATE PROCEDURE I_InvestorLoadFull AS SELECT 1')
GO

ALTER PROCEDURE I_InvestorLoadFull
@InvestorID INT
AS
BEGIN
	SET NOCOUNT ON;

	-- Main table

	EXECUTE I_InvestorLoad @InvestorID

	-- Dependent tables (level 1)

	EXECUTE I_InvestorContactLoad @InvestorID
	EXECUTE I_InvestorBankAccountLoad @InvestorID
	EXECUTE I_InvestorTypeLoad @InvestorID
	
	-- Dependent tables (level 2)

	EXECUTE I_InvestorAccountTypeLoad @InvestorID
END
GO