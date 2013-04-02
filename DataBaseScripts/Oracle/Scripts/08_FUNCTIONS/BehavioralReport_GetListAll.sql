CREATE OR REPLACE FUNCTION BehavioralReport_GetListAll
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
       select 	rep.id, rep.strategyid, rep.name, s.DisplayName as strategyname,
		rep.typeid, rep.creationdate, rep.testrun, rep.isnotread
       from BehavioralReports rep, Strategy_Strategy s
	where 
		rep.strategyid = s.strategyid;

  return l_Cursor;

END;
/

