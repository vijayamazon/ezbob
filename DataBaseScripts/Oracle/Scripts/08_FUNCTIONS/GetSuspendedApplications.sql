CREATE OR REPLACE FUNCTION GetSuspendedApplications RETURN SYS_REFCURSOR
AS
 l_cur sys_refcursor;
BEGIN
 OPEN l_cur FOR
     select
      aa.applicationid
     ,aa.appcounter
     ,aa.creationdate
     ,aa.state
     ,(select ss.displayname from strategy_strategy ss where ss.strategyid = aa.strategyid) as strategyname
     ,(select cpp.name from creditproduct_products cpp, creditproduct_strategyrel cps where cpp.id = cps.creditproductid and cps.strategyid = aa.strategyid) as creditproduct_name
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
     ,(SELECT ae.ErrorMessage FROM Application_Error ae WHERE ae.applicationid = aa.applicationid) as errorMessage
    from
        application_application aa
       ,Application_Suspended susp
    where
        (aa.state = 4 OR aa.state = 5 OR aa.state = 6  OR aa.state = 7)
        and aa.applicationid = susp.applicationid;
  return l_cur;
END GetSuspendedApplications;
/
