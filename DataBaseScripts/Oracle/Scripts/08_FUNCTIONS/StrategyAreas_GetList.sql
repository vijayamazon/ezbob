create or replace function StrategyAreas_GetList
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
       select 	*
       from StrategyAreas;

  return l_Cursor;

END;

/