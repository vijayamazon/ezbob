CREATE OR REPLACE PROCEDURE Strategy_StrategyPublish
  (
    pStrategyId in Number,
    pPublishNameId in Number,
    pPercent in Number,
    pAction IN VARCHAR2,
    pUserId IN number,
    pSignedDocument in clob,
    pData in clob,
	pAllData in clob
  )
AS
lPrevCount number;
BEGIN
  select count(publicid) into lPrevCount from strategy_publicrel
  where strategyid = pStrategyId and publicid = pPublishNameId;
  if lPrevCount = 0 then
  insert into strategy_publicrel
    (publicid, strategyid, percent)
  values
    (pPublishNameId, pStrategyId, pPercent);

    strategy_updatechampionstatus(ppublicid => pPublishNameId);
  end if;
  if NOT (pData IS NULL) then
    Strategy_PublicSignInsert (pPublishNameId, pAction, pUserId, pSignedDocument, pData, pStrategyId, pAllData);
  end if;

END;
/

