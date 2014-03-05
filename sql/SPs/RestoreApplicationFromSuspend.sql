IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RestoreApplicationFromSuspend]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RestoreApplicationFromSuspend]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RestoreApplicationFromSuspend] 
	(@pApplicationId BIGINT)
AS
BEGIN
	DECLARE @AppSpecific int;

        DELETE FROM [STRATEGYENGINE_EXECUTIONSTATE] WHERE ApplicationId = @pApplicationId;
        INSERT INTO [STRATEGYENGINE_EXECUTIONSTATE]
                       ([APPLICATIONID]
                       ,[CURRENTNODEID]
                       ,[CURRENTNODEPOSTFIX]
                       ,[DATA]
                       ,[STARTTIME]
                       ,[ISTIMEOUTREPORTED])
        SELECT 
                        [ApplicationId]
                       ,null
                       ,[Postfix]
                       ,[ExecutionState]
                       ,GETDATE()
                       ,null
        FROM Application_Suspended
        WHERE ApplicationId = @pApplicationId
          AND NOT(Postfix is null OR ExecutionState is null);

        SELECT @AppSpecific = [Version]
        FROM Application_Application
        WHERE ApplicationId = @pApplicationId
          
        DELETE FROM [SIGNAL] WHERE ApplicationId = @pApplicationId;
        INSERT INTO [SIGNAL]
                       ([TARGET]
                       ,[LABEL]
                       ,[STATUS]
                       ,[STARTTIME]
                       ,[APPSPECIFIC]
                       ,[APPLICATIONID]
                       ,[ExecutionType]
                       ,[MESSAGE])
        SELECT 
                        [Target]
                       ,[Label]
                       ,0
                       ,GETDATE()
                       ,@AppSpecific
                       ,[ApplicationId]
                       ,[ExecutionType]
                       ,[Message]
        FROM Application_Suspended
        WHERE ApplicationId = @pApplicationId;

        DELETE
        FROM Application_Suspended
        WHERE ApplicationId = @pApplicationId;

        UPDATE Application_Application
        SET State = 0
        WHERE ApplicationId = @pApplicationId
END
GO
