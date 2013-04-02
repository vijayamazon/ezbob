CREATE OR REPLACE PROCEDURE add_dictionary_record

(pDictionaryId number,
 pInsString  varchar2 )

 AS

  l_InsString    varchar2(2000);
  l_tablename    varchar2(40);
  l_RecId         number;
  
   

BEGIN
 
  select SEQ_etl_dict_content.nextval into l_RecId from dual;
 
  select tablename
    into l_tablename
    from dictionaries
   where id = pDictionaryId;


 l_InsString := 'insert into '||l_tablename||' '||'values ( '||
 l_RecId ||pInsString||' )'; 

--dbms_output.put_line (l_InsString);

 execute immediate l_InsString ;

  commit;

END;
/