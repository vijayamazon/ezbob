CREATE OR REPLACE FUNCTION creditproduct_getproductbyname
(
  pName varchar
) return sys_refcursor
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
    where isdeleted is null and name = pName;
  return l_Cursor;

END;
/

