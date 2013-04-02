CREATE OR REPLACE FUNCTION create_new_account_type
(pDisplayName    varchar2,
 pDescription    varchar2,
 pCustomerTypeId number)
RETURN NUMBER
 AS
  l_tablename   varchar2(30);
  l_tablename_cust varchar2(30);

  l_tablename_hf   varchar2(30);
  l_seqname_hf	     varchar2(30);
  l_tablename_h     varchar2(30);
  l_tablename_h2hr   varchar2(30);


  l_seq number;
BEGIN

  select SEQ_account_type.nextval into l_seq from dual;

  l_tablename := 'AccountType_' || l_seq;
  l_tablename_hf := 'AccountType_' || l_seq || '_HFact';
  l_seqname_hf := 'SEQ_AccType_' || l_seq;
  l_tablename_h := 'AccountType_' || l_seq || '_HIST';
  l_tablename_h2hr := 'AccountType_' || l_seq || '_H2HR';

  execute immediate 'create table ' || l_tablename ||
                    ' (id number, customerid number) ';

  execute immediate 'alter table ' || l_tablename || ' add constraint PK_' ||
                    l_tablename || ' primary key (ID)';

  select tablename
    into l_tablename_cust
    from customertypes
   where id = pCustomerTypeId;

  execute immediate 'alter table ' || l_tablename || ' add constraint ' ||
                    'FK_' || l_tablename || ' foreign key (customerid) references '|| l_tablename_cust|| '(ID) on delete cascade ';

CreateHistoricalTables(
  l_tablename,
  l_tablename_hf,
  l_seqname_hf,
  l_tablename_h,
  l_tablename_h2hr
);

  insert into accounttypes
    (id,
     displayname,
     description,
     tablename,
     HistFactTableName, 
     HistFactSeqName,
     HistoryTableName, 
     H2HRTableName,
     customertypeid,
     creationtime,
     userid,
     isdeleted)
  values
    (l_seq,
     pDisplayName,
     pDescription,
     l_tablename,
     l_tablename_hf,
     l_seqname_hf,
     l_tablename_h,
     l_tablename_h2hr,
     pCustomerTypeId,
     sysdate,
     null,
     null);
commit;

return l_Seq;

END;
/
