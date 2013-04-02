create or replace package body PKG_STRATEGYUPDATE is
procedure update_strategy_strategy
as
 l_logId binary_integer;
 l_strategyid Number;
begin
  latch := true;
  -- Eqkh eqr| namnbkemm{e g`ohqh
  if (t_logIds.count > 0) then
    l_logId := t_logIds.first;
    while l_logId is not null loop
      select ls.strategyid into l_strategyid from log_strategy ls where ls.logstrategyid = l_logId;

      update Strategy_Strategy ss
        set ss.currentversionid = l_logId
       where ss.strategyid = l_strategyid;

       l_logId := t_logIds.next(l_logId);
    end loop;
  end if;
  latch := false;
exception when others then
  latch := false;
  raise;
end;


end PKG_STRATEGYUPDATE;
/
