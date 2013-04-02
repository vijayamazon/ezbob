CREATE OR REPLACE PROCEDURE delete_dictionary(pdictionaryId number)

 AS
  l_tablename varchar2(30);
  l_cnt       number;

BEGIN

  select tablename
    into l_tablename
    from dictionaries
   where id = pdictionaryId;

  execute immediate 'update dictionaries set isdeleted = ''1'' where 
  tablename = '''||l_tablename||'''';

  execute immediate ' select count(*) from ' || l_tablename into l_cnt;

  if l_cnt = 0 then

    execute immediate 'drop table ' || l_tablename;

    delete from dictionaryparams where dictionaryid = pdictionaryId;
    delete from dictionaries where Id = pdictionaryId;



  end if;
commit;
END;
/
