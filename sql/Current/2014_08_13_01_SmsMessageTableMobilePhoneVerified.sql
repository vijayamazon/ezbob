IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'MobilePhoneVerified' and Object_ID = Object_ID(N'Customer'))
BEGIN 
ALTER TABLE Customer ADD MobilePhoneVerified BIT NOT NULL DEFAULT(0)
END
GO


UPDATE Customer SET MobilePhoneVerified = 1 WHERE Id IN (SELECT c.Id FROM Customer c INNER JOIN MobileCodes m ON c.MobilePhone = m.Phone WHERE m.Active = 1)
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SmsMessage') IS NULL
BEGIN
	CREATE TABLE SmsMessage (
		Id INT IDENTITY(1,1) NOT NULL,
		UserId INT NULL,
		UnderwriterId INT NOT NULL,
		Sid NVARCHAR(255) NULL,
		DateCreated DATETIME NOT NULL,
		DateUpdated DATETIME NOT NULL,
		DateSent DATETIME NOT NULL,
		AccountSid NVARCHAR(255) NULL,
		[From] NVARCHAR(255) NULL,
		[To] NVARCHAR(255) NULL,
		Body NVARCHAR(255) NULL,
		Status NVARCHAR(255) NULL,
		Direction NVARCHAR(255) NULL,
		Price DECIMAL(18,6) NOT NULL,
		ApiVersion NVARCHAR(255) NULL,
		CONSTRAINT PK_SmsMessage PRIMARY KEY (Id),
		CONSTRAINT FK_SmsMessage_User FOREIGN KEY (UserId) REFERENCES Security_User(UserId),
		CONSTRAINT FK_SmsMessage_Underwriter FOREIGN KEY (UnderwriterId) REFERENCES Security_User(UserId)
	)
END
GO
