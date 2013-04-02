CREATE OR REPLACE PROCEDURE CreditProduct_DeleteParam
  (
    pId IN number
   )
AS
BEGIN

  if pId is not null then
     delete creditproduct_params where id = pId;
  end if;

END;
/

