IF OBJECT_ID ('dbo.ExperianDefaultAccountsData') IS NOT NULL
BEGIN
	PRINT 'ExperianDefaultAccountsData exists'
	DROP TABLE ExperianDefaultAccountsData
END

CREATE TABLE dbo.ExperianDefaultAccountsData
(
	Id INT IDENTITY, -- Internal Default Account Id
	ServiceLogId BIGINT NOT NULL, 
	CustomerId INT NOT NULL, 
	AccountType VARCHAR(3),
	DefMonth DATETIME, 
	Balance INT, 
	CurrentDefBalance INT,
	CONSTRAINT FK_ExperianDefaultAccountsData_ServiceLogId FOREIGN KEY (ServiceLogId) REFERENCES dbo.MP_ServiceLog (Id),
	CONSTRAINT FK_ExperianDefaultAccountsData_CustomerId FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id),
	CONSTRAINT FK_ExperianDefaultAccountsData_ExperianAccountType FOREIGN KEY (AccountType) REFERENCES dbo.ExperianAccountTypes (Id)
)
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
