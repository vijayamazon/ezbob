create or replace function Strategy_NodeStrategyRelCheck
(
       pNodeId in numeric
)return sys_refcursor
as
  l_cur sys_refcursor;
begin
  open l_cur for
    SELECT DISTINCT strat.displayname, strat.termdate
       FROM  Strategy_Strategy strat
             INNER JOIN Strategy_NodeStrategyRel strRel
             ON strat.StrategyId = strRel.StrategyId
       WHERE strRel.NodeId = pNodeId
             AND strat.IsDeleted = 0;

  return l_cur;
end;
/
