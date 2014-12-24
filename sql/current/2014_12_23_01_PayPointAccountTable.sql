IF object_id('PayPointAccount') IS NULL
BEGIN
	CREATE TABLE PayPointAccount(
		PayPointAccountID INT NOT NULL IDENTITY(1,1),
		IsDefault BIT NOT NULL,
		Mid NVARCHAR(30) NOT NULL,
		VpnPassword NVARCHAR(30) NOT NULL,
		RemotePassword NVARCHAR(30) NOT NULL,
		Options NVARCHAR(100) NULL,
		TemplateUrl NVARCHAR(300) NOT NULL,
		ServiceUrl NVARCHAR(300) NOT NULL,
		EnableCardLimit BIT NOT NULL,
		CardLimitAmount INT NOT NULL,
		CardExpiryMonths INT NOT NULL,
		DebugModeEnabled BIT NOT NULL,
		DebugModeErrorCodeNEnabled BIT NOT NULL,
		DebugModeIsValidCard BIT NOT NULL,
		CONSTRAINT PK_PayPointAccount PRIMARY KEY (PayPointAccountID)
	)
END
GO

IF EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointMid')
BEGIN
	DECLARE @Mid NVARCHAR(30) = (SELECT Value FROM ConfigurationVariables WHERE Name = 'PayPointMid')
	DECLARE @VpnPassword NVARCHAR(30) = (SELECT Value FROM ConfigurationVariables WHERE Name = 'PayPointVpnPassword')
	DECLARE @RemotePassword NVARCHAR(30) = (SELECT Value FROM ConfigurationVariables WHERE Name = 'PayPointRemotePassword')
	DECLARE @Options NVARCHAR(100) = (SELECT Value FROM ConfigurationVariables WHERE Name = 'PayPointOptions')
	DECLARE @TemplateUrl NVARCHAR(300) = (SELECT Value FROM ConfigurationVariables WHERE Name = 'PayPointTemplateUrl')
	DECLARE @ServiceUrl NVARCHAR(300) = (SELECT Value FROM ConfigurationVariables WHERE Name = 'PayPointServiceUrl')
	DECLARE @DebugMode BIT = (SELECT CAST((CASE WHEN Value IN ('True', 'true', '1') THEN 1 ELSE 0 END) AS BIT) FROM ConfigurationVariables WHERE Name = 'PayPointDebugMode')
	DECLARE @ValidCard BIT = (SELECT CAST((CASE WHEN Value IN ('True', 'true', '1') THEN 1 ELSE 0 END) AS BIT) FROM ConfigurationVariables WHERE Name = 'PayPointIsValidCard')
	DECLARE @EnableCardLimit BIT = (SELECT CAST((CASE WHEN Value IN ('True', 'true', '1') THEN 1 ELSE 0 END) AS BIT) FROM ConfigurationVariables WHERE Name = 'PayPointEnableCardLimit')
	DECLARE @CardLimitAmount INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'PayPointCardLimitAmount')
	DECLARE @EnableDebugErrorCodeN BIT = (SELECT CAST((CASE WHEN Value IN ('True', 'true', '1') THEN 1 ELSE 0 END) AS BIT) FROM ConfigurationVariables WHERE Name = 'PayPointEnableDebugErrorCodeN')
	DECLARE @CardExpiryMonths INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'PayPointCardExpiryMonths')
	
	INSERT INTO PayPointAccount(IsDefault,Mid,VpnPassword,RemotePassword,Options,TemplateUrl,ServiceUrl,EnableCardLimit,CardLimitAmount,CardExpiryMonths,DebugModeEnabled,DebugModeIsValidCard,DebugModeErrorCodeNEnabled)
	VALUES (1, @Mid,@VpnPassword,@RemotePassword,@Options,@TemplateUrl,@ServiceUrl,@EnableCardLimit,@CardLimitAmount,@CardExpiryMonths,@DebugMode,@ValidCard,@EnableDebugErrorCodeN)
	
	DELETE FROM ConfigurationVariables WHERE Name IN ('PayPointCardExpiryMonths','PayPointCardLimitAmount','PayPointDebugMode','PayPointEnableCardLimit','PayPointEnableDebugErrorCodeN','PayPointIsValidCard','PayPointMid','PayPointOptions','PayPointRemotePassword','PayPointServiceUrl','PayPointTemplateUrl','PayPointVpnPassword')	

END 
GO

IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id = object_id('PayPointCard') AND name='PayPointAccountID')
BEGIN
	ALTER TABLE PayPointCard ADD PayPointAccountID INT
	ALTER TABLE PayPointCard ADD CONSTRAINT FK_PayPointCard_PayPointAccount FOREIGN KEY (PayPointAccountID) REFERENCES PayPointAccount(PayPointAccountID)
	ALTER TABLE PayPointCard ADD IsDefaultCard BIT NOT NULL DEFAULT(0)
END
GO

IF NOT EXISTS (SELECT 1 FROM PayPointCard WHERE IsDefaultCard = 1)
BEGIN
	UPDATE PayPointCard SET IsDefaultCard = 1
	FROM Customer c INNER JOIN PayPointCard p ON c.PayPointTransactionId = p.TransactionId
	
	UPDATE PayPointCard SET PayPointAccountID = 1 WHERE PayPointAccountID IS NULL
END
GO
