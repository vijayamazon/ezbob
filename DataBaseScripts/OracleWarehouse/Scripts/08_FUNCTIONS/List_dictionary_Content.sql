CREATE OR REPLACE FUNCTION List_dictionary_Content(pdictionaryId number)
  return sys_refcursor as
  lcur        sys_refcursor;
  l_tablename varchar2(40);
  l_str varchar2(2000);
  
begin

l_str := ' id ';

for rec1 in (select * from dictionaryparams where dictionaryid = pdictionaryid) 
loop

l_str := l_str||' ,';

l_str := l_str||' '||rec1.fieldname||' '||rec1.displayname||' ';

end loop;

  select tablename
    into l_tablename
    from dictionaries
   where id = pdictionaryid;

--dbms_output.put_line ('select '||l_str||' from '||l_tablename);

  open lcur for
    'select '||l_str||' from '||l_tablename;

  return lcur;
end List_dictionary_content;
/