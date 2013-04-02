create or replace procedure ChangeCustomerTypeName
(pCustomerTypeId number,
 pDisplayName  varchar2,
 pDescription varchar2)

 as

  l_flag number;

begin

  l_flag := 0;

  begin
    select 1
      into l_flag
      from customertypes
     where upper(displayname) = upper(pDisplayName)
	and NOT(id = pCustomerTypeId);
  exception
    when no_data_found then
      null; 
  end;

  if l_flag = 0 then
    update CustomerTypes
       set displayname = pDisplayName, description = pDescription
     where id = pCustomerTypeId ;
     commit;
  end if;

end ChangeCustomerTypeName;
/