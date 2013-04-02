CREATE OR REPLACE Procedure Strategy_SchemaSelect
(
	pStrategyId IN NUMBER,
	cur_OUT in out sys_refcursor
)
AS
BEGIN

  OPEN cur_OUT FOR
    SELECT * FROM Strategy_Schemas
	WHERE StrategyId = pStrategyId;
end;
/

