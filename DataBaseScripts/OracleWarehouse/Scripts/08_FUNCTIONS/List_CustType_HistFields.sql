CREATE OR REPLACE FUNCTION 
List_CustType_HistFields
(pCustomerTypeId number) return sys_refcursor
as
  lcur sys_refcursor;
begin
  open lcur for
      select  id, fieldname, fieldtype, dictionaryid,
      (select displayname from dictionaries where id = a.dictionaryid) as ReferencedDictionaryName
      from CustomerTypeHistoricalParams a
      where customertypeid = pCustomerTypeId;
  return lcur;
end List_CustType_HistFields;
/
