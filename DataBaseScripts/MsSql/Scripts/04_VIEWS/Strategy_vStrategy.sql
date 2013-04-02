IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_vStrategy]'))
DROP VIEW [dbo].[Strategy_vStrategy]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Strategy_vStrategy]
AS
SELECT     dbo.Strategy_Strategy.StrategyId, dbo.Strategy_Strategy.CurrentVersionId, dbo.Strategy_Strategy.Name, dbo.Strategy_Strategy.Description, 
                      dbo.Strategy_Strategy.Icon, dbo.Strategy_Strategy.IsEmbeddingAllowed, dbo.Strategy_Strategy.UserId, dbo.Strategy_Strategy.AuthorId, 
                      dbo.Strategy_Strategy.State, dbo.Strategy_Strategy.SubState, dbo.Strategy_Strategy.StrategyType,
                      dbo.Strategy_Strategy.IsDeleted, dbo.Security_User.FullName, 
                      User_1.FullName AS AuthorName, dbo.Strategy_Strategy.ExecutionDuration,
                      dbo.Strategy_Strategy.DisplayName, dbo.Strategy_Strategy.TermDate,
                          (SELECT     COUNT(STRATEGYID) AS Expr1
                            FROM          dbo.STRATEGY_PUBLICREL
                            WHERE      (STRATEGYID = dbo.Strategy_Strategy.StrategyId)) AS STRATEGYCOUNT
FROM         dbo.Strategy_Strategy LEFT OUTER JOIN
                      dbo.Security_User ON dbo.Strategy_Strategy.UserId = dbo.Security_User.UserId LEFT OUTER JOIN
                      dbo.Security_User AS User_1 ON dbo.Strategy_Strategy.AuthorId = User_1.UserId
GO
