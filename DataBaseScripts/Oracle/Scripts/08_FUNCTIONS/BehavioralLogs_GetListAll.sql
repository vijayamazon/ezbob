CREATE OR REPLACE FUNCTION BehavioralLogs_GetListAll
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN
  OPEN l_Cursor FOR
       select 	logs.id, logs.strategyid, logs.name, s.Name as strategyname,
		logs.creationdate, logs.testrun, logs.outcome, logs.isnotread
       from BehavioralLogs logs, Strategy_Strategy s
	where 
		logs.strategyid = s.strategyid
		and s.strategytype = 1;

  return l_Cursor;

END;
/

