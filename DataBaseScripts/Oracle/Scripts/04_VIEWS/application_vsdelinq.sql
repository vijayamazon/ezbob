create or replace view application_vsdelinq as
select s.strategyid
     , a.applicationid
     , s.executionduration as executiontimelimit
     , a.creationdate as starttime
     , case
         when a.state = 2 or a.state = 3
         then a.lastupdatedate
         else null
       end as finishtime
     , case
         when a.state = 2 or a.state = 3
         then round((a.lastupdatedate - a.creationdate)*24*3600)
         else round((sysdate - a.creationdate)*24*3600)
       end as executiontime
     , case
         when a.state = 2 or a.state = 3
         then a.istimelimitexceeded
         else case
                when a.creationdate + NUMTODSINTERVAL( NVL( s.executionduration, 9.99E125), 'SECOND') < SYSDATE
                then 1
                else 0
              end
       end as istimelimitexpiried
from application_application a
join strategy_strategy s on a.strategyid = s.strategyid

/