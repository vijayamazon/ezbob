CREATE OR REPLACE FUNCTION TaskedStrategy_ListParams
(
pTaskedStrategyId NUMBER
)
RETURN sys_refcursor
as
  l_cursor sys_refcursor;
begin

  OPEN l_cursor FOR
  	select	*
  	from	TaskedStrategyParams p
	where 	p.TSID = pTaskedStrategyId;

return l_cursor;

end;
/