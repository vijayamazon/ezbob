IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_UpdatePublicRel]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_UpdatePublicRel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].Strategy_UpdatePublicRel
    @pPublicId bigint,
    @pStrategyId bigint,
    @pPercent decimal,
    @pAction nvarchar(7),
    @pUserId INT,
    @pSignedDocument ntext,
    @pData ntext,
	@pAllData ntext

AS
BEGIN
  update strategy_publicrel
  set [percent] = @pPercent
  where publicid = @pPublicId and strategyid = @pStrategyId;
  EXEC dbo.strategy_updatechampionstatus @pPublicId; 
  if NOT (@pData IS NULL)
    execute Strategy_PublicSignInsert @pPublicId, @pAction, @pUserId, @pSignedDocument, @pData, @pStrategyId, @pAllData

END
GO
