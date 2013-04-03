IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationState]'))
DROP VIEW [dbo].[ApplicationState]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[ApplicationState]
AS
SELECT     ApplicationId, AppCounter, CreationDate, State, CurrentNodeName, CurrentNodeId, StrategyName, CreditProductName, Version, ChildCount, UserId, userName, 
                      UserFullName, NULL AS GCRecord, ApplicationId AS OID, 0 AS OptimisticLockField
FROM         (SELECT     a.ApplicationId, a.AppCounter, a.CreationDate, a.State, ISNULL
                                                  ((SELECT     DisplayName
                                                      FROM         dbo.Strategy_Node AS sn
                                                      WHERE     (NodeId = idn.CurrentNodeId)), 'SE') AS CurrentNodeName,
                                                  (SELECT     NodeId
                                                    FROM          dbo.Strategy_Node AS sn
                                                    WHERE      (NodeId = idn.CurrentNodeId)) AS CurrentNodeId,
                                                  (SELECT     DisplayName
                                                    FROM          dbo.Strategy_Strategy AS ss
                                                    WHERE      (StrategyId = a.StrategyId)) AS StrategyName
                                                    ,(select cpp.name + '' + ';' 
					from creditproduct_products cpp, creditproduct_strategyrel cps
					where cpp.id = cps.creditproductid and cps.strategyid = a.strategyid 
					group by cpp.name for xml path('')) as CreditProductName
                                                    , a.Version, ISNULL(a.ChildCount, 0) AS ChildCount,
                                                  (SELECT     UserId
                                                    FROM          dbo.Security_User AS su
                                                    WHERE      (UserId = a.CreatorUserId)) AS UserId,
                                                  (SELECT     UserName
                                                    FROM          dbo.Security_User AS su
                                                    WHERE      (UserId = a.CreatorUserId)) AS userName,
                                                  (SELECT     FullName
                                                    FROM          dbo.Security_User AS su
                                                    WHERE      (UserId = a.CreatorUserId)) AS UserFullName
                       FROM          (SELECT     MAX(Id) AS id, ApplicationId
                                               FROM          dbo.StrategyEngine_ExecutionState
                                               GROUP BY ApplicationId) AS ids INNER JOIN
                                              dbo.StrategyEngine_ExecutionState AS s ON ids.id = s.Id INNER JOIN
                                                  (SELECT     MAX(Id) AS id, ApplicationId, CurrentNodeId
                                                    FROM          dbo.StrategyEngine_ExecutionState AS StrategyEngine_ExecutionState_1
                                                    GROUP BY ApplicationId, CurrentNodeId) AS idn ON s.Id = idn.id INNER JOIN
                                              dbo.Application_Application AS a ON ids.ApplicationId = a.ApplicationId INNER JOIN
                                              dbo.Strategy_Strategy AS t ON a.StrategyId = t.StrategyId
                       WHERE      (a.ParentAppID IS NULL)
                       UNION ALL
                       SELECT     a.ApplicationId, a.AppCounter, a.CreationDate, a.State, ISNULL
                                                 ((SELECT     sn.Name
                                                     FROM         dbo.Application_History AS ah INNER JOIN
                                                                           dbo.Strategy_Node AS sn ON ah.CurrentNodeID = sn.NodeId
                                                     WHERE     (ah.ApplicationId = a.ApplicationId) AND (ah.AppHistoryId =
                                                                               (SELECT     MAX(AppHistoryId) AS Expr1
                                                                                 FROM          dbo.Application_History
                                                                                 WHERE      (ApplicationId = ah.ApplicationId)))), 'SE') AS CurrentNodeName,
                                                 (SELECT     ah.CurrentNodeID
                                                   FROM          dbo.Application_History AS ah INNER JOIN
                                                                          dbo.Strategy_Node AS sn ON ah.CurrentNodeID = sn.NodeId
                                                   WHERE      (ah.ApplicationId = a.ApplicationId) AND (ah.AppHistoryId =
                                                                              (SELECT     MAX(AppHistoryId) AS Expr1
                                                                                FROM          dbo.Application_History AS Application_History_1
                                                                                WHERE      (ApplicationId = ah.ApplicationId)))) AS CurrentNodeId,
                                                 (SELECT     DisplayName
                                                   FROM          dbo.Strategy_Strategy AS ss
                                                   WHERE      (StrategyId = a.StrategyId)) AS StrategyName,
                                                 (SELECT     cpp.Name
                                                   FROM          dbo.CreditProduct_Products AS cpp INNER JOIN
                                                                          dbo.Creditproduct_Strategyrel AS cps ON cpp.Id = cps.CreditProductId
                                                   WHERE      (cps.StrategyId = a.StrategyId)) AS CreditProductName, a.Version, ISNULL(a.ChildCount, 0) AS ChildCount,
                                                 (SELECT     UserId
                                                   FROM          dbo.Security_User AS su
                                                   WHERE      (UserId = a.CreatorUserId)) AS UserId,
                                                 (SELECT     UserName
                                                   FROM          dbo.Security_User AS su
                                                   WHERE      (UserId = a.CreatorUserId)) AS UserName,
                                                 (SELECT     FullName
                                                   FROM          dbo.Security_User AS su
                                                   WHERE      (UserId = a.CreatorUserId)) AS UserFullName
                       FROM         dbo.Application_Application AS a INNER JOIN
                                             dbo.Application_Suspended AS susp ON a.ApplicationId = susp.ApplicationId
                       WHERE     (a.State = 4 OR
                                             a.State = 5 OR
                                             a.State = 6 OR
                                             a.State = 7) AND (a.ParentAppID IS NULL)) AS allapp
GO
