CREATE OR REPLACE FUNCTION Strategy_CheckStrategyState
(
   pStrategyId in number
)
return sys_refcursor
as
  resCursor sys_refcursor;
begin
  /* 0 - Application started, 
     2 - Application finished OK,
     3 - Exception occured*/
 open resCursor for
      select (select count(applicationid) from application_application where
       strategyid = pStrategyId and state not in(2,3,0)) as "ActiveAppCount",
       lastupdatedate
       from strategy_strategy
       where strategyid=pStrategyId;

  return resCursor;
end;
/

