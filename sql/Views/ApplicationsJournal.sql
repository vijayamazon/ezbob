IF OBJECT_ID (N'dbo.ApplicationsJournal') IS NOT NULL
	DROP VIEW dbo.ApplicationsJournal
GO

CREATE VIEW [dbo].[ApplicationsJournal]
AS
SELECT     dbo.Application_Application.CreationDate, dbo.Application_Application.ApplicationId AS SystemApplicationIdentifier, 
                      dbo.Application_Application.AppCounter AS ApplicationIdentifier, dbo.Strategy_Strategy.DisplayName AS StrategyName, NULL AS GCRecord, 
                      dbo.Application_Application.ApplicationId AS OID, 0 AS OptimisticLockField, dbo.Application_Application.State, dbo.Application_Application.AppCounter, 
                      CASE WHEN Application_Application.State = 6 THEN 0 ELSE 1 END AS IsSignValidString
FROM         dbo.Application_Application INNER JOIN
                      dbo.Strategy_Strategy ON dbo.Application_Application.StrategyId = dbo.Strategy_Strategy.StrategyId
WHERE     EXISTS
                          (SELECT     id, applicationId, nodeId, outletName, dateAdded, signedData, data, nodeName, userName
                            FROM          dbo.Application_NodeDataSign
                            WHERE      (applicationId = dbo.Application_Application.ApplicationId))

GO

