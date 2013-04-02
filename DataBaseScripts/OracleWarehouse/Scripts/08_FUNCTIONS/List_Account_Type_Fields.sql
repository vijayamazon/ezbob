CREATE OR REPLACE FUNCTION 
List_Account_Type_Fields (pAccountTypeId number) return sys_refcursor
as
  lcur sys_refcursor;
begin
  open lcur for
      select  id, fieldname, fieldtype, dictionaryid,
      (select displayname from dictionaries where id = a.dictionaryid) as ReferencedDictionaryName
      from accounttypeparams a
      where accounttypeid = paccountTypeId;
  return lcur;
end List_Account_Type_Fields;
/
