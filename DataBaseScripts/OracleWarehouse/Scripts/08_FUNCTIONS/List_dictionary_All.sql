CREATE OR REPLACE FUNCTION 
List_dictionary_All return sys_refcursor
as
  lcur sys_refcursor;
begin
  open lcur for
      select  id, displayname, tablename, description from dictionaries;
  return lcur;
end List_dictionary_All;
/
