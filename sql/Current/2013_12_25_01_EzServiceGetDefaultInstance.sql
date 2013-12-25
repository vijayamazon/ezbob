IF OBJECT_ID('EzServiceDefaultInstance') IS NULL
BEGIN
	CREATE TABLE EzServiceDefaultInstance (
		EntryID INT IDENTITY(1, 1) NOT NULL,
		Argument NVARCHAR(255) NOT NULL,
		InstanceID INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EzServiceDefaultInstance PRIMARY KEY (EntryID),
		CONSTRAINT FK_EzServiceDefaultInstance FOREIGN KEY (InstanceID) REFERENCES EzServiceInstance (InstanceID)
	)
END
GO

IF OBJECT_ID('EzServiceGetDefaultInstance') IS NULL
	EXECUTE('CREATE PROCEDURE EzServiceGetDefaultInstance AS SELECT 1')
GO

ALTER PROCEDURE EzServiceGetDefaultInstance
@Argument NVARCHAR(255)
AS
	SELECT
		i.InstanceID,
		i.InstanceName,
		i.SleepTimeout,
		i.AdminPort,
		i.ClientPort,
		i.HostName
	FROM
		EzServiceInstance i
		INNER JOIN EzServiceDefaultInstance d ON i.InstanceID = d.InstanceID
	WHERE
		LOWER(d.Argument) = LOWER(@Argument)
GO
