IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyScheduleParam_Insert]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyScheduleParam_Insert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyScheduleParam_Insert] 
	(@pScheduleId int
                  , @pName nvarchar(100)
                  , @pDescription nvarchar(max)
                  , @pUserId int
                  , @pData nvarchar(max)
                  , @pSignedDocument nvarchar(max))
AS
BEGIN
	DECLARE @pParameterTypeID int;
   
     SELECT @pParameterTypeID = NULL;
     
     SELECT TOP 1 @pParameterTypeID = Id
     FROM Strategy_ScheduleParam
     WHERE [Name] = @pName 
       AND Deleted IS null
       AND StrategyScheduleId = @pScheduleId;

     IF @pParameterTypeID IS NULL
      INSERT INTO [dbo].[Strategy_ScheduleParam]
           ([StrategyScheduleId]
           ,[CurrentVersionId]
           ,[Name]
           ,[Description]
           ,[Data]
           ,[UserId]
           ,[Deleted]
           ,[SignedDocument])
      VALUES
           (@pScheduleId
           ,0
           ,@pName
           ,@pDescription
           ,@pData
           ,@pUserId
           ,null
           ,@pSignedDocument)

     ELSE
      begin
        UPDATE Strategy_ScheduleParam
        SET Deleted = @pParameterTypeID
        WHERE Id = @pParameterTypeID;

        INSERT INTO [dbo].[Strategy_ScheduleParam]
           ([StrategyScheduleId]
           ,[CurrentVersionId]
           ,[Name]
           ,[Description]
           ,[Data]
           ,[UserId]
           ,[Deleted]
           ,[SignedDocument])
        VALUES
           (@pScheduleId
           ,0
           ,@pName
           ,@pDescription
           ,@pData
           ,@pUserId
           ,null
           ,@pSignedDocument)
      end;

     SELECT SCOPE_IDENTITY()
END
GO
