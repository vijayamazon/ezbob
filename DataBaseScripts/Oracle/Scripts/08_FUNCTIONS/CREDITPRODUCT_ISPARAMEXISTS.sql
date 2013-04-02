CREATE OR REPLACE FUNCTION CreditProduct_IsParamExists
  (
    paramName IN varchar2,
    creditProductName IN varchar2,
    paramType IN varchar2,
    defaultValue IN varchar2,
    userId IN number
  )
  return number
AS
l_id  number;
crProductId number;
used number;
prevType varchar2(1024);
BEGIN
   l_id := null;

  begin
    select p.ID, count(rel.STRATEGYID) into crProductId, used
    from creditproduct_products p
    left join CREDITPRODUCT_STRATEGYREL rel
      on rel.CREDITPRODUCTID=p.ID
    where upper(p.name) = upper(creditProductName)
      and p.isdeleted is null
    GROUP BY p.ID;
  exception
    when no_data_found then
     return 3;
  end;


  begin
    select creditproduct_params.id, creditproduct_params.type into l_id, prevType
      from creditproduct_params
     where creditproduct_params.creditproductid = crProductId
	 and creditproduct_params.type = paramType
     and upper(creditproduct_params.name) = upper(paramName);

    IF paramType <> prevType AND used>0 then
        return 3;
    end if;
    return 0;
  exception
    when no_data_found then

    return 3;

  end;

END;
/
