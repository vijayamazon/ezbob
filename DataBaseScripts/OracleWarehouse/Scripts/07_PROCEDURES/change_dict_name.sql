create or replace procedure change_dict_name(pDictionaryId number,
                                           pNewDisplayName  varchar2,
                                           pNewDescription varchar2)

 as

  l_flag number;

begin

  l_flag := 0;

  begin
    select 1
      into l_flag
      from dictionaries
     where upper(displayname) = upper(pNewDisplayName);
  exception
    when no_data_found then
      null; --l_flag := 0;
  end;

  if l_flag = 0 then
    update dictionaries
       set displayname = pNewDisplayName, description = pNewDEscription
     where id = pDictionaryid ;
     commit;
  end if;

end change_dict_name;
/