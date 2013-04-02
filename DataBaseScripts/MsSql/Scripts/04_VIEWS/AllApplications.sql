IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[AllApplications]'))
DROP VIEW [dbo].[AllApplications]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[AllApplications]
AS
SELECT     aa.applicationid AS AppId, aa.appcounter AS AppCounter, aa.creationdate AS CreationDate, ss.displayname AS StrategyName, ss.StrategyId AS StrategyId, 
                      cp.name AS CreditProductName, Version, NULL AS GCRecord, aa.applicationid AS OID, 0 AS OptimisticLockField, 
                      aa.state, aa.errormsg AS ErrorMessage, 
                      ISNULL(childcount.counter, 0) ChildCount, su.userid AS UserId, su.username AS UserName, su.fullname AS UserFullName
FROM         application_application aa LEFT JOIN
                      strategy_strategy ss ON ss.strategyid = aa.strategyid LEFT JOIN
                          (SELECT     (SELECT     TOP 200 cpp.name + '' + ';'
                                                    FROM         creditproduct_products cpp, creditproduct_strategyrel cps
                                                    WHERE     cpp.id = cps.creditproductid AND cps1.strategyid = cps.strategyid
                                                    GROUP BY cpp.name FOR xml path('')) AS name, cps1.strategyid
                            FROM          creditproduct_products cpp, creditproduct_strategyrel cps1
                            WHERE      cpp.id = cps1.creditproductid
                            GROUP BY cps1.strategyid) cp ON cp.strategyid = aa.strategyid LEFT JOIN
                          (SELECT     aac.appcounter, count(aac.applicationid) AS counter
                            FROM          application_application aac
                            WHERE      aac.parentappid IS NOT NULL
                            GROUP BY aac.appcounter) childcount ON childcount.appcounter = aa.appcounter LEFT JOIN
                      security_user su ON su.userid = aa.creatoruserid
WHERE     aa.parentappid IS NULL;
GO
