CREATE OR REPLACE FUNCTION create_new_customer_type
(pDisplayName varchar2, pDescription varchar2) 
RETURN NUMBER
AS

l_tablename varchar2(30);
l_seq number;

  l_tablename_hf     varchar2(30);
  l_seqname_hf	     varchar2(30);
  l_tablename_h      varchar2(30);
  l_tablename_h2hr   varchar2(30);

BEGIN

select SEQ_customer_type.nextval into l_seq from dual ;

  l_tablename := 'CustomerType_'||l_seq;
  l_tablename_hf := 'CustomerType_' || l_seq || '_HFact';
  l_seqname_hf := 'SEQ_CType_' || l_seq;
  l_tablename_h := 'CustomerType_' || l_seq || '_HIST';
  l_tablename_h2hr := 'CustomerType_' || l_seq || '_H2HR';

  execute immediate 'create table ' || l_tablename || ' (id number) ';

  execute immediate 'alter table ' || l_tablename || ' add constraint PK_' ||
                    l_tablename || ' primary key (ID)';

CreateHistoricalTables(
  l_tablename,
  l_tablename_hf,
  l_seqname_hf,
  l_tablename_h,
  l_tablename_h2hr
);

  insert into customertypes
    (id, displayname, description, tablename, 
     HistFactTableName, HistFactSeqName, 
     HistoryTableName , H2HRTableName,
     creationtime, userid, isdeleted)
  values
    (l_seq, pDisplayName, pDescription, l_tablename, 
     l_tablename_hf, l_seqname_hf,
     l_tablename_h, l_tablename_h2hr,
     sysdate, null, null);
commit;

return l_seq;

END;
/
