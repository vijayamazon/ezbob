CREATE OR REPLACE PROCEDURE Strategy_PublicSignInsert
  (
    pStrategyPublicNameId IN number,
    pAction IN VARCHAR2,
    pUserId IN number,
    pSignedDocument in clob,
    pData in clob,
    pStrategyId in number,
	pAllData in clob
   )
AS
  strategyPublishSignId Number;
BEGIN
  if psignedDocument is not null then
    Select SEQ_StrategyPublish_SIGN.nextval into strategyPublishSignId from dual;
      INSERT INTO Strategy_PublicSign
        (Id
        ,StrategyPublicId
        ,CreationDate
        ,Action
        ,Data
        ,SignedDocument
        ,UserId
        ,StrategyId
		,AllData)
      VALUES
        (strategyPublishSignId
        ,pStrategyPublicNameId
        ,sysdate
        ,pAction
        ,pData
        ,pSignedDocument
        ,pUserId
        ,pStrategyId
		,pAllData);
  end if;

END;
/

