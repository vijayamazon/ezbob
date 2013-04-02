create or replace function StrategyTasks_GetAreaTasks
(pAreaId NUMBER)
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
        select * 
	from StrategyTasks
	where AreaID = pAreaId;

  return l_Cursor;

END;

/