IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPrioritySignals]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPrioritySignals]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPrioritySignals]
(
    @pPriority bigint,
    @pOwnerAppId bigint,
    @pCount int 
)
AS
  
BEGIN
  update Signal 
  set Status = -1
  where id in
    (select TOP(@pCount) id 
     from Signal 
     where status= 0
       and ((priority is null) or (priority <= @pPriority))
       and ((ownerApplicationId is null) or (ownerApplicationId <= @pOwnerAppId))
       and (IsExternal is null or IsExternal = 0) 
    );

  SELECT 
    applicationId,
    appSpecific,
    id,
    label,
    priority,
    ownerApplicationId,
    executionType,
    message body
  FROM
    signal WITH (NOLOCK)
  WHERE
    status = -1
  order by priority;
END
GO
