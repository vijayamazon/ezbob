IF object_id('CustomerCompanyHistory') IS NULL
BEGIN
CREATE TABLE CustomerCompanyHistory(
	 Id INT NOT NULL IDENTITY(1,1)
	,InsertDate DATETIME NOT NULL
	,CustomerId INT NOT NULL
	,CompanyId INT NOT NULL
	,CONSTRAINT PK_CustomerCompanyHistory PRIMARY KEY (Id)
	,CONSTRAINT FK_PK_CustomerCompanyHistory_Customer FOREIGN KEY (CustomerId) REFERENCES Customer(Id)
	,CONSTRAINT FK_PK_CustomerCompanyHistory_Company FOREIGN KEY (CompanyId) REFERENCES Company(Id)
)
END
GO

IF NOT EXISTS (SELECT * FROM CustomerCompanyHistory)
BEGIN
	INSERT INTO CustomerCompanyHistory (InsertDate, CustomerId, CompanyId)
	SELECT c.GreetingMailSentDate InsertDate, c.Id CustomerId, c.CompanyId FROM Customer c WHERE c.CompanyId IS NOT NULL
END
GO
