IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('EzServiceActionHistory') AND name = 'ActionNameID')
BEGIN
	CREATE TABLE EzServiceActionName (
		ActionNameID INT IDENTITY(1, 1) NOT NULL,
		ActionName NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EzServiceActionName PRIMARY KEY (ActionNameID)
	)

	ALTER TABLE EzServiceActionHistory
		ADD ActionNameID INT NULL
		CONSTRAINT FK_EzServiceActionHistory_Name FOREIGN KEY (ActionNameID) REFERENCES EzServiceActionName (ActionNameID)
END
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('EzServiceGetActionNameID') IS NULL
	EXECUTE('CREATE PROCEDURE EzServiceGetActionNameID AS SELECT 1')
GO

ALTER PROCEDURE EzServiceGetActionNameID
@ActionName NVARCHAR(255),
@ActionNameID INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		@ActionNameID = ActionNameID
	FROM
		EzServiceActionName
	WHERE
		ActionName = @ActionName

	IF @ActionNameID IS NULL
	BEGIN
		INSERT INTO EzServiceActionName (ActionName) VALUES (@ActionName)

		SET @ActionNameID = SCOPE_IDENTITY()
	END
END
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('EzServiceSaveActionMetaData') IS NULL
	EXECUTE('CREATE PROCEDURE EzServiceSaveActionMetaData AS SELECT 1')
GO

ALTER PROCEDURE EzServiceSaveActionMetaData
@InstanceName NVARCHAR(32),
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
	
	INSERT INTO EzServiceActionHistory (ServiceInstanceName, ActionNameID, ActionID, IsSync, ActionStatusID, CurrentThreadID, UnderlyingThreadID, Comment) VALUES
		(@InstanceName, @ActionNameID, @ActionID, @IsSync, @Status, @CurrentThreadID, @UnderlyingThreadID, @Comment)
END
GO

-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM EzServiceActionStatus WHERE ActionStatusID = 7)
	INSERT INTO EzServiceActionStatus (ActionStatusID, ActionStatusName, ActionStatusDescription) VALUES
		(7, 'BG launch', 'Underlying thread has been started.')
GO
