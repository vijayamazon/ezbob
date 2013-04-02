CREATE OR REPLACE FUNCTION DataSource_GetLinkedStr
(
  pDataSourceName IN varchar2
) return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
	select	strategy_strategy.strategyid as "ID", 
		strategy_strategy.displayname as "Name"
	from strategy_strategy,
		datasource_strategyrel           
     where datasource_strategyrel.datasourcename = pDataSourceName
           and strategy_strategy.strategyid = datasource_strategyrel.strategyid
           AND (Strategy_Strategy.IsDeleted IS NULL OR Strategy_Strategy.IsDeleted = 0);

  return l_Cursor;

END;
/