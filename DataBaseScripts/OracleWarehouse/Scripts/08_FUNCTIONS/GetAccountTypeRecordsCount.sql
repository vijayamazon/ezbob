CREATE OR REPLACE FUNCTION GetAccountTypeRecordsCount
(pAccountTypeId number)
  return number 
  
  as
  
  l_records number;
  l_table varchar2(30);
begin

    select tablename
      into l_table
      from ACCOUNTTYPES
     where ID = pAccountTypeId;

  execute immediate 'select count(*) from ' || l_table into l_records ;
  return l_records;


end GetAccountTypeRecordsCount;
/
