IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[ExceededApplication]'))
DROP VIEW [dbo].[ExceededApplication]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[ExceededApplication]
AS
SELECT     aa.CreationDate, aa.ApplicationId,
                          (SELECT     DisplayName
                            FROM          dbo.Strategy_Strategy AS ss
                            WHERE      (StrategyId = aa.StrategyId)) AS StrategyName
                            
                          ,(select cpp.name + '' + ';' 
					from creditproduct_products cpp, creditproduct_strategyrel cps
					where cpp.id = cps.creditproductid and cps.strategyid = aa.strategyid 
					group by cpp.name for xml path('')) as CreditProductName
                            , aa.AppCounter, NULL AS GCRecord, aa.ApplicationId AS OID, 0 AS OptimisticLockField
FROM         dbo.Application_Application AS aa CROSS JOIN
                      dbo.Strategy_Node AS sn
WHERE     (sn.ExecutionDuration IS NOT NULL OR
                      sn.ExecutionDuration <> 0) AND (aa.ParentAppID IS NULL)
GROUP BY aa.CreationDate, aa.ApplicationId, aa.StrategyId, aa.AppCounter
GO
