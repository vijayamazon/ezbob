CREATE OR REPLACE FUNCTION 
List_Customer_Type_All return sys_refcursor
as
  lcur sys_refcursor;
begin
  open lcur for
	select
		ID, DisplayName, Description, TableName, 
		HistFactTableName, HistFactSeqName,
		HistoryTableName, H2HRTableName		
	from customertypes;
  return lcur;
end List_Customer_Type_All;
/
