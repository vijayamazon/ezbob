IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EzServiceSaveActionMetaData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[EzServiceSaveActionMetaData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EzServiceSaveActionMetaData] 
	(@InstanceID INT,
@ActionName NVARCHAR(255),
@ActionID UNIQUEIDENTIFIER,
@IsSync BIT,
@Status INT,
@CurrentThreadID INT,
@UnderlyingThreadID INT,
@Comment NTEXT = NULL,
@UserID INT = NULL,
@CustomerID INT = NULL)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ActionNameID INT

	EXECUTE EzServiceGetActionNameID @ActionName, @ActionNameID OUTPUT
	
	INSERT INTO EzServiceActionHistory (ServiceInstanceID, ActionNameID, ActionID, IsSync, ActionStatusID, CurrentThreadID, UnderlyingThreadID, Comment, UserID, CustomerID) VALUES
		(@InstanceID, @ActionNameID, @ActionID, @IsSync, @Status, @CurrentThreadID, @UnderlyingThreadID, @Comment, @UserID, @CustomerID)
END
GO
