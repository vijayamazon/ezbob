IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_CheckStrategyState]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_CheckStrategyState]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_CheckStrategyState] 
	(@pStrategyId as bigint)
AS
BEGIN
	select (select count(applicationid) from application_application where
       strategyid = @pStrategyId and state not in(2,3,0)) as "ActiveAppCount",
       lastupdatedate
       from strategy_strategy
       where strategyid=@pStrategyId
END
GO
