CREATE OR REPLACE VIEW SUSPENDEDAPPLICATIONS AS
select
      aa.applicationid
     ,aa.appcounter
     ,aa.creationdate
     ,aa.state
     ,(select ss.displayname from strategy_strategy ss where ss.strategyid = aa.strategyid) as strategyname
     ,(select cpp.name from creditproduct_products cpp, creditproduct_strategyrel cps where cpp.id = cps.creditproductid and cps.strategyid = aa.strategyid) as creditproductname
     ,Version
     ,(select su.userid from security_user su where su.userid = aa.creatoruserid) as userId
     ,(select su.username from security_user su where su.userid = aa.creatoruserid) as userName
     ,(select su.fullname from security_user su where su.userid = aa.creatoruserid) as userFullName
     ,susp."Date" as  actiondate
     ,(SELECT ah.CurrentNodeID FROM Application_History ah,Strategy_Node sn
              WHERE (ah.ApplicationId = aa.applicationid) and ah.AppHistoryId = (SELECT MAX(AppHistoryId)FROM Application_History WHERE ApplicationId = ah.ApplicationId) and sn.nodeId = ah.CurrentNodeID) as nodeId
     ,nvl((SELECT sn.Name FROM Application_History ah,Strategy_Node sn
              WHERE (ah.ApplicationId = aa.applicationid) and ah.AppHistoryId = (SELECT MAX(AppHistoryId)FROM Application_History WHERE ApplicationId = ah.ApplicationId) and sn.nodeId = ah.CurrentNodeID), 'SE') as nodeName
     ,nvl((SELECT sn.displayname FROM Application_History ah,Strategy_Node sn
              WHERE (ah.ApplicationId = aa.applicationid) and ah.AppHistoryId = (SELECT MAX(AppHistoryId)FROM Application_History WHERE ApplicationId = ah.ApplicationId) and sn.nodeId = ah.CurrentNodeID), 'SE') as nodeDisplayName
     ,(SELECT ae.ErrorMessage FROM Application_Error ae WHERE ae.applicationid = aa.applicationid) as errorMessage,
     NULL AS GCRecord,
     aa.applicationid AS OID,
     0 AS OptimisticLockField,
     0 AS Checked
    from
        application_application aa left join Application_Suspended susp 
     on aa.applicationid = susp.applicationid
    where
        aa.state = 3 OR aa.state = 4 OR aa.state = 5 OR aa.state = 6  OR aa.state = 7
/