CREATE OR REPLACE PROCEDURE change_dictionary_record

(pDictionaryId number, pRecId number, pInsString varchar2)

 AS

  l_InsString varchar2(2000);
  l_tablename varchar2(40);
  --l_RecId         number;

BEGIN

  select tablename
    into l_tablename
    from dictionaries
   where id = pDictionaryId;

  execute immediate 'delete from ' || l_tablename || ' where id = ' ||
                    pRecId;

  l_InsString := 'insert into ' || l_tablename || ' ' || 'values ( ' ||
                 pRecId || pInsString || ' )';

  --dbms_output.put_line (l_InsString);

  execute immediate l_InsString;

  commit;

END;
/