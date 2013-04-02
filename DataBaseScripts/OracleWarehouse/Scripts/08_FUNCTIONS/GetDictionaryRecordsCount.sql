CREATE OR REPLACE FUNCTION GetDictionaryRecordsCount
(pDictionaryId number)
  return number 
  
  as
  
  l_records number;
  l_table varchar2(30);
begin

    select tablename
      into l_table
      from DICTIONARIES
     where ID = pDictionaryId;

  execute immediate 'select count(*) from ' || l_table into l_records ;
  return l_records;


end GetDictionaryRecordsCount;
/
