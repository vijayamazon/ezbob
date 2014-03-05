IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_AssignEmbedded]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_AssignEmbedded]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_AssignEmbedded] 
	(@pStrategyId int,
    @pEmbStrategyName nvarchar(max))
AS
BEGIN
	DECLARE @embStrategyId as int;

 select @embStrategyId = strategyid  from strategy_strategy
  where [name] = @pEmbStrategyName and isdeleted = 0;
  
    delete from strategy_embededrel
    where strategyid = @pStrategyId and embstrategyid=@embStrategyId;
	
	if (@pStrategyId <> @embStrategyId)
	begin
		insert into strategy_embededrel
		  (strategyid, embstrategyid)
		values
		  (@pStrategyId, @embStrategyId);
	end
END
GO
