IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSuspendedApplications]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetSuspendedApplications]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSuspendedApplications]
AS
BEGIN
	select
      aa.applicationid
     ,aa.appcounter
     ,aa.creationdate
     ,aa.state
     ,ss.displayname as strategyname
     ,cp.name as creditproduct_name
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
     ,(SELECT ae.ErrorMessage FROM Application_Error ae WHERE ae.applicationid = aa.applicationid) as errorMessage
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
	 ,Application_Suspended susp
	 
    where
     (aa.state = 4 OR aa.state = 5 OR aa.state = 6  OR aa.state = 7)
     and aa.applicationid = susp.applicationid
END
GO
