CREATE OR REPLACE FUNCTION GetAccountTypeById 
(pAccountTypeId number)
return sys_refcursor as
  lcur sys_refcursor;
begin
  open lcur for
	select 
		a.ID, a.DisplayName, a.Description, a.TableName, 
		a.HistFactTableName, a.HistFactSeqName,
		a.HistoryTableName, a.H2HRTableName, 
		a.CustomerTypeId, b.displayname as CustomerTypeName
	from 	accounttypes a, customertypes b
	where 	a.ID = pAccountTypeId 
		and a.customertypeid = b.id;

  return lcur;
end GetAccountTypeById;
/
