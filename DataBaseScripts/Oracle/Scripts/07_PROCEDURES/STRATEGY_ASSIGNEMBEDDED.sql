CREATE OR REPLACE PROCEDURE Strategy_AssignEmbedded
  (
    pStrategyId IN number,
    pEmbStrategyName IN varchar2
  )
AS
 embStrategyId number;
BEGIN
 select strategyid  into embStrategyId
   from strategy_strategy
  where name = pEmbStrategyName and isdeleted = 0;



    delete from strategy_embededrel
    where strategyid = pStrategyId and embstrategyid=embStrategyId;
    if (pStrategyId != embStrategyId) then
      insert into strategy_embededrel
        (strategyid, embstrategyid)
      values
        (pStrategyId, embStrategyId);
      end if;


END;
/

