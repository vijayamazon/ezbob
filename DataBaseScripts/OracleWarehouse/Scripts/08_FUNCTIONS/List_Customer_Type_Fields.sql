CREATE OR REPLACE FUNCTION 
List_Customer_Type_Fields (pCustomerTypeId number) return sys_refcursor
as
  lcur sys_refcursor;
begin
  open lcur for
      select  id, fieldname, fieldtype, dictionaryid,
      (select displayname from dictionaries where id = a.dictionaryid) as ReferencedDictionaryName
      from customertypeparams a
      where customertypeid = pCustomerTypeId;
  return lcur;
end List_Customer_Type_Fields;
/
