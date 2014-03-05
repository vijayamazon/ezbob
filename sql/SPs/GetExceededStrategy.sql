IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExceededStrategy]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExceededStrategy]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExceededStrategy]
AS
BEGIN
	select
*
from
(
      select
       aa.applicationid
      ,aa.appcounter
      ,(select ss.displayname from strategy_strategy ss where ss.strategyid = aa.strategyid) as strategy_name
      ,(select top 1 cpp.name from creditproduct_products cpp, creditproduct_strategyrel cps where cpp.id = cps.creditproductid and cps.strategyid = aa.strategyid) as creditproduct_name
      ,(
        select min(an.comingtime)
        from
        application_application appchild
        ,application_nodetime an
        where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid)
        and an.applicationid = appchild.applicationid
      ) minCommingTime
      ,(
        select
        max(ISNULL(an.exittime, GETDATE()))
        from
        application_application appchild
        ,application_nodetime an
        where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid)
        and an.applicationid = appchild.applicationid
      ) as exitTime
      ,
      case
       when ss.executionduration = 0
       then  1200
       else
             (select ss.executionduration from application_application aalim, strategy_strategy ss where aalim.applicationid = aa.applicationid and ss.strategyid = aalim.strategyid)
      end as value_of_limit

      ,case
       when ss.executionduration >
       (

		DATEDIFF(s, 
				 (select min(an.comingtime) from application_application appchild,application_nodetime an where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid) and an.applicationid = appchild.applicationid),	
				 (select max(ISNULL(an.exittime, GETDATE())) from application_application appchild,application_nodetime an where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid) and an.applicationid = appchild.applicationid)
         )
	   ) 
       or ss.executionduration is null
       then 0
       else
           round(
				  DATEDIFF(s, 
						   (select min(an.comingtime) from application_application appchild,application_nodetime an where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid) and an.applicationid = appchild.applicationid),	
						   (select max(ISNULL(an.exittime, GETDATE())) from application_application appchild,application_nodetime an where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid) and an.applicationid = appchild.applicationid)
				          )
                , 0)
      end as commingExitTime


      ,case
       when ss.executionduration >
                 (
				  DATEDIFF(s, 
						   (select min(an.comingtime) from application_application appchild,application_nodetime an where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid) and an.applicationid = appchild.applicationid),
   						   (select max(ISNULL(an.exittime, GETDATE())) from application_application appchild,application_nodetime an where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid) and an.applicationid = appchild.applicationid)
				   )
				  )
         or ss.executionduration is null
       then 0
       when ss.executionduration = 0
       then
           round(
				   DATEDIFF(s, 
							(select min(an.comingtime) from application_application appchild,application_nodetime an where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid) and an.applicationid = appchild.applicationid),
							(select max(ISNULL(an.exittime, GETDATE())) from application_application appchild,application_nodetime an where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid) and an.applicationid = appchild.applicationid)
			       ), 0
                ) - 1200
       else
           round(
					DATEDIFF(s, 
							(select min(an.comingtime) from application_application appchild,application_nodetime an where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid) and an.applicationid = appchild.applicationid),
							(select max(ISNULL(an.exittime, GETDATE())) from application_application appchild,application_nodetime an where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid) and an.applicationid = appchild.applicationid)
					), 0
           ) - ss.executionduration
      end as exceededTime

      from
       application_application aa
       ,strategy_strategy ss
      where
       ss.strategyid = aa.strategyid
       and aa.parentappid is null
) t
where t.exceededTime > 0
order by
       t.appcounter
END
GO
