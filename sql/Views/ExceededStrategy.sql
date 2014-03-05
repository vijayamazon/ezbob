IF OBJECT_ID (N'dbo.ExceededStrategy') IS NOT NULL
	DROP VIEW dbo.ExceededStrategy
GO

CREATE VIEW [dbo].[ExceededStrategy]
AS
SELECT     CreationDate, ApplicationId, AppCounter, StrategyName, CreditProductName, minCommingTime, exitTime, ValueOfLimit, commingExitTime, exceededTime, NULL 
                      AS GCRecord, ApplicationId AS OID, 0 AS OptimisticLockField
FROM         (SELECT     aa.CreationDate, aa.ApplicationId, aa.AppCounter,
                                                  (SELECT     DisplayName
                                                    FROM          dbo.Strategy_Strategy AS ss
                                                    WHERE      (StrategyId = aa.StrategyId)) AS StrategyName
                                                    ,(select cpp.name + '' + ';' 
					from creditproduct_products cpp, creditproduct_strategyrel cps
					where cpp.id = cps.creditproductid and cps.strategyid = aa.strategyid 
					group by cpp.name for xml path('')) as CreditProductName
                                                    ,
                                                  (SELECT     MIN(an.ComingTime) AS Expr1
                                                    FROM          dbo.Application_Application AS appchild INNER JOIN
                                                                           dbo.Application_NodeTime AS an ON appchild.ApplicationId = an.ApplicationId
                                                    WHERE      (appchild.AppCounter IN
                                                                               (SELECT     AppCounter
                                                                                 FROM          dbo.Application_Application AS achild
                                                                                 WHERE      (ApplicationId = aa.ApplicationId)))) AS minCommingTime,
                                                  (SELECT     MAX(ISNULL(an.ExitTime, GETDATE())) AS Expr1
                                                    FROM          dbo.Application_Application AS appchild INNER JOIN
                                                                           dbo.Application_NodeTime AS an ON appchild.ApplicationId = an.ApplicationId
                                                    WHERE      (appchild.AppCounter IN
                                                                               (SELECT     AppCounter
                                                                                 FROM          dbo.Application_Application AS achild
                                                                                 WHERE      (ApplicationId = aa.ApplicationId)))) AS exitTime, CASE WHEN ss.executionduration = 0 THEN 1200 ELSE
                                                  (SELECT     ss.executionduration
                                                    FROM          application_application aalim, strategy_strategy ss
                                                    WHERE      aalim.applicationid = aa.applicationid AND ss.strategyid = aalim.strategyid) END AS ValueOfLimit, 
                                              CASE WHEN ss.executionduration > (DATEDIFF(s,
                                                  (SELECT     MIN(an.comingtime)
                                                    FROM          application_application appchild, application_nodetime an
                                                    WHERE      appchild.appcounter IN
                                                                               (SELECT     achild.appcounter
                                                                                 FROM          application_application achild
                                                                                 WHERE      achild.applicationid = aa.applicationid) AND an.applicationid = appchild.applicationid),
                                                  (SELECT     MAX(ISNULL(an.exittime, GETDATE()))
                                               FROM          application_application appchild, application_nodetime an
                                                    WHERE      appchild.appcounter IN
                                                                               (SELECT     achild.appcounter
                                                                                 FROM          application_application achild
                                                                                 WHERE      achild.applicationid = aa.applicationid) AND an.applicationid = appchild.applicationid))) OR
                                              ss.executionduration IS NULL THEN 0 ELSE round(DATEDIFF(s,
                                                  (SELECT     MIN(an.comingtime)
                                                    FROM          application_application appchild, application_nodetime an
                                                    WHERE      appchild.appcounter IN
                                                                               (SELECT     achild.appcounter
                                                                                 FROM          application_application achild
                                                                                 WHERE      achild.applicationid = aa.applicationid) AND an.applicationid = appchild.applicationid),
                                                  (SELECT     MAX(ISNULL(an.exittime, GETDATE()))
                                                    FROM          application_application appchild, application_nodetime an
                                                    WHERE      appchild.appcounter IN
                                                                               (SELECT     achild.appcounter
                                                                                 FROM          application_application achild
                                                                                 WHERE      achild.applicationid = aa.applicationid) AND an.applicationid = appchild.applicationid)), 0) END AS commingExitTime, 
                                              CASE WHEN ss.executionduration > (DATEDIFF(s,
                                                  (SELECT     MIN(an.comingtime)
                                                    FROM          application_application appchild, application_nodetime an
                                                    WHERE      appchild.appcounter IN
                                                                               (SELECT     achild.appcounter
                                                                                 FROM          application_application achild
                                                                                 WHERE      achild.applicationid = aa.applicationid) AND an.applicationid = appchild.applicationid),
                                                  (SELECT     MAX(ISNULL(an.exittime, GETDATE()))
                                                    FROM          application_application appchild, application_nodetime an
                                                    WHERE      appchild.appcounter IN
                                                                               (SELECT     achild.appcounter
                                                                                 FROM          application_application achild
                                                                                 WHERE      achild.applicationid = aa.applicationid) AND an.applicationid = appchild.applicationid))) OR
                                              ss.executionduration IS NULL THEN 0 WHEN ss.executionduration = 0 THEN round(DATEDIFF(s,
                                                  (SELECT     MIN(an.comingtime)
                  FROM          application_application appchild, application_nodetime an
                                                    WHERE      appchild.appcounter IN
                                                                               (SELECT     achild.appcounter
                                                                                 FROM          application_application achild
                                                                                 WHERE      achild.applicationid = aa.applicationid) AND an.applicationid = appchild.applicationid),
                                                  (SELECT     MAX(ISNULL(an.exittime, GETDATE()))
                                                    FROM          application_application appchild, application_nodetime an
                                                    WHERE      appchild.appcounter IN
                                                                               (SELECT     achild.appcounter
                                                                                 FROM          application_application achild
                                                                                 WHERE      achild.applicationid = aa.applicationid) AND an.applicationid = appchild.applicationid)), 0) - 1200 ELSE round(DATEDIFF(s,
                                                  (SELECT     MIN(an.comingtime)
                                                    FROM          application_application appchild, application_nodetime an
                                                    WHERE      appchild.appcounter IN
                                                                               (SELECT     achild.appcounter
                                                                                 FROM          application_application achild
                                                                                 WHERE      achild.applicationid = aa.applicationid) AND an.applicationid = appchild.applicationid),
                                                  (SELECT     MAX(ISNULL(an.exittime, GETDATE()))
                                                    FROM          application_application appchild, application_nodetime an
                                                    WHERE      appchild.appcounter IN
                                                                               (SELECT     achild.appcounter
                                                                                 FROM          application_application achild
                                                                                 WHERE      achild.applicationid = aa.applicationid) AND an.applicationid = appchild.applicationid)), 0) 
                                              - ss.executionduration END AS exceededTime
                       FROM          dbo.Application_Application AS aa INNER JOIN
                                              dbo.Strategy_Strategy AS ss ON aa.StrategyId = ss.StrategyId
                       WHERE      (aa.ParentAppID IS NULL)) AS t
WHERE     (exceededTime > 0)

GO

