IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllApplication]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAllApplication]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllApplication]
AS
BEGIN
	select
      aa.applicationid
     ,aa.appcounter
     ,aa.creationdate
     ,ss.displayname as strategyname
	 ,ss.strategyid
     ,cp.name as creditproduct_name
     ,Version
     ,case
        when aa.state = 0 then 'NeedProcessBySE'
        when aa.state = 1 then 'NeedProcessByNode'
        when aa.state = 2 then 'StrategyHasBeenFinishedWithoutErrors'
        when aa.state = 3 then 'StrategyHasBeenFinishedWithErrors'
        when aa.state = 4 then 'ArchiveApplication'
        when aa.state = 5 then 'ApplicationError'
        when aa.state = 6 then 'ApplicationWithSecurityViolationError'
        when aa.state = 7 then 'ApplicationWithHandledError'
      end state
     ,aa.errormsg
     ,ISNULL(childcount.counter, 0) childcount
     ,(select su.userid from security_user su where su.userid = aa.creatoruserid) as userId
     ,(select su.username from security_user su where su.userid = aa.creatoruserid) as userName
     ,(select su.fullname from security_user su where su.userid = aa.creatoruserid) as userFullName
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
    left join
          (select aac.appcounter, count(aac.applicationid) as counter
                  from application_application aac
           where aac.parentappid is not null group by aac.appcounter) childcount on childcount.appcounter = aa.appcounter
     left join security_user su on su.userid = aa.creatoruserid
    where
     aa.parentappid is null
END
GO
