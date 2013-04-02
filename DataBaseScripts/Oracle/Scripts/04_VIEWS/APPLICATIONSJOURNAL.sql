create or replace view APPLICATIONSJOURNAL as
SELECT     
    Application_Application.CreationDate
    ,Application_Application.ApplicationId AS SystemApplicationIdentifier
    ,Application_Application.AppCounter as ApplicationIdentifier
    ,Strategy_Strategy.DisplayName AS StrategyName
    ,NULL AS GCRecord
    ,Application_Application.ApplicationId AS OID
    ,0 AS OptimisticLockField
    ,Application_Application.State as state
    ,case
      when Application_Application.State = 6
      then 0
      else 1
     end as IsSignValidString
FROM         Application_Application INNER JOIN
                      Strategy_Strategy ON Application_Application.StrategyId = Strategy_Strategy.StrategyId 
WHERE
        EXISTS(SELECT * FROM Application_NodeDataSign WHERE Application_NodeDataSign.ApplicationId = Application_Application.ApplicationId)
/