IF OBJECT_ID('EzServiceSaveActionMetaData') IS NULL
	EXECUTE('CREATE PROCEDURE EzServiceSaveActionMetaData AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE EzServiceSaveActionMetaData
@Now DATETIME,
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
	
	INSERT INTO EzServiceActionHistory (
		EntryTime,
		ServiceInstanceID,
		ActionNameID,
		ActionID,
		IsSync,
		ActionStatusID,
		CurrentThreadID,
		UnderlyingThreadID,
		Comment,
		UserID,
		CustomerID
	) VALUES (
		@Now,
		@InstanceID,
		@ActionNameID,
		@ActionID,
		@IsSync,
		@Status,
		@CurrentThreadID,
		@UnderlyingThreadID,
		@Comment,
		@UserID,
		@CustomerID
	)
END
GO
