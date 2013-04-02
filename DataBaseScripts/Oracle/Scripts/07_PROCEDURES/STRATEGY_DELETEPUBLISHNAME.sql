CREATE OR REPLACE PROCEDURE Strategy_DeletePublishName
(
    pId             in number,
    pSignedDocument in clob,
    pUserId         in int
)
AS
BEGIN

  if pId > 0 then
      update strategy_strategy
        set state = 0
      where strategyid in (select strategyid from strategy_publicrel
      where publicid = pId);
     
     update strategy_publicname 
        set
            IsDeleted = pId,
            TerminationDate  = sysdate,
            SignedDocumentDelete = pSignedDocument,
            DeleterUserId = pUserId
     where publicnameid = pId;
  end if;

END;
/

