SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BrokerInstantOfferRequest') IS NULL
BEGIN
	CREATE TABLE BrokerInstantOfferRequest (
		Id INT NOT NULL IDENTITY(1,1),
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
		ExperianCompanyLegalStatus NVARCHAR(30) NULL,
		ExperianCompanyPostcode NVARCHAR(30) NULL,
		CONSTRAINT PK_BrokerInstantOfferRequest PRIMARY KEY (Id),
		CONSTRAINT FK_BrokerInstantOfferRequest_BrokerId FOREIGN KEY (BrokerId) REFERENCES Broker(BrokerID)
	)
END
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BrokerInstantOfferResponse') IS NULL
BEGIN
	CREATE TABLE BrokerInstantOfferResponse (
		Id INT NOT NULL IDENTITY(1,1),
		BrokerInstantOfferRequestId INT NOT NULL,
		ApprovedSum INT NOT NULL,
		RepaymentPeriod INT NOT NULL,
		InterestRate DECIMAL(18,6) NOT NULL,
		LoanTypeId INT NOT NULL,
		LoanSourceId INT NOT NULL,
		UseBrokerSetupFee BIT NOT NULL,
		UseSetupFee BIT NOT NULL,
		CONSTRAINT PK_BrokerInstantOfferResponse PRIMARY KEY (Id),
		CONSTRAINT FK_BrokerInstantOfferResponse_BrokerInstantOfferRequestId FOREIGN KEY (BrokerInstantOfferRequestId) REFERENCES BrokerInstantOfferRequest(Id),
		CONSTRAINT FK_BrokerInstantOfferResponse_LoanTypeId FOREIGN KEY (LoanTypeId) REFERENCES LoanType(Id),
		CONSTRAINT FK_BrokerInstantOfferResponse_LoanSourceId FOREIGN KEY (LoanSourceId) REFERENCES LoanSource(LoanSourceID)
	)
END
GO



