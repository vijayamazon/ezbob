CREATE OR REPLACE PROCEDURE CreditProduct_DeleteProduct
  (
    pId IN number,
    pUserId int,
    psignedDocument clob,
    pdata clob
   )
AS
 productSignId Number;
BEGIN
  if pId > 0 then
     update creditproduct_products
        set isdeleted = pId
      where id = pId;
  end if;

    if psignedDocument is not null then
    Select SEQ_CREDITPRODUCT_SIGN.nextval into productSignId from dual;
      INSERT INTO CreditProduct_Sign
        (Id
        ,CreditProductId
        ,CreationDate
        ,Data
        ,SignedDocument
        ,UserId)
      VALUES
        (productSignId
        ,pId
        ,sysdate
        ,pdata
        ,psignedDocument
        ,puserId);
  end if;

END;
/

