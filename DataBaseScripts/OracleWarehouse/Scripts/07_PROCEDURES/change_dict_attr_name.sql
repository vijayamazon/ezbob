CREATE OR REPLACE procedure change_dict_attr_name
(pDictionaryId number,
 pOldAttrName  varchar2,
 pNewAttrName  varchar2)

 as

  l_flag number;

begin

  l_flag := 0;

  begin
    select 1
      into l_flag
      from dictionaryparams
     where upper(displayname) = upper(pNewAttrName)
       and dictionaryid = pdictionaryid;
  exception
    when no_data_found then
      null; --l_flag := 0;
  end;

  if l_flag = 0 then
    update dictionaryparams
       set displayname = pNewAttrName
     where dictionaryid = pDictionaryid
	and displayname = pOldAttrName;
    commit;
  end if;

end change_dict_attr_name;
/
