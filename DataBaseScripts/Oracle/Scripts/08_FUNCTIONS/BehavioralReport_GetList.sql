CREATE OR REPLACE FUNCTION BehavioralReport_GetList
(
pStartDate IN date,
pEndDate IN date,
pTypeId IN number,
pStrategyName in varchar2,
pTestRun in number
)
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
  l_startTypeID number;
  l_endTypeID number;
  l_startTestRun number;
  l_endTestRun number;
BEGIN

if pTypeId = - 1 then
  l_startTypeID := 0;
  l_endTypeID := 10000;
else
  l_startTypeID := pTypeId;
  l_endTypeID := pTypeId;
end if;

if pTestRun = - 1 then
  l_startTestRun := 0;
  l_endTestRun := 1;
else
  l_startTestRun := pTestRun;
  l_endTestRun := pTestRun;
end if;

  OPEN l_Cursor FOR
       select 	rep.id, rep.strategyid, rep.name, s.DisplayName as strategyname,
		rep.typeid, rep.creationdate, rep.testrun, rep.isnotread
       from BehavioralReports rep, Strategy_Strategy s
	where rep.strategyid = s.strategyid
	and rep.typeid >= l_startTypeID and rep.typeid <= l_endTypeID
	and rep.creationdate >= pStartDate and rep.creationDate <= pEndDate
	and rep.testrun >= l_startTestRun and rep.testrun <= l_endTestRun 
	and s.IsDeleted = 0
	and (s.DisplayName=pStrategyName OR pStrategyName is null)
	;

  return l_Cursor;

END;
/

