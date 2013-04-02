create or replace function GetExceededApplication
return sys_refcursor
as
 l_cur sys_refcursor;
begin
 open l_cur for
 select
   aa.applicationid
   ,(select ss.displayname from strategy_strategy ss where ss.strategyid = aa.strategyid) as strategy_name
   ,(select cpp.name from creditproduct_products cpp, creditproduct_strategyrel cps where cpp.id = cps.creditproductid and cps.strategyid = aa.strategyid and ROWNUM=1) as creditproduct_name
   ,aa.appcounter
  from
   application_application aa
   ,strategy_node sn
  where
   (sn.executionduration is not null or sn.executionduration != 0)
   and aa.parentappid is null
  group by
    aa.applicationid
   ,aa.strategyid
   ,aa.appcounter;

return l_cur;
end GetExceededApplication;
/