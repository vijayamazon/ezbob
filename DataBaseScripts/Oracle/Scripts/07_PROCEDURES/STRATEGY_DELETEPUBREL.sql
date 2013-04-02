CREATE OR REPLACE PROCEDURE Strategy_DeletePubRel
  (
    pPublicId IN number,
    pStrategyId IN number,
    pAction IN VARCHAR2,
    pUserId IN number,
    pSignedDocument in clob,
    pData in clob,
    pAllData in clob

   )
AS
BEGIN

     delete strategy_publicrel  
     where publicid = pPublicId and strategyid = pStrategyId;
     
     update strategy_strategy
        set state = 0
      where strategyid = pStrategyId;
     
     if not (pSignedDocument is null) then
       Strategy_PublicSignInsert (pPublicId, pAction, pUserId, pSignedDocument, pData, pStrategyId, pAllData);
     end if;
END;
/

