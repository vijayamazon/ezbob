CREATE OR REPLACE PROCEDURE CreditProduct_UpdateProduct
  (
    pId IN OUT number,
    pName IN varchar2,
    pDescription IN varchar2,
    pDateCreation IN date,
    pUserId IN number,
    pSignedDocument in clob,
    pData in clob
   )
AS
  productId Number;
  productSignId Number;
  dublicateId Number;
BEGIN
 
  SELECT count(Id) into dublicateId FROM creditproduct_products WHERE lower(name)=lower(pName) AND (id <> pId OR pId is null) AND IsDeleted is null;
  
  if dublicateId > 0 then
			raise_application_error(-20000, 'IX_CREDITPRODUCT_NAME'); 
			return;
  end if;

  if pId is not null then
     update creditproduct_products
        set name = pName,
            description = pDescription,
            creationdate = pDateCreation,
            userid = pUserId
      where id = pId;
  else
    Select seq_creditproduct_product.nextval into productId from dual;
    pId := productId;
    insert into creditproduct_products
      (id, name, description, creationdate, userid)
    values
      (productId, pName, pDescription, sysdate, pUserId);
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

