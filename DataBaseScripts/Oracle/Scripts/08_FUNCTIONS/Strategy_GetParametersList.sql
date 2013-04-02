CREATE OR REPLACE FUNCTION Strategy_GetParametersList
(pStrategyId NUMBER)
RETURN
  sys_refcursor
AS
  l_cursor sys_refcursor;

BEGIN
  OPEN l_cursor FOR
  select sp.StratParamId, sp.Name, sp.Description, spt.Name as TypeName
  from   Strategy_StrategyParameter sp,
    Strategy_ParameterType spt
  where  sp.TypeId = spt.ParamTypeId
    and sp.OwnerId = pStrategyId;

  return l_cursor;
END;
/