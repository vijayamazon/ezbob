IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Customer') AND name = 'Vip')
BEGIN
	ALTER TABLE Customer ADD Vip BIT NOT NULL DEFAULT(0)
END
GO

IF OBJECT_ID('VipRequest') IS NULL
BEGIN
CREATE TABLE VipRequest(
	Id INT IDENTITY(1,1) NOT NULL,
	RequestDate DATETIME NOT NULL,
	CustomerId INT,
	Ip NVARCHAR(30),
	Email NVARCHAR(300),
	FullName NVARCHAR(50),
	Phone NVARCHAR(12)
	CONSTRAINT PK_VipRequest PRIMARY KEY (Id),	
	CONSTRAINT FK_VipRequest_Customer FOREIGN KEY (CustomerId) REFERENCES Customer(Id)
)
END 
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='VipEnabled')
BEGIN
	INSERT INTO ConfigurationVariables(Name,Value,Description) VALUES ('VipEnabled', 'true', 'true if vip request enabled else false')
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='VipMaxRequests')
BEGIN
	INSERT INTO ConfigurationVariables(Name,Value,Description) VALUES ('VipMaxRequests', '2', 'max num of vip requests per Ip allowed')
END
GO

DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment <> 'Prod'
BEGIN

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='VipMailReceiver')
BEGIN
	INSERT INTO ConfigurationVariables(Name,Value,Description) VALUES ('VipMailReceiver', '', 'email of the receiver of the VIP requests')
END

END

IF @Environment = 'Prod'
BEGIN

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='VipMailReceiver')
BEGIN
	INSERT INTO ConfigurationVariables(Name,Value,Description) VALUES ('VipMailReceiver', 'sales@ezbob.com', 'email of the receiver of the VIP requests')
END

END
GO

IF EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='VipMailReceiver' AND Description='max num of vip requests per Ip allowed')
BEGIN
	UPDATE ConfigurationVariables SET Description = 'email of the receiver of the VIP requests' WHERE Name='VipMailReceiver'
END 
GO
