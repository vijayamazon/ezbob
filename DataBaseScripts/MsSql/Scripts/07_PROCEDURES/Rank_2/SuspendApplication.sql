IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SuspendApplication]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SuspendApplication]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [SuspendApplication]
	@pApplicationId         BIGINT,
	@pExecutionState        ntext,
	@pPostfix               NVARCHAR(512),
	@pTarget                NVARCHAR(50),
	@pLabel                 NVARCHAR(250),
	@pExecutionPathCurrentItemId  BIGINT,
	@pExecutionType         SMALLINT,
	@pMessage               varbinary(max)
AS
BEGIN

        DECLARE @pAppSpecific bigint;
        DECLARE @pStrategyId int;

        exec DeleteSignalByAppId @pApplicationId, @pAppSpecific OUTPUT, @pStrategyId OUTPUT;
		
		DECLARE @pCurrentNodeId int;
		exec DeleteExecutionStateByAppId @pApplicationId, @pCurrentNodeId OUTPUT;

        INSERT INTO [Application_Suspended]
               ([ApplicationId]
               ,[ExecutionState]
               ,[Postfix]
               ,[Target]
               ,[Label]
               ,[ExecutionPathCurrentItemId]
               ,[ExecutionType]
               ,[Message]
               ,[AppSpecific]
               ,[Date])
        VALUES 
               ( @pApplicationId
               , @pExecutionState
               , @pPostfix
               , @pTarget
               , @pLabel
               , @pExecutionPathCurrentItemId
               , @pExecutionType
               , @pMessage
               , @pAppSpecific
               , GETDATE());


        SELECT @@IDENTITY;
END
GO
