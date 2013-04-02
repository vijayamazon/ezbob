IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyPublish]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_StrategyPublish]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_StrategyPublish]
(
    @pStrategyId bigint,
    @pPublishNameId bigint,
    @pPercent decimal,
    @pAction nvarchar(7),
    @pUserId INT,
    @pSignedDocument ntext,
    @pData ntext,
	@pAllData ntext
)
AS
BEGIN
  IF NOT EXISTS(SELECT * from strategy_publicrel  WHERE strategyid = @pStrategyId AND
  publicid = @pPublishNameId)
  BEGIN
  	 insert into strategy_publicrel
    (publicid, strategyid, [percent])
	values
    (@pPublishNameId, @pStrategyId, @pPercent);
    
    EXEC strategy_updatechampionstatus @pPublishNameId;
    if NOT (@pData IS NULL)
      execute Strategy_PublicSignInsert @pPublishNameId, @pAction, @pUserId, @pSignedDocument, @pData, @pStrategyId, @pAllData

  END
END
GO
