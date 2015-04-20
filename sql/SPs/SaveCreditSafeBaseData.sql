SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeBaseData') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeBaseData
GO

IF TYPE_ID('CreditSafeBaseDataList') IS NOT NULL
	DROP TYPE CreditSafeBaseDataList
GO

CREATE TYPE CreditSafeBaseDataList AS TABLE (
	ServiceLogID BIGINT NULL,
	CompanyRefNum NVARCHAR(10) NULL,
	HasCreditSafeError BIT NULL,
	HasParsingError BIT NULL,
	Error NVARCHAR(MAX) NULL,
	InsertDate DATETIME NULL,
	Number NVARCHAR(10) NULL,
	Name NVARCHAR(100) NULL,
	Telephone NVARCHAR(20) NULL,
	TpsRegistered BIT NULL,
	Address1 NVARCHAR(100) NULL,
	Address2 NVARCHAR(100) NULL,
	Address3 NVARCHAR(100) NULL,
	Address4 NVARCHAR(100) NULL,
	Postcode NVARCHAR(10) NULL,
	SicCode NVARCHAR(10) NULL,
	SicDescription NVARCHAR(500) NULL,
	Website NVARCHAR(100) NULL,
	CompanyType NVARCHAR(500) NULL,
	AccountsType NVARCHAR(100) NULL,
	AnnualReturnDate DATETIME NULL,
	IncorporationDate DATETIME NULL,
	AccountsFilingDate DATETIME NULL,
	LatestAccountsDate DATETIME NULL,
	Quoted NVARCHAR(10) NULL,
	CompanyStatus NVARCHAR(10) NULL,
	CCJValues INT NULL,
	CCJNumbers INT NULL,
	CCJDateFrom DATETIME NULL,
	CCJDateTo DATETIME NULL,
	CCJNumberOfWrits INT NULL,
	Outstanding INT NULL,
	Satisfied INT NULL,
	ShareCapital INT NULL
)
GO

CREATE PROCEDURE SaveCreditSafeBaseData
@Tbl CreditSafeBaseDataList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @CreditSafeBaseDataID BIGINT
	
	INSERT INTO CreditSafeBaseData (
		ServiceLogID,
		CompanyRefNum,
		HasCreditSafeError,
		HasParsingError,
		Error,
		InsertDate,
		Number,
		Name,
		Telephone,
		TpsRegistered,
		Address1,
		Address2,
		Address3,
		Address4,
		Postcode,
		SicCode,
		SicDescription,
		Website,
		CompanyType,
		AccountsType,
		AnnualReturnDate,
		IncorporationDate,
		AccountsFilingDate,
		LatestAccountsDate,
		Quoted,
		CompanyStatus,
		CCJValues,
		CCJNumbers,
		CCJDateFrom,
		CCJDateTo,
		CCJNumberOfWrits,
		Outstanding,
		Satisfied,
		ShareCapital
	) SELECT
		ServiceLogID,
		CompanyRefNum,
		HasCreditSafeError,
		HasParsingError,
		Error,
		InsertDate,
		Number,
		Name,
		Telephone,
		TpsRegistered,
		Address1,
		Address2,
		Address3,
		Address4,
		Postcode,
		SicCode,
		SicDescription,
		Website,
		CompanyType,
		AccountsType,
		AnnualReturnDate,
		IncorporationDate,
		AccountsFilingDate,
		LatestAccountsDate,
		Quoted,
		CompanyStatus,
		CCJValues,
		CCJNumbers,
		CCJDateFrom,
		CCJDateTo,
		CCJNumberOfWrits,
		Outstanding,
		Satisfied,
		ShareCapital
	FROM @Tbl
	
	SET @CreditSafeBaseDataID = SCOPE_IDENTITY()

	SELECT @CreditSafeBaseDataID AS CreditSafeBaseDataID
END
GO


