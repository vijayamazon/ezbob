IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_GetShortAsgStrategies]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_GetShortAsgStrategies]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_GetShortAsgStrategies] 
	(@pPublishNameId bigint)
AS
BEGIN
	SELECT t.strategyId, t.name, t.state, p.[percent],
       t.DisplayName, t.TermDate
       ,(select TOP 1 ss.SignedDocument 
         from Strategy_PublicSign ss 
         where ss.StrategyPublicId = p.PUBLICID 
           and ss.StrategyId = p.strategyId
         ORDER BY Id DESC) as SignedDocument
       FROM STRATEGY_publicRel p, strategy_vstrategy t 
       WHERE p.strategyID = t.strategyId AND p.publicid = @pPublishNameId 
       ORDER BY p.strategyID
END
GO
