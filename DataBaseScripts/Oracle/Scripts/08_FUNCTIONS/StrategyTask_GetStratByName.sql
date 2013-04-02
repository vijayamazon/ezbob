CREATE OR REPLACE FUNCTION StrategyTask_GetStratByName
(
pStrategyName in varchar2,
pStrategyType in varchar2
)
RETURN
  sys_refcursor
AS
  l_Cursor sys_refcursor;
begin

   OPEN l_Cursor FOR
	select	ts.ID, ts.TaskId, ts.Label, ts.StrategyId, st.DisplayName as Name, st.CurrentVersionId as Version
	from	TaskedStrategies ts, Strategy_Strategy st
	where
		ts.StrategyId = st.StrategyId
		and st.DisplayName = pStrategyName
		and st.StrategyType = pStrategyType
		and st.isdeleted = 0;

  return l_Cursor;

end;
/