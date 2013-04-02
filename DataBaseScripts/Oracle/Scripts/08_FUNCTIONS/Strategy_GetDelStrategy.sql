CREATE OR REPLACE FUNCTION Strategy_GetDelStrategy
(
  pStrategyName in varchar2
) return sys_refcursor
AS
  retCursor sys_refcursor;
BEGIN
  OPEN retCursor FOR 
    select strategyid from strategy_strategy 
    where strategy_strategy.name = pStrategyName;   
  return retCursor;
END;
/