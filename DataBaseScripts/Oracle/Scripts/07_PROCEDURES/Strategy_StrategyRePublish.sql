CREATE OR REPLACE PROCEDURE Strategy_StrategyRePublish
  (
    pDisplayName in varchar2,
    pNewStrategyId in number
  )
AS
pOldStratId number;
pOldState number;
BEGIN

  select strategyid, state into pOldStratId, pOldState from strategy_strategy
  where displayname = pDisplayName and isdeleted = 0 
  and termdate = (select max(termdate) from strategy_strategy
  where displayname = pDisplayName and isdeleted = 0);
  
  update  strategy_publicrel
  set strategyid = pNewStrategyId
  where strategyid = pOldStratId;
  
  update strategy_strategy 
  set state = pOldState
  where strategyid = pNewStrategyId;

END;
/

