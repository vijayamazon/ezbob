create or replace view exceededstrategy as
select
"APPLICATIONID","APPCOUNTER","STRATEGYNAME","CREDITPRODUCTNAME","MINCOMMINGTIME","EXITTIME","VALUEOFLIMIT","COMMINGEXITTIME","EXCEEDEDTIME",
NULL AS GCRecord,
ApplicationId AS OID,
0 AS OptimisticLockField
from
(
      select
       aa.applicationid
      ,aa.appcounter
      ,(select ss.displayname from strategy_strategy ss where ss.strategyid = aa.strategyid) as strategyname
      ,(select cpp.name from creditproduct_products cpp, creditproduct_strategyrel cps where cpp.id = cps.creditproductid and cps.strategyid = aa.strategyid) as creditproductname
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
      end as valueoflimit

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
where t.exceededTime > 0
/