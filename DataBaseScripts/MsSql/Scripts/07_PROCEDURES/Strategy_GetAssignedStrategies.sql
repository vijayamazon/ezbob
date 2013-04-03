IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_GetAssignedStrategies]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_GetAssignedStrategies]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].Strategy_GetAssignedStrategies
(
  @pPublishNameId bigint
)
AS
BEGIN

   SELECT 
     t.strategyId
    ,t.name
    ,t.authorName
    ,t.state
    ,p.[percent]
    ,t.description
    ,t.DisplayName
    ,t.TermDate
    ,(select TOP 1 ss.SignedDocument 
      from Strategy_PublicSign ss 
      where ss.StrategyPublicId = p.PUBLICID
        and ss.StrategyId = p.strategyId
      ORDER BY Id DESC) as SignedDocument
   FROM STRATEGY_publicRel p 
     INNER JOIN strategy_vstrategy t ON p.strategyID = t.strategyId
   WHERE p.publicid = @pPublishNameId;
END;
GO
