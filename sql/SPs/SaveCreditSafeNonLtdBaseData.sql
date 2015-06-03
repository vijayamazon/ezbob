SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeNonLtdBaseData') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeNonLtdBaseData
GO

IF TYPE_ID('CreditSafeNonLtdBaseDataList') IS NOT NULL
	DROP TYPE CreditSafeNonLtdBaseDataList
GO

CREATE TYPE CreditSafeNonLtdBaseDataList AS TABLE (
	ServiceLogID BIGINT NULL,
	EzbobCompanyID NVARCHAR(10) NULL,
	HasCreditSafeError BIT NULL,
	HasParsingError BIT NULL,
	Error NVARCHAR(MAX) NULL,
	InsertDate DATETIME NULL,
	Number NVARCHAR(10) NULL,
	Name NVARCHAR(100) NULL,
	Address1 NVARCHAR(100) NULL,
	Address2 NVARCHAR(100) NULL,
	Address3 NVARCHAR(100) NULL,
	Address4 NVARCHAR(100) NULL,
	PostCode NVARCHAR(10) NULL,
	MpsRegistered BIT NULL,
	AddressDate DATETIME NULL,
	AddressReason NVARCHAR(100) NULL,
	PremiseType NVARCHAR(100) NULL,
	Activities NVARCHAR(100) NULL,
	Employees INT NULL,
	Website NVARCHAR(100) NULL,
	Email NVARCHAR(100) NULL,
	MatchedCcjValue INT NULL,
	MatchedCcjNumber INT NULL,
	MatchedCcjDateFrom DATETIME NULL,
	MatchedCcjDateTo DATETIME NULL,
	PossibleCcjValue INT NULL,
	PossibleCcjNumber INT NULL,
	PossibleCcjDateFrom DATETIME NULL,
	PossibleCcjDateTo DATETIME NULL,
	ExecutiveName NVARCHAR(100) NULL,
	ExecutivePosition NVARCHAR(100) NULL,
	ExecutiveEmail NVARCHAR(100) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeNonLtdBaseData
@Tbl CreditSafeNonLtdBaseDataList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @CreditSafeNonLtdBaseDataID BIGINT

	INSERT INTO CreditSafeNonLtdBaseData (
		ServiceLogID,
		EzbobCompanyID,
		HasCreditSafeError,
		HasParsingError,
		Error,
		InsertDate,
		Number,
		Name,
		Address1,
		Address2,
		Address3,
		Address4,
		PostCode,
		MpsRegistered,
		AddressDate,
		AddressReason,
		PremiseType,
		Activities,
		Employees,
		Website,
		Email,
		MatchedCcjValue,
		MatchedCcjNumber,
		MatchedCcjDateFrom,
		MatchedCcjDateTo,
		PossibleCcjValue,
		PossibleCcjNumber,
		PossibleCcjDateFrom,
		PossibleCcjDateTo,
		ExecutiveName,
		ExecutivePosition,
		ExecutiveEmail
	) SELECT
		ServiceLogID,
		EzbobCompanyID,
		HasCreditSafeError,
		HasParsingError,
		Error,
		InsertDate,
		Number,
		Name,
		Address1,
		Address2,
		Address3,
		Address4,
		PostCode,
		MpsRegistered,
		AddressDate,
		AddressReason,
		PremiseType,
		Activities,
		Employees,
		Website,
		Email,
		MatchedCcjValue,
		MatchedCcjNumber,
		MatchedCcjDateFrom,
		MatchedCcjDateTo,
		PossibleCcjValue,
		PossibleCcjNumber,
		PossibleCcjDateFrom,
		PossibleCcjDateTo,
		ExecutiveName,
		ExecutivePosition,
		ExecutiveEmail
	FROM @Tbl
	
	SET @CreditSafeNonLtdBaseDataID = SCOPE_IDENTITY()

	SELECT @CreditSafeNonLtdBaseDataID AS CreditSafeNonLtdBaseDataID	
END
GO


