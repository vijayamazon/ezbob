create or replace function CreditProduct_CoreGetParams
(pCreditProductName IN varchar2) return  sys_refcursor
as
  curs sys_refcursor;
begin
  OPEN curs FOR
    select a.name, a.value           
      from creditproduct_params a,
           Creditproduct_Products b
     where a.creditproductid = b.id and
           b.Name = pCreditProductName;

  return(curs);
end CreditProduct_CoreGetParams;
/
