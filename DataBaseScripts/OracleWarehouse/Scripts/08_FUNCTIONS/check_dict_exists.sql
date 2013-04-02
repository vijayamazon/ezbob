CREATE OR REPLACE FUNCTION check_dict_exists
(pDisplayName varchar2)
  return number 
  
  as
  
  l_flag number;

begin

  l_flag := 0;

  begin
    select 1
      into l_flag
      from dictionaries
     where upper(displayname) = upper(pDisplayName);
  exception
    when no_data_found then
      null; --l_flag := 0;
  end;

  return l_flag;

end check_dict_exists;
/
