CREATE OR REPLACE FUNCTION BehavioralLogs_GetList
(
pStartDate IN date,
pEndDate IN date,
pStrategyId in number,
pTestRun in number,
pOutcome IN number
)
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
  l_startOutcome number;
  l_endOutcome number;
  l_startTestRun number;
  l_endTestRun number;
  l_startStrategyId number;
  l_endStrategyId number;
BEGIN

if pOutcome = - 1 then
  l_startOutcome := 0;
  l_endOutcome := 2;
else
  l_startOutcome := pOutcome;
  l_endOutcome := pOutcome;
end if;

if pStrategyId = - 1 then
  select MIN(StrategyId), MAX(StrategyId)
	into l_startStrategyId, l_endStrategyId
	from Strategy_Strategy
	where StrategyType = 1;
else
  l_startStrategyId := pStrategyId;
  l_endStrategyId := pStrategyId;
end if;

if pTestRun = - 1 then
  l_startTestRun := 0;
  l_endTestRun := 1;
else
  l_startTestRun := pTestRun;
  l_endTestRun := pTestRun;
end if;

  OPEN l_Cursor FOR
       select 	logs.id, logs.strategyid, logs.name, s.Name as strategyname,
		 logs.creationdate, logs.testrun, logs.outcome, logs.isnotread
       from BehavioralLogs logs, Strategy_Strategy s
	where logs.strategyid = s.strategyid
	and logs.creationdate >= pStartDate and logs.creationDate <= pEndDate
	and logs.testrun >= l_startTestRun and logs.testrun <= l_endTestRun
	and logs.outcome >= l_startOutcome and logs.outcome <= l_endOutcome
	and s.strategytype = 1
	and s.strategyid >= l_startStrategyId and s.strategyid <= l_endStrategyId
	;

  return l_Cursor;

END;
/