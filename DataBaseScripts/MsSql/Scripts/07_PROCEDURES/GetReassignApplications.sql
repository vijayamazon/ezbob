IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetReassignApplications]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetReassignApplications]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetReassignApplications]
(
  @iUserId int
)
AS
BEGIN
    select a.ApplicationId,
           a.appcounter,
           a.creationdate,
           (select sn.displayname from strategy_node sn where sn.nodeid = idn.currentnodeid) CurrentNodeName,
           (select sn.nodeid from strategy_node sn where sn.nodeid = idn.currentnodeid) CurrentNodeId,
           (select ss.displayname from strategy_strategy ss where ss.strategyid = a.strategyid) as strategyname,
           (select TOP(1) cpp.name from creditproduct_products cpp, creditproduct_strategyrel cps where cpp.id = cps.creditproductid and cps.strategyid = a.strategyid) as creditproduct_name,
           Version,
           ISNULL(a.childcount, 0) childcount
    from application_Application a,
         StrategyEngine_ExecutionState s,
         Strategy_Strategy t,
         (SELECT max(id) as id, applicationId
              FROM StrategyEngine_ExecutionState
          GROUP BY applicationId) ids
        ,(SELECT max(id) as id, applicationId, currentnodeid
              FROM StrategyEngine_ExecutionState
          GROUP BY applicationId, currentnodeid) idn
     where ids.id = s.id
           and idn.id = s.id
           and ids.applicationId = a.applicationId
           and a.creatoruserid = @iUserId
           and (a.LockedByUserId is null or a.LockedByUserId = @iUserId)
           and t.StrategyId = a.StrategyId
           and a.parentappid is null
    order by a.appcounter;
END;
GO
