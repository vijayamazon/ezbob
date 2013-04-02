CREATE OR REPLACE FUNCTION 
List_AccType_HistFields 
		(pAccountTypeId number) 
	return sys_refcursor
as
  lcur sys_refcursor;
begin
  open lcur for
      select  id, fieldname, fieldtype, dictionaryid,
      (select displayname from dictionaries where id = a.dictionaryid) as ReferencedDictionaryName
      from AccountTypeHistoricalParams a
      where accounttypeid = pAccountTypeId;
  return lcur;
end List_AccType_HistFields;
/