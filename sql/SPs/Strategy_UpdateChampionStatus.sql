IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_UpdateChampionStatus]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_UpdateChampionStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_UpdateChampionStatus] 
	(@pPublicId bigint)
AS
BEGIN
	declare @stratId as int;

    update strategy_strategy
        set state = 0
      where strategyid in (select strategyid from strategy_publicrel
   where publicid = @pPublicId);

  select TOP(1) @stratId = strategyId  from strategy_publicrel
   where publicid = @pPublicId
   and [percent] = (select MAX([percent]) from strategy_publicrel
   where publicid = @pPublicId);
      
   if @stratId > 0 begin
     update strategy_strategy
        set state = 1
      where strategyid = @stratId;
   end
END
GO
