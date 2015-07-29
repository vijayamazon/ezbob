IF OBJECT_ID('TopUpReceiver') IS NULL
BEGIN
	CREATE TABLE [TopUpReceiver] (
		[TopUpReceiverID] INT NOT NULL IDENTITY(1,1),
		[Name] NVARCHAR(100) NULL,
		[Email] NVARCHAR(250) NULL,
		[SendMobilePhone] NVARCHAR(50) NULL,
		[PhoneOriginIsrael] Bit NULL,
		[IsActive] Bit NOT NULL
		CONSTRAINT PK_TopUpReceiver PRIMARY KEY  CLUSTERED ([TopUpReceiverID] ASC)
	)
END
GO

DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM TopUpReceiver WHERE Name = 'Underwriters')
	BEGIN
		INSERT INTO dbo.TopUpReceiver(Name, Email, SendMobilePhone, PhoneOriginIsrael, IsActive) VALUES('Underwriters', ' risk@ezbob.com', NULL, NULL, 1)
	END	

	IF NOT EXISTS (SELECT 1 FROM TopUpReceiver WHERE Name = 'Vitas Dijokas')
	BEGIN
		INSERT INTO dbo.TopUpReceiver(Name, Email, SendMobilePhone, PhoneOriginIsrael, IsActive) VALUES('Vitas Dijokas', ' vitasd@ezbob.com', '+972546970549', 1, 1)
	END	

	IF NOT EXISTS (SELECT 1 FROM TopUpReceiver WHERE Name = 'Shiri Katzir')
	BEGIN
		INSERT INTO dbo.TopUpReceiver(Name, Email, SendMobilePhone, PhoneOriginIsrael, IsActive) VALUES('Shiri Katzir', ' shirik@ezbob.com', '+972505625320', 1, 1)
	END	
END
GO

