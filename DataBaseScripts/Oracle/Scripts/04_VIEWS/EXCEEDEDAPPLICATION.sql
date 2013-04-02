CREATE OR REPLACE VIEW EXCEEDEDAPPLICATION AS
SELECT     aa.CreationDate, aa.ApplicationId,
                          (SELECT     DisplayName
                            FROM          Strategy_Strategy ss
                            WHERE      (StrategyId = aa.StrategyId)) AS StrategyName,
                          (SELECT     cpp.Name
                            FROM          CreditProduct_Products cpp INNER JOIN
                                                   Creditproduct_Strategyrel cps ON cpp.Id = cps.CreditProductId
                            WHERE      (cps.StrategyId = aa.StrategyId)) AS CreditProductName, aa.AppCounter, NULL AS GCRecord, aa.ApplicationId AS OID, 0 AS OptimisticLockField
FROM         Application_Application aa CROSS JOIN
                      Strategy_Node sn
WHERE     (sn.ExecutionDuration IS NOT NULL OR
                      sn.ExecutionDuration <> 0) AND (aa.ParentAppID IS NULL)
GROUP BY aa.CreationDate, aa.ApplicationId, aa.StrategyId, aa.AppCounter
/