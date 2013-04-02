CREATE OR REPLACE PROCEDURE CreditProduct_UpdateParams
  (
    pId IN OUT number,
    pName IN varchar2,
    pType IN varchar2,
    pDescription IN varchar2,
    pCreditProductId IN number,
    pValue IN varchar2
   )
AS
  paramId Number;
BEGIN

  if pId is not null then
    update creditproduct_params
       set name = pName,
           type = pType,
           description = pDescription,
           creditproductid = pCreditProductId,
           value = pValue
     where id = pId;
  else
    Select seq_creditproduct_param.nextval into paramId from dual;
    pId := paramId;
    insert into creditproduct_params
      (id, name, type, description, creditproductid, value)
    values
      (paramId, pName, pType, pDescription, pCreditProductId, pValue);
  end if;

END;
/

