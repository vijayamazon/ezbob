CREATE OR REPLACE FUNCTION CreditProduct_GetProductList
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
    select
       id
      ,name
      ,description
      ,creationdate
      ,userid
      ,(CredProdList_GetSignedDoc(Id)) as "SignedDocument"
    from creditproduct_products
    where isdeleted is null;
  return l_Cursor;

END;
/

