CREATE OR REPLACE FUNCTION GetCustomerTypeReferenceCount
(pCustomerTypeId number)
  return number 
  
  as
  
  l_refs number;

begin

    select count(*)
      into l_refs
      from ACCOUNTTYPES
     where CustomerTypeId = pCustomerTypeId;

  return l_refs;


end GetCustomerTypeReferenceCount;
/
