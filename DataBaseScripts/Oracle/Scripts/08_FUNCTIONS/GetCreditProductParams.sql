create or replace function GetCreditProductParams( iCreditProductName in varchar2)
return sys_refcursor
as
 l_cur sys_refcursor;
begin
 open l_cur for
    select
      cpp.id
      ,cpp.name
      ,cpp.description
      ,cpp.type
      ,cpp.value
	  ,cpp.creditproductid
    from
      creditproduct_products cp,
      creditproduct_params cpp
    where
      cp.id = cpp.creditproductid
      and cp.name = iCreditProductName
      and cp.IsDeleted is null;
  return l_cur;
end GetCreditProductParams;
/
