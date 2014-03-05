IF OBJECT_ID (N'dbo.SuspendedApplications') IS NOT NULL
	DROP VIEW dbo.SuspendedApplications
GO

CREATE VIEW [dbo].[SuspendedApplications]
AS
select
      aa.applicationid
     ,aa.appcounter
     ,aa.creationdate
     ,aa.state
     ,ss.displayname as strategyname
     ,cp.name as creditproductname
     ,Version
     ,su.userid as userId
     ,su.username as userName
     ,su.fullname as userFullName
	 ,susp.date as actiondate
	 ,(SELECT ah.CurrentNodeID FROM Application_History ah,Strategy_Node sn
		WHERE (ah.ApplicationId = aa.applicationid) and ah.AppHistoryId = 
		(SELECT MAX(AppHistoryId)FROM Application_History WHERE ApplicationId = ah.ApplicationId) and sn.nodeId = ah.CurrentNodeID) as nodeId
	 ,ISNULL((SELECT sn.Name FROM Application_History ah,Strategy_Node sn
		WHERE (ah.ApplicationId = aa.applicationid) and ah.AppHistoryId = 
		(SELECT MAX(AppHistoryId)FROM Application_History WHERE ApplicationId = ah.ApplicationId) and sn.nodeId = ah.CurrentNodeID), 'SE') as nodeName
	 ,ISNULL((SELECT sn.displayname FROM Application_History ah,Strategy_Node sn
		WHERE (ah.ApplicationId = aa.applicationid) and ah.AppHistoryId = 
		(SELECT MAX(AppHistoryId)FROM Application_History WHERE ApplicationId = ah.ApplicationId) and sn.nodeId = ah.CurrentNodeID), 'SE') as nodeDisplayName
     ,(SELECT ae.ErrorMessage FROM Application_Error ae WHERE ae.applicationid = aa.applicationid) as errorMessage,
NULL AS GCRecord, aa.ApplicationId AS OID, 0 AS OptimisticLockField, 0 AS Checked
    from
     application_application aa 
		left join strategy_strategy ss on ss.strategyid = aa.strategyid
		left join
          (select 
			(
				select top 200 cpp.name + '' + ';' 
					from creditproduct_products cpp, creditproduct_strategyrel cps
					where cpp.id = cps.creditproductid and cps1.strategyid = cps.strategyid 
					group by cpp.name for xml path('')) as name
				, cps1.strategyid 
				from creditproduct_products cpp, creditproduct_strategyrel cps1 
				where cpp.id = cps1.creditproductid 
				group by cps1.strategyid
			) cp on cp.strategyid = aa.strategyid
        left join security_user su on su.userid = aa.creatoruserid
	 left join Application_Suspended susp on aa.applicationid = susp.applicationid
	 
    where
     aa.state = 3 OR aa.state = 4 OR aa.state = 5 OR aa.state = 6  OR aa.state = 7

GO

