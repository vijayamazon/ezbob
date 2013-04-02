CREATE OR REPLACE FUNCTION DataSource_InsertNew
(pDataSourceName in varchar2,
 pDataSourceType in varchar2,
 pDocument in clob,
 pSignedData in clob,
 pDisplayName in varchar2,
 pDescription in varchar2,
 pUserId in number
)
 return NUMBER
AS
  l_ID number;
BEGIN

  select SEQ_DATASOURCE_SOURCE.nextval into l_ID from dual;

  UPDATE DataSource_Sources
  SET TerminationDate = sysdate
  WHERE TerminationDate IS NULL and DisplayName = pDisplayName;

  insert into datasource_sources
  (
     id
    ,name
    ,Description
    ,Type
    ,Document
    ,SIGNEDDOCUMENT
    ,CreationDate
    ,DisplayName
    ,UserId)
  values
  (
     l_ID
    ,pDataSourceName
    ,pDescription
    ,pDataSourceType
    ,pDocument
    ,pSignedData
    ,sysdate
    ,pDisplayName
    ,pUserId);

  return l_ID;

END;
/

