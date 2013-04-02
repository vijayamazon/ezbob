CREATE OR REPLACE PROCEDURE CreditProduct_AssignProduct
  (
    pCreditProdName IN Varchar2,
    pStrategyId IN number
  )
AS
  newId number;
  pCreditProdId number;
BEGIN

  begin
    select id into pCreditProdId
    from creditproduct_products a
    where a.name = pCreditProdName
    and a.isdeleted is null;
  
    select tbl.id into newId from creditproduct_strategyrel tbl
    where tbl.creditproductid = pCreditProdId and tbl.strategyid = pStrategyId;

    exception
      when no_data_found then
        begin
          select seq_creditproduct_strategyrel.nextval into newId from dual;
          insert into creditproduct_strategyrel
            (id, creditproductid, strategyid)
          values
            (newId, pCreditProdId, pStrategyId);
        end;
  end;

END;
/
