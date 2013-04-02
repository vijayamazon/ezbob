CREATE OR REPLACE PROCEDURE delete_dictionary_field

(pDictionaryId number, pFieldName varchar2)

 AS

  l_tablename varchar2(30);

BEGIN

  select tablename
    into l_tablename
    from dictionaries
   where id = pdictionaryId;

  execute immediate 'alter table ' || l_tablename || ' drop column ' ||
                    pFieldName;

  delete from dictionaryparams
   where dictionaryId = pdictionaryId
     and upper(FieldName) = upper(pFieldName);
commit;
END;
/
