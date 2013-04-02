create or replace function GetExceededStrategy
return sys_refcursor
as
 l_cur sys_refcursor;
begin
 open l_cur for
select
*
from
(
      select
       aa.applicationid
      ,aa.appcounter
      ,(select ss.displayname from strategy_strategy ss where ss.strategyid = aa.strategyid) as strategy_name
      ,(select cpp.name from creditproduct_products cpp, creditproduct_strategyrel cps where cpp.id = cps.creditproductid and cps.strategyid = aa.strategyid and ROWNUM=1) as creditproduct_name
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
        max(nvl(an.exittime, sysdate))
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
       (((
        select
        max(nvl(an.exittime, sysdate))
        from
        application_application appchild
        ,application_nodetime an
        where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid)
        and an.applicationid = appchild.applicationid
       ) -
       (
        select min(an.comingtime)
        from
        application_application appchild
        ,application_nodetime an
        where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid)
        and an.applicationid = appchild.applicationid
       )) * 24 * 3600)
         or ss.executionduration is null
       then 0
       else
           round(
                 (((
                  select
                  max(nvl(an.exittime, sysdate))
                  from
                  application_application appchild
                  ,application_nodetime an
                  where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid)
                  and an.applicationid = appchild.applicationid
                 ) -
                 (
                  select min(an.comingtime)
                  from
                  application_application appchild
                  ,application_nodetime an
                  where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid)
                  and an.applicationid = appchild.applicationid
                 )) * 24 * 3600)
           )
      end as commingExitTime


      ,case
       when ss.executionduration >
                 (((
                  select
                  max(nvl(an.exittime, sysdate))
                  from
                  application_application appchild
                  ,application_nodetime an
                  where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid)
                  and an.applicationid = appchild.applicationid
                 ) -
                 (
                  select min(an.comingtime)
                  from
                  application_application appchild
                  ,application_nodetime an
                  where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid)
                  and an.applicationid = appchild.applicationid
                 )) * 24 * 3600)
         or ss.executionduration is null
       then 0
       when ss.executionduration = 0
       then
           round(
                 (((
                  select
                  max(nvl(an.exittime, sysdate))
                  from
                  application_application appchild
                  ,application_nodetime an
                  where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid)
                  and an.applicationid = appchild.applicationid
                 ) -
                 (
                  select min(an.comingtime)
                  from
                  application_application appchild
                  ,application_nodetime an
                  where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid)
                  and an.applicationid = appchild.applicationid
                 )) * 24 * 3600)
           ) - 1200
       else
           round(
                 (((
                  select
                  max(nvl(an.exittime, sysdate))
                  from
                  application_application appchild
                  ,application_nodetime an
                  where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid)
                  and an.applicationid = appchild.applicationid
                 ) -
                 (
                  select min(an.comingtime)
                  from
                  application_application appchild
                  ,application_nodetime an
                  where appchild.appcounter in (select achild.appcounter from application_application achild where achild.applicationid = aa.applicationid)
                  and an.applicationid = appchild.applicationid
                 )) * 24 * 3600)
           ) - ss.executionduration
      end as exceededTime

      from
       application_application aa
       ,strategy_strategy ss
      where
       ss.strategyid = aa.strategyid
       and aa.parentappid is null
      order by
       aa.appcounter
) t
where t.exceededTime > 0;
return l_cur;
end GetExceededStrategy;
/
