CREATE OR REPLACE PROCEDURE delete_dictionary_record

(pDictionaryId number, pRecordId number)

 AS

  l_tablename varchar2(40);

BEGIN

  select tablename into l_tablename from dictionaries where id = pDictionaryId;

  execute immediate 'delete from ' || l_tablename || ' where id = ' ||
                    pRecordId;

  commit;
END;
/