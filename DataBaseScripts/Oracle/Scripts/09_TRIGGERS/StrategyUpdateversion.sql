create or replace trigger StrategyUpdateversion
after insert or update on Strategy_Strategy
begin
  if not PKG_STRATEGYUPDATE.latch then
    PKG_STRATEGYUPDATE.update_strategy_strategy;
  end if;
end;
/