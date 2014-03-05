IF OBJECT_ID (N'dbo.AllLockedApplications') IS NOT NULL
	DROP VIEW dbo.AllLockedApplications
GO

CREATE VIEW [dbo].[AllLockedApplications]
AS
SELECT     aa.ApplicationId, aa.AppCounter, aa.CreationDate,
                          (SELECT     MAX(ActionDateTime) AS Expr1
                            FROM          dbo.Application_History AS ah
                            WHERE      (ApplicationId = aa.ApplicationId) AND (UserId = aa.LockedByUserId) AND (CurrentNodeID = sn.NodeId) AND (ActionType = 0)) AS LockedDate, aa.Version, 
                      sn.Name AS NodeName, sn.DisplayName AS NodeDisplayName, sn.NodeId, cu.UserId AS CreatorUserId, cu.UserName AS CreatorUserName, 
                      cu.FullName AS CreatorUserFullName, lu.UserId AS LockedByUserId, lu.UserName AS LockedByUserName, lu.FullName AS LockedByUserFullName, 
                      ss.DisplayName AS strategyname, ss.StrategyId
                      ,(select cpp.name + '' + ';' 
					from creditproduct_products cpp, creditproduct_strategyrel cps
					where cpp.id = cps.creditproductid and cps.strategyid = aa.strategyid 
					group by cpp.name for xml path('')) as CreditProductName
                      , 
                      NULL AS GCRecord, aa.ApplicationId AS OID, 0 AS OptimisticLockField, 
                      0 AS Checked
FROM         dbo.Application_Application AS aa LEFT OUTER JOIN
                      dbo.StrategyEngine_ExecutionState AS se ON aa.ApplicationId = se.ApplicationId LEFT OUTER JOIN
                      dbo.Strategy_Node AS sn ON sn.NodeId = se.CurrentNodeId LEFT OUTER JOIN
                      dbo.Strategy_Strategy AS ss ON ss.StrategyId = aa.StrategyId LEFT OUTER JOIN
                      dbo.Security_User AS cu ON cu.UserId = aa.CreatorUserId LEFT OUTER JOIN
                      dbo.Security_User AS lu ON lu.UserId = aa.LockedByUserId
WHERE     (aa.LockedByUserId IS NOT NULL)

GO

