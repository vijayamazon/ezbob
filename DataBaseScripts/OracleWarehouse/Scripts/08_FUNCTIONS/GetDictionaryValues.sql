CREATE OR REPLACE FUNCTION 
GetDictionaryValues
(pDictionaryId number)
  return sys_refcursor
as
  lcur sys_refcursor;
  l_table varchar2(256);
  l_Select varchar2(256);
begin
  select TableName into l_table
  from Dictionaries
  where ID = pDictionaryId;

  l_Select := 'select id, value from ' || l_table;
  open lcur for l_Select;

  return lcur;
end GetDictionaryValues;
/
