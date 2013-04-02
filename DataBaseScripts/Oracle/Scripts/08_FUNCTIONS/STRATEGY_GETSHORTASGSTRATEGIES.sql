CREATE OR REPLACE FUNCTION Strategy_GetShortAsgStrategies
(
  pPublishNameId in Number
)
return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
    SELECT
       t.strategyId
      ,t.name
      ,t.state
      ,p.percent
      ,t.displayname
      ,t.termdate
      ,(AssignedStrat_GetSignedDoc(p.PUBLICID, p.strategyId)) as "SignedDocument"
    FROM STRATEGY_publicRel p, strategy_vstrategy t
    WHERE p.strategyID = t.strategyId AND p.publicid = pPublishNameId
    ORDER BY p.strategyID;
  return l_Cursor;

END;
/

