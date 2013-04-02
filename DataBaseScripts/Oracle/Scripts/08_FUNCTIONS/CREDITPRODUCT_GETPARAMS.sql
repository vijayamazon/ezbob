CREATE OR REPLACE FUNCTION CreditProduct_GetParams
(
  pCreditProductId IN number

) return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
    select id, name, type, description, creditproductid, value from creditproduct_params
    where creditproductid = pCreditProductId;

  return l_Cursor;

END;
/

