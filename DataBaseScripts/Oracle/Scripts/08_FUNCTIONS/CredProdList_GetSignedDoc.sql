CREATE OR REPLACE FUNCTION CredProdList_GetSignedDoc
(
  pProductId in Number
)
return CLOB
AS
  l_SignedDoc CLOB;
BEGIN

  SELECT SignedDocument
  INTO l_SignedDoc
  FROM CreditProduct_Sign 
  WHERE CreditProductId = pProductId
    AND rownum = 1
  ORDER BY Id DESC;

  return l_SignedDoc;

END;
/

