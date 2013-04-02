CREATE OR REPLACE FUNCTION GetCustomerTypeById
(pCustomerTypeId number)
 return sys_refcursor
as
  lcur sys_refcursor;
begin
  open lcur for
	select
		ID, DisplayName, Description, TableName, 
		HistFactTableName, HistFactSeqName,
		HistoryTableName, H2HRTableName 
	from 	customertypes
	where 	id = pCustomerTypeId; 
  return lcur;
end GetCustomerTypeById;
/
