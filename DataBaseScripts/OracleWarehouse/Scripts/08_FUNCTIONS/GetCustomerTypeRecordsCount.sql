CREATE OR REPLACE FUNCTION GetCustomerTypeRecordsCount
(pCustomerTypeId number)
  return number 
  
  as
  
  l_records number;
  l_table varchar2(30);
begin

    select tablename
      into l_table
      from CUSTOMERTYPES
     where ID = pCustomerTypeId;

  execute immediate 'select count(*) from ' || l_table into l_records ;
  return l_records;


end GetCustomerTypeRecordsCount;
/
