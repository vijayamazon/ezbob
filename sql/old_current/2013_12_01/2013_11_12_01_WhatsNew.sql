IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WhatsNew]') AND type IN (N'U'))
BEGIN
	CREATE TABLE WhatsNew (
		Id INT IDENTITY(1,1) NOT NULL 
	  , WhatsNew NVARCHAR(MAX) NOT NULL
	  , ValidFrom DATETIME NOT NULL
	  , ValidUntil DATETIME NOT NULL
	  , Active BIT NOT NULL
	  , CONSTRAINT PK_WhatsNew PRIMARY KEY (Id)
	)
END

GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WhatsNewCustomerMap]') AND type IN (N'U'))
BEGIN
	CREATE TABLE WhatsNewCustomerMap (
		Id INT IDENTITY(1,1) NOT NULL 
	  , WhatsNewId INT NOT NULL
	  , CustomerId INT NOT NULL	
	  , Date DATETIME NOT NULL
	  , Understood BIT NOT NULL
	  , CONSTRAINT PK_WhatsNewCustomerMap PRIMARY KEY (Id)
	  , CONSTRAINT FK_WhatsNewCustomerMap_WhatsNew FOREIGN KEY (WhatsNewId) REFERENCES WhatsNew (Id)
	  , CONSTRAINT FK_WhatsNewCustomerMap_Customer FOREIGN KEY (CustomerId) REFERENCES Customer (Id)
	)
END

GO


	  
	  
