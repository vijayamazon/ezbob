IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('EzServiceActionHistory') AND name = 'UserID')
BEGIN
	-- Relevant foreign keys are not added intentionally:
	-- when these keys exist Sign up process got stuck because of lock.

	ALTER TABLE EzServiceActionHistory
		ADD UserID INT NULL

	ALTER TABLE EzServiceActionHistory
		ADD CustomerID INT NULL
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
@Comment NTEXT = NULL,
@UserID INT = NULL,
@CustomerID INT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ActionNameID INT

	EXECUTE EzServiceGetActionNameID @ActionName, @ActionNameID OUTPUT
	
	INSERT INTO EzServiceActionHistory (ServiceInstanceID, ActionNameID, ActionID, IsSync, ActionStatusID, CurrentThreadID, UnderlyingThreadID, Comment, UserID, CustomerID) VALUES
		(@InstanceID, @ActionNameID, @ActionID, @IsSync, @Status, @CurrentThreadID, @UnderlyingThreadID, @Comment, @UserID, @CustomerID)
END
GO
