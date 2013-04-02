CREATE OR REPLACE FUNCTION DataDest_AddParameter
(
 pDataDestId number,
 pParameterName varchar2,
 pTypeString varchar2,
 pConstraintString varchar2,
 pDictionaryId number
)
 return NUMBER
AS
  l_ID number;
BEGIN

  select SEQ_DataDestParam.nextval into l_ID from dual;

insert into DataDestinationParams
  (id, name, type, constraint, 
   dictionaryid, destinationid)
values
  (l_ID, pParameterName, pTypeString, pConstraintString, 
   pDictionaryId, pDataDestId);

  return l_ID;

END;
/

