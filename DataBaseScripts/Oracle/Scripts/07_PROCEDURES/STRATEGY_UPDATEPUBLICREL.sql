CREATE OR REPLACE PROCEDURE Strategy_UpdatePublicRel
  (
    pPublicId IN number,
    pStrategyId IN number,
    pPercent IN number,
    pAction IN VARCHAR2,
    pUserId IN number,
    pSignedDocument in clob,
    pData in clob,
	pAllData in clob
   )
AS

BEGIN

  update strategy_publicrel
     set percent = pPercent
     where publicid = pPublicId and strategyid = pStrategyId;
  
  strategy_updatechampionstatus(ppublicid => pPublicId);
  if NOT (pData IS NULL) then
    Strategy_PublicSignInsert (pPublicId, pAction, pUserId, pSignedDocument, pData, pStrategyId, pAllData);
  end if;   
END;
/

