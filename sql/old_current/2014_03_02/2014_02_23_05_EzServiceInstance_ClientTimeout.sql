IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('EzServiceInstance') AND name = 'ClientTimeoutSeconds')
	ALTER TABLE EzServiceInstance ADD ClientTimeoutSeconds INT NOT NULL CONSTRAINT DF_EzServiceInstance_ClientTimeout DEFAULT (60)
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('EzServiceGetDefaultInstance') IS NULL
	EXECUTE('CREATE PROCEDURE EzServiceGetDefaultInstance AS SELECT 1')
GO

-------------------------------------------------------------------------------

ALTER PROCEDURE EzServiceGetDefaultInstance
@Argument NVARCHAR(255)
AS
	SELECT
		i.InstanceID,
		i.InstanceName,
		i.SleepTimeout,
		i.AdminPort,
		i.ClientPort,
		i.HostName,
		i.ClientTimeoutSeconds
	FROM
		EzServiceInstance i
		INNER JOIN EzServiceDefaultInstance d ON i.InstanceID = d.InstanceID
	WHERE
		LOWER(d.Argument) = LOWER(@Argument)
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('EzServiceLoadConfiguration') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE EzServiceLoadConfiguration AS SELECT 1')
END
GO

-------------------------------------------------------------------------------

ALTER PROCEDURE EzServiceLoadConfiguration
@InstanceName NVARCHAR(32)
AS
	SELECT
		InstanceID,
		SleepTimeout,
		AdminPort,
		ClientPort,
		HostName,
		ClientTimeoutSeconds
	FROM
		EzServiceInstance
	WHERE
		InstanceName = @InstanceName
GO
