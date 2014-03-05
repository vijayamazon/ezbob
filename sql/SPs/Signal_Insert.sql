IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Signal_Insert]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Signal_Insert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Signal_Insert] 
	(@pTarget        nvarchar(50),
    @pLabel         nvarchar(250),
    @pAppSpecific   int,
    @pApplicationId bigint,
    @pPriority      bigint,
    @pOwnerAppId    bigint,
    @pMessage       varbinary(max))
AS
BEGIN
	declare @executionType bigint;
  select @executionType = ExecutionType
  from Application_ExecutionType WITH(NOLOCK)
  where ApplicationId = @pApplicationId;
  insert into Signal
    (Target, Label, Status, StartTime, AppSpecific, ApplicationId, Priority, OwnerApplicationId, Message, ExecutionType)
  values
    (@pTarget, @pLabel, 0, getdate(), @pAppSpecific, @pApplicationId, @pPriority, @pOwnerAppId, @pMessage, @executionType)
END
GO
