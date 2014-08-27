SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveBrokerInstantOfferRequest') IS NOT NULL
	DROP PROCEDURE SaveBrokerInstantOfferRequest
GO

IF TYPE_ID('BrokerInstantOfferRequestList') IS NOT NULL
	DROP TYPE BrokerInstantOfferRequestList
GO

CREATE TYPE BrokerInstantOfferRequestList AS TABLE (
	Created DATETIME NOT NULL,
	BrokerId INT NOT NULL,
	CompanyNameNumber NVARCHAR(255) NULL,
	AnnualTurnover DECIMAL(18,6) NOT NULL,
	AnnualProfit DECIMAL(18,6) NOT NULL,
	NumOfEmployees INT NOT NULL,
	IsHomeOwner BIT NOT NULL,
	MainApplicantCreditScore NVARCHAR(255) NULL,
	ExperianRefNum NVARCHAR(255) NULL,
	ExperianCompanyName NVARCHAR(255) NULL,
	ExperianCompanyLegalStatus NVARCHAR(255) NULL,
	ExperianCompanyPostcode NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveBrokerInstantOfferRequest
@Tbl BrokerInstantOfferRequestList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO BrokerInstantOfferRequest (
		Created,
		BrokerId,
		CompanyNameNumber,
		AnnualTurnover,
		AnnualProfit,
		NumOfEmployees,
		IsHomeOwner,
		MainApplicantCreditScore,
		ExperianRefNum,
		ExperianCompanyName,
		ExperianCompanyLegalStatus,
		ExperianCompanyPostcode
	) SELECT
		Created,
		BrokerId,
		CompanyNameNumber,
		AnnualTurnover,
		AnnualProfit,
		NumOfEmployees,
		IsHomeOwner,
		MainApplicantCreditScore,
		ExperianRefNum,
		ExperianCompanyName,
		ExperianCompanyLegalStatus,
		ExperianCompanyPostcode
	FROM @Tbl
	
	DECLARE @RequestId INT
	SET @RequestId = SCOPE_IDENTITY()

	SELECT @RequestId AS RequestId
END
GO
