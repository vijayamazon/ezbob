IF object_id('BrokerInstantOfferRequest') IS NULL
BEGIN
CREATE TABLE BrokerInstantOfferRequest(
	 Id INT NOT NULL IDENTITY(1,1)
	,BrokerID INT NOT NULL
	,CompanyNameNumber NVARCHAR(300) NOT NULL
	,AnnualTurnover DECIMAL(18,6) NOT NULL
	,AnnualProfit DECIMAL(18,6) NOT NULL
	,NumOfEmployees INT NOT NULL
	,IsHomeOwner BIT NOT NULL DEFAULT(0)
	,MainApplicantCreditScore NVARCHAR(30)
	,ExperianRefNum NVARCHAR(30)
	,ExperinaCompanyName NVARCHAR(300)
	,ExperianCompanyLegalStatus NVARCHAR(30)
	,ExperianComapnyPostcode NVARCHAR(30)
	,CONSTRAINT PK_BrokerInstantOfferRequest PRIMARY KEY (Id)
	,CONSTRAINT FK_BrokerInstantOfferRequest_BrokerID FOREIGN KEY (BrokerID) REFERENCES Broker(BrokerID)
)
END
GO
