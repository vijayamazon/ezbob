CREATE OR REPLACE PROCEDURE Delete_DS_Strategy_Rel
  (
    pStrategyId IN number
   )
AS
BEGIN

  if pStrategyId > 0 then
       delete from DATASOURCE_STRATEGYREL where strategyid = pStrategyId;
  end if;

END;
/

