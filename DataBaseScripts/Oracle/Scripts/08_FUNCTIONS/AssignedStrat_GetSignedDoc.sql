CREATE OR REPLACE FUNCTION AssignedStrat_GetSignedDoc
(
  pPUBLICID   in Number,
  pStrategyId in Number
)
return CLOB
AS
  l_SignedDoc CLOB;
BEGIN

  SELECT SignedDocument
  INTO l_SignedDoc
  FROM Strategy_PublicSign 
  WHERE StrategyPublicId = pPUBLICID
    AND StrategyId = pStrategyId
    AND rownum = 1
  ORDER BY Id DESC;

  return l_SignedDoc;

END;
/

