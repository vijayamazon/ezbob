CREATE OR REPLACE FUNCTION List_account_Type_All return sys_refcursor as
  lcur sys_refcursor;
begin
  open lcur for
	select
		a.ID, a.DisplayName, a.Description, a.TableName, 
		a.HistFactTableName, a.HistFactSeqName,
		a.HistoryTableName, a.H2HRTableName, 
		a.customertypeid, b.displayname as CustomerTypeName
      from accounttypes a, customertypes b
     where a.customertypeid = b.id;
  return lcur;
end List_account_Type_All;
/
