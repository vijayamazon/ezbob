CREATE OR REPLACE FUNCTION CreditProduct_GetLinkedStr
(
  pCreditProductId IN number
) return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
    select creditproduct_products.id,
           creditproduct_products.name as "ProductName",
           strategy_strategy.displayname as "Name",
           strategy_strategy.termdate
      from strategy_strategy,
           creditproduct_strategyrel,
           creditproduct_products
     where creditproductid = pCreditProductId
           and strategy_strategy.strategyid = creditproduct_strategyrel.strategyid
           and creditproduct_products.id = creditproduct_strategyrel.creditproductid
		   and strategy_strategy.isdeleted = 0;

  return l_Cursor;

END;
/

