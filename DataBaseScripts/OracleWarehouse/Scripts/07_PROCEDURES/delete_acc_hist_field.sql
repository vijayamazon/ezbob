CREATE OR REPLACE PROCEDURE delete_acc_hist_field
(pAccountTypeId number, pFieldName varchar2 )

 AS

  l_tablename varchar2(30);
  l_colname varchar2(30);
BEGIN

  select HistFactTableName
    into l_tablename
    from accounttypes
   where id = paccountTypeId;

  l_colname := upper(pFieldName);
  execute immediate 'alter table ' || l_tablename || ' drop column "' || l_colname || '"';

  delete from AccountTypeHistoricalParams
  where accountTypeId = paccountTypeId and
  upper(FieldName) = upper(pFieldName) ;
commit;
END;
/
