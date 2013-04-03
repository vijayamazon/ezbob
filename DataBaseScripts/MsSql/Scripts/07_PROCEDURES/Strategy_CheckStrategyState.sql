IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_CheckStrategyState]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_CheckStrategyState]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_CheckStrategyState]
(
   @pStrategyId as bigint
)

as

begin
  /* 0 - Application started,
     2 - Application finished OK,
     3 - Exception occured*/
 
      select (select count(applicationid) from application_application where
       strategyid = @pStrategyId and state not in(2,3,0)) as "ActiveAppCount",
       lastupdatedate
       from strategy_strategy
       where strategyid=@pStrategyId;

end;
GO
