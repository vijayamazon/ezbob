IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyRePublish]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_StrategyRePublish]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE Strategy_StrategyRePublish
  (
    @pDisplayName NVARCHAR(max),
    @pNewStrategyId BIGINT
  )
AS

BEGIN
  DECLARE @pOldStratId BIGINT, @pOldState BIGINT;

  select @pOldStratId = strategyid, @pOldState = [state] from strategy_strategy
  where displayname = @pDisplayName and isdeleted = 0 
  and termdate = (select max(termdate) from strategy_strategy
  where displayname = @pDisplayName and isdeleted = 0);
  
  update  strategy_publicrel
  set strategyid = @pNewStrategyId
  where strategyid = @pOldStratId;
  
  update strategy_strategy 
  set state = @pOldState
  where strategyid = @pNewStrategyId;

END
GO
