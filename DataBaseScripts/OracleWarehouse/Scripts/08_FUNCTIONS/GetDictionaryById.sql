CREATE OR REPLACE FUNCTION 
GetDictionaryById
(pDictionaryId number)
 return sys_refcursor
as
  lcur sys_refcursor;
begin
  open lcur for
	select	id, displayname, 
		tablename, description 
	from dictionaries
	where id = pDictionaryId;
  return lcur;
end GetDictionaryById;
/
