CREATE OR REPLACE VIEW ALLLOCKEDAPPLICATIONS AS
SELECT     aa.ApplicationId, aa.AppCounter, aa.CreationDate,
                          (SELECT     MAX(ActionDateTime) AS Expr1
                            FROM          Application_History ah
                            WHERE      (ApplicationId = aa.ApplicationId) AND (UserId = aa.LockedByUserId) AND (CurrentNodeID = sn.NodeId) AND (ActionType = 0)) AS LockedDate, aa.Version,
                      sn.Name AS NodeName, sn.DisplayName AS NodeDisplayName, sn.NodeId, cu.UserId AS CreatorUserId, cu.UserName AS CreatorUserName,
                      cu.FullName AS CreatorUserFullName, lu.UserId AS LockedByUserId, lu.UserName AS LockedByUserName, lu.FullName AS LockedByUserFullName,
                      ss.DisplayName AS strategyname, ss.StrategyId, cp.Name AS CreditProductName, NULL AS GCRecord, aa.ApplicationId AS OID, 0 AS OptimisticLockField,
                      0 AS Checked
FROM         Application_Application aa LEFT OUTER JOIN
                      StrategyEngine_ExecutionState se ON aa.ApplicationId = se.ApplicationId LEFT OUTER JOIN
                      Strategy_Node sn ON sn.NodeId = se.CurrentNodeId LEFT OUTER JOIN
                      Strategy_Strategy ss ON ss.StrategyId = aa.StrategyId LEFT OUTER JOIN
                      Creditproduct_Strategyrel cps ON cps.StrategyId = aa.StrategyId LEFT OUTER JOIN
                      CreditProduct_Products cp ON cp.Id = cps.CreditProductId LEFT OUTER JOIN
                      Security_User cu ON cu.UserId = aa.CreatorUserId LEFT OUTER JOIN
                      Security_User lu ON lu.UserId = aa.LockedByUserId
WHERE     (aa.LockedByUserId IS NOT NULL)
/
