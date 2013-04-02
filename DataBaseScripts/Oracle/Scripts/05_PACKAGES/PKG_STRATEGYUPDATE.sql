create or replace package PKG_STRATEGYUPDATE as
  type tbl_strategy_logid is table of integer index by binary_integer;
  t_logIds tbl_strategy_logid;
  latch boolean := false;
  procedure update_strategy_strategy;
end;
/
