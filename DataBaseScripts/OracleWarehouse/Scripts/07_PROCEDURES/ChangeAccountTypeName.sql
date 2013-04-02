create or replace procedure ChangeAccountTypeName
(pAccountTypeId number,
 pDisplayName  varchar2,
 pDescription varchar2)

 as

  l_flag number;

begin

  l_flag := 0;

  begin
    select 1
      into l_flag
      from accounttypes
     where upper(displayname) = upper(pDisplayName)
	and NOT(id = pAccountTypeId);
  exception
    when no_data_found then
      null; 
  end;

  if l_flag = 0 then
    update AccountTypes
       set displayname = pDisplayName, description = pDescription
     where id = pAccountTypeId ;
     commit;
  end if;

end ChangeAccountTypeName;
/