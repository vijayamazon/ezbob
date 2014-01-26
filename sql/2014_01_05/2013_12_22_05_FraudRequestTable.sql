IF OBJECT_ID('FraudRequest') IS NULL
BEGIN 
	CREATE TABLE FraudRequest(
		 Id INT NOT NULL IDENTITY(1,1)
		,CustomerId INT NOT NULL
		,CheckDate DATETIME NOT NULL
		, CONSTRAINT PK_FraudRequest PRIMARY KEY (Id)
		, CONSTRAINT FK_FraudRequest_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id)
	)

	DECLARE @Statement VARCHAR(MAX)
	SET @Statement = 'INSERT INTO FraudRequest (CustomerId,CheckDate) SELECT CurrentCustomerId AS CustomerId, DateOfCheck AS CheckDate FROM FraudDetection GROUP BY CurrentCustomerId, DateOfCheck'
	EXEC (@Statement)

	ALTER TABLE FraudDetection ADD FraudRequestId INT NOT NULL DEFAULT(0)

	SET @Statement = 'UPDATE FraudDetection SET FraudRequestId = fr.Id FROM FraudDetection fd, FraudRequest fr WHERE fd.CurrentCustomerId=fr.CustomerId AND fd.DateOfCheck = fr.CheckDate'
	EXEC (@Statement)

	ALTER TABLE FraudDetection ADD CONSTRAINT K_FraudDetection_FraudRequest FOREIGN KEY (FraudRequestId) REFERENCES dbo.FraudRequest (Id)

	DROP INDEX FraudDetection.IX_FraudDetection_CurrentCustomerId
	ALTER TABLE FraudDetection DROP COLUMN DateOfCheck

END 
GO
