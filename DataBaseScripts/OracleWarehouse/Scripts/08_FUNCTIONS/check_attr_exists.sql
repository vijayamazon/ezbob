CREATE OR REPLACE FUNCTION check_attr_exists(pDictionaryId number,
                                             pDisplayName  varchar2)
  return number

 as

  l_flag number;

begin

  l_flag := 0;

  begin
    select 1
      into l_flag
      from dictionaryparams
     where upper(displayname) = upper(pDisplayName)
       and dictionaryid = pdictionaryid;
  exception
    when no_data_found then
      null; --l_flag := 0;
  end;

  return l_flag;

end check_attr_exists;
/
