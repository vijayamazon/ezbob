create or replace trigger StrategyUpdateTriggerA
before insert or update or delete on Strategy_Strategy
begin
-- Sd`kemhe bqeu g`ohqei hg pl/sql-r`akhv{ oeped m`w`knl namnbkemh.
  if not PKG_STRATEGYUPDATE.latch then
    PKG_STRATEGYUPDATE.t_logIds.delete();
  end if;

end;
/