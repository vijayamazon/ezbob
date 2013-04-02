CREATE OR REPLACE FUNCTION List_dictionary_Fields(pdictionaryId number)
  return sys_refcursor
  as
  lcur sys_refcursor;
begin
  open lcur for
    select id,
           fieldname,
           displayname,
           fieldtype,
	   masterdictionaryid,
           (select displayname
              from dictionaries
             where id = a.masterdictionaryid) as ReferencedDictionaryName,
             defvalue
      from dictionaryparams a
     where dictionaryid = pdictionaryId;
  return lcur;
end List_dictionary_Fields;
/
