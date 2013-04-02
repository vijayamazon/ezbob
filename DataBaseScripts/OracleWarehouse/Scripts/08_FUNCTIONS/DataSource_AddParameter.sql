CREATE OR REPLACE FUNCTION DataSource_AddParameter
(pDataSourceId number,
 pParameterName varchar2,
 pTypeString varchar2,
 pConstraintString varchar2,
 pIsHistorical number,
 pIsIdentity number,
 pDictionaryId number
)
 return NUMBER
AS
  l_ID number;
BEGIN

  select SEQ_DataSourceParam.nextval into l_ID from dual;

insert into datasourceparams
  (id, name, type, constraint, 
   ishistorical, isidentity, dictionaryid,    
   datasourceid)
values
  (l_ID, pParameterName, pTypeString, pConstraintString, 
   pIsHistorical, pIsIdentity, pDictionaryId, 
   pDataSourceId);

  return l_ID;

END;
/

