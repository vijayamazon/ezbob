CREATE OR REPLACE Procedure Strategy_GetAssignedStrategies
(
  pPublishNameId in Number,
  cur_OUT in out sys_refcursor
)
AS
BEGIN

  OPEN cur_OUT FOR
    SELECT
       t.strategyId
      ,t.name
      ,t.authorName
      ,t.state
      ,p.percent
      ,t.description
      ,t.displayname
      ,t.termdate
      ,(AssignedStrat_GetSignedDoc(p.PUBLICID, p.strategyId)) as "SignedDocument"
    FROM STRATEGY_publicRel p, strategy_vstrategy t
    WHERE p.strategyID = t.strategyId AND p.publicid = pPublishNameId;
END;
/
