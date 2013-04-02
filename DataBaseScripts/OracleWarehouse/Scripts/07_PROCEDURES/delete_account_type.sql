CREATE OR REPLACE PROCEDURE delete_account_type
(pAccountTypeId number)
 AS
  l_tablename varchar2(30);
  l_cnt       number;

  l_tablename_hf   varchar2(30);
  l_tablename_h     varchar2(30);
  l_tablename_h2hr   varchar2(30);

BEGIN

    select tablename,
           HistFactTableName,
           HistoryTableName,
           H2HRTableName
      into l_tablename, l_tablename_hf, l_tablename_h, l_tablename_h2hr
      from accounttypes
     where id = paccountTypeId;

execute immediate 'select count(*)  from '|| l_tablename into l_cnt;

  if l_cnt = 0 then

    execute immediate 'drop table ' || l_tablename_h2hr;
    execute immediate 'drop table ' || l_tablename_h;
    execute immediate 'drop table ' || l_tablename_hf;
    execute immediate 'drop table ' || l_tablename;

    delete from accounttypehistoricalparams where accounttypeid = paccountTypeId;
    delete from accounttypeparams where accounttypeid = paccountTypeId;
    delete from accounttypes where Id = paccountTypeId;

  end if;
commit;
END;
/