CREATE OR REPLACE FUNCTION
 create_new_dictionary (pDisplayName varchar2, pDescription varchar2) 
RETURN NUMBER
AS
  l_tablename varchar2(30);
  l_seq number;
BEGIN

select SEQ_dictionary.nextval into l_seq from dual ;

  l_tablename := 'Dictionary_'||l_seq;

  execute immediate 'create table ' || l_tablename || ' (id number, value varchar2(256)) ';

  execute immediate 'alter table ' || l_tablename || ' add constraint PK_' ||
                    l_tablename || ' primary key (ID)';

  execute immediate 'alter table ' || l_tablename || ' add constraint IX_' ||
                    l_tablename || ' unique (VALUE)';

  insert into dictionaries
    (id, displayname, tablename, description, creationtime, userid, isdeleted)
  values
    (l_seq, pDisplayName, l_tablename, pDescription, sysdate, null, null);

  return l_seq;
commit;
END;
/
