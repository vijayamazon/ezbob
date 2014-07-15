SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdDLB5') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdDLB5
GO

IF TYPE_ID('ExperianLtdDLB5List') IS NOT NULL
	DROP TYPE ExperianLtdDLB5List
GO

CREATE TYPE ExperianLtdDLB5List AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	RecordType NVARCHAR(255) NULL,
	IssueCompany NVARCHAR(255) NULL,
	CurrentpreviousIndicator NVARCHAR(255) NULL,
	EffectiveDate DATETIME NULL,
	ShareClassNumber NVARCHAR(255) NULL,
	ShareholdingNumber NVARCHAR(255) NULL,
	ShareholderNumber NVARCHAR(255) NULL,
	ShareholderType NVARCHAR(255) NULL,
	Prefix NVARCHAR(255) NULL,
	FirstName NVARCHAR(255) NULL,
	MidName1 NVARCHAR(255) NULL,
	LastName NVARCHAR(255) NULL,
	Suffix NVARCHAR(255) NULL,
	ShareholderQualifications NVARCHAR(255) NULL,
	Title NVARCHAR(255) NULL,
	ShareholderCompanyName NVARCHAR(255) NULL,
	KgenName NVARCHAR(255) NULL,
	ShareholderRegisteredNumber NVARCHAR(255) NULL,
	AddressLine1 NVARCHAR(255) NULL,
	AddressLine2 NVARCHAR(255) NULL,
	AddressLine3 NVARCHAR(255) NULL,
	Town NVARCHAR(255) NULL,
	County NVARCHAR(255) NULL,
	Postcode NVARCHAR(255) NULL,
	Country NVARCHAR(255) NULL,
	ShareholderPunaPcode NVARCHAR(255) NULL,
	ShareholderRMC NVARCHAR(255) NULL,
	SuppressionFlag NVARCHAR(255) NULL,
	NOCRefNumber NVARCHAR(255) NULL,
	LastUpdated DATETIME NULL
)
GO

CREATE PROCEDURE SaveExperianLtdDLB5
@Tbl ExperianLtdDLB5List READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdDLB5 (
		ExperianLtdID,
		RecordType,
		IssueCompany,
		CurrentpreviousIndicator,
		EffectiveDate,
		ShareClassNumber,
		ShareholdingNumber,
		ShareholderNumber,
		ShareholderType,
		Prefix,
		FirstName,
		MidName1,
		LastName,
		Suffix,
		ShareholderQualifications,
		Title,
		ShareholderCompanyName,
		KgenName,
		ShareholderRegisteredNumber,
		AddressLine1,
		AddressLine2,
		AddressLine3,
		Town,
		County,
		Postcode,
		Country,
		ShareholderPunaPcode,
		ShareholderRMC,
		SuppressionFlag,
		NOCRefNumber,
		LastUpdated
	) SELECT
		ExperianLtdID,
		RecordType,
		IssueCompany,
		CurrentpreviousIndicator,
		EffectiveDate,
		ShareClassNumber,
		ShareholdingNumber,
		ShareholderNumber,
		ShareholderType,
		Prefix,
		FirstName,
		MidName1,
		LastName,
		Suffix,
		ShareholderQualifications,
		Title,
		ShareholderCompanyName,
		KgenName,
		ShareholderRegisteredNumber,
		AddressLine1,
		AddressLine2,
		AddressLine3,
		Town,
		County,
		Postcode,
		Country,
		ShareholderPunaPcode,
		ShareholderRMC,
		SuppressionFlag,
		NOCRefNumber,
		LastUpdated
	FROM @Tbl
END
GO


