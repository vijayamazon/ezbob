SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveBrokerInstantOfferResponse') IS NOT NULL
	DROP PROCEDURE SaveBrokerInstantOfferResponse
GO

IF TYPE_ID('BrokerInstantOfferResponseList') IS NOT NULL
	DROP TYPE BrokerInstantOfferResponseList
GO

CREATE TYPE BrokerInstantOfferResponseList AS TABLE (
	BrokerInstantOfferRequestId INT NOT NULL,
	ApprovedSum INT NOT NULL,
	RepaymentPeriod INT NOT NULL,
	InterestRate DECIMAL(18,6) NOT NULL,
	LoanTypeId INT NOT NULL,
	LoanSourceId INT NOT NULL,
	UseBrokerSetupFee BIT NOT NULL,
	UseSetupFee BIT NOT NULL
)
GO

CREATE PROCEDURE SaveBrokerInstantOfferResponse
@Tbl BrokerInstantOfferResponseList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO BrokerInstantOfferResponse (
		BrokerInstantOfferRequestId,
		ApprovedSum,
		RepaymentPeriod,
		InterestRate,
		LoanTypeId,
		LoanSourceId,
		UseBrokerSetupFee,
		UseSetupFee
	) SELECT
		BrokerInstantOfferRequestId,
		ApprovedSum,
		RepaymentPeriod,
		InterestRate,
		LoanTypeId,
		LoanSourceId,
		UseBrokerSetupFee,
		UseSetupFee
	FROM @Tbl
	
	DECLARE @ResponseId INT
	SET @ResponseId = SCOPE_IDENTITY()

	SELECT @ResponseId AS ResponseId
END
GO
