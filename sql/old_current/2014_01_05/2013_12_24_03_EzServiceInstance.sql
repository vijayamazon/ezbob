IF OBJECT_ID('EzServiceInstance') IS NULL
BEGIN
	CREATE TABLE EzServiceInstance (
		InstanceID INT NOT NULL,
		InstanceName NVARCHAR(32) NOT NULL,
		SleepTimeout INT NOT NULL,
		AdminPort INT NOT NULL,
		ClientPort INT NOT NULL,
		HostName NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EzServiceInstance PRIMARY KEY (InstanceID),
		CONSTRAINT UC_EzServiceInstance UNIQUE (InstanceName),
		CONSTRAINT CHK_EzServiceInstance CHECK (
			(InstanceName != '') AND
			(SleepTimeout > 100) AND
			(1024 <= ClientPort) AND (ClientPort <= 65535) AND
			(1024 <= AdminPort) AND (AdminPort <= 65535)
		)
	)
END
GO

-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('EzServiceActionHistory') AND name = 'ServiceInstanceID')
BEGIN
	ALTER TABLE EzServiceActionHistory
		ADD ServiceInstanceID INT NULL
		CONSTRAINT FK_EzServiceActionHistory_Instance FOREIGN KEY (ServiceInstanceID) REFERENCES EzServiceInstance(InstanceID)
END
GO

-------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('EzServiceActionHistory') AND name = 'ServiceInstanceName')
BEGIN
	ALTER TABLE EzServiceActionHistory DROP COLUMN ServiceInstanceName
END
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('EzServiceSaveActionMetaData') IS NULL
	EXECUTE('CREATE PROCEDURE EzServiceSaveActionMetaData AS SELECT 1')
GO

ALTER PROCEDURE EzServiceSaveActionMetaData
@InstanceID INT,
@ActionName NVARCHAR(255),
@ActionID UNIQUEIDENTIFIER,
@IsSync BIT,
@Status INT,
@CurrentThreadID INT,
@UnderlyingThreadID INT,
@Comment NTEXT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ActionNameID INT

	EXECUTE EzServiceGetActionNameID @ActionName, @ActionNameID OUTPUT
	
	INSERT INTO EzServiceActionHistory (ServiceInstanceID, ActionNameID, ActionID, IsSync, ActionStatusID, CurrentThreadID, UnderlyingThreadID, Comment) VALUES
		(@InstanceID, @ActionNameID, @ActionID, @IsSync, @Status, @CurrentThreadID, @UnderlyingThreadID, @Comment)
END
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('EzServiceLoadConfiguration') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE EzServiceLoadConfiguration AS SELECT 1')
END
GO

ALTER PROCEDURE EzServiceLoadConfiguration
@InstanceName NVARCHAR(32)
AS
	SELECT
		InstanceID,
		SleepTimeout,
		AdminPort,
		ClientPort,
		HostName
	FROM
		EzServiceInstance
	WHERE
		InstanceName = @InstanceName
GO
