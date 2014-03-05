IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_Insert]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Application_Insert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Application_Insert] 
	(@pUserId INT,
	@pStrategyId INT,
	@pParentAppId INT,
	@pApplicationId BIGINT OUTPUT)
AS
BEGIN
	DECLARE @RootDetailID  INT,
	        @DetailNameID  INT,
	        @realCounter   BIGINT;
	
	IF @pParentAppId IS NULL
	BEGIN
	    SELECT @realCounter = aa.AppCounter + 1
	    FROM   Application_Application aa
	    WHERE  aa.ApplicationId = (
	               SELECT MAX(aa.ApplicationId)
	               FROM   Application_Application aa
	               WHERE  aa.ParentAppID IS NULL
	           )
	    
	    IF (@realCounter IS NULL)
	        SET @realCounter = 1;
	END
	ELSE
	BEGIN
	    SELECT @realCounter = ISNULL(aa.AppCounter, 1)
	    FROM   Application_Application aa
	    WHERE  aa.ApplicationId = @pParentAppId;
	END
	
	INSERT INTO Application_Application
	  (
	    CreatorUserId,
	    StrategyId,
	    ParentAppId,
	    AppCounter
	  )
	VALUES
	  (
	    @pUserId,
	    @pStrategyId,
	    @pParentAppId,
	    @realCounter
	  )
	
	SET @pApplicationId = @@IDENTITY
	
	SELECT @DetailNameID = DetailNameId
	FROM   dbo.Application_DetailName
	WHERE  [Name] = 'Root'
	
	INSERT INTO [Application_Detail]
	  (
	    [ApplicationId],
	    [DetailNameId],
	    [ParentDetailId],
	    [ValueStr],
	    [ValueNum],
	    [ValueDateTime],
	    [IsBinary]
	  )
	VALUES
	  (
	    @pApplicationId,
	    @DetailNameID,
	    NULL,
	    NULL,
	    NULL,
	    NULL,
	    NULL
	  ); 
	SET @RootDetailID = @@identity
	
	SELECT @DetailNameID = DetailNameId
	FROM   dbo.Application_DetailName
	WHERE  [Name] = 'Body'
	
	INSERT INTO [Application_Detail]
	  (
	    [ApplicationId],
	    [DetailNameId],
	    [ParentDetailId],
	    [ValueStr],
	    [ValueNum],
	    [ValueDateTime],
	    [IsBinary]
	  )
	VALUES
	  (
	    @pApplicationId,
	    @DetailNameID,
	    @RootDetailID,
	    NULL,
	    NULL,
	    NULL,
	    NULL
	  ); 
	
	SELECT @pApplicationId
END
GO
