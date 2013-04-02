CREATE OR REPLACE FUNCTION DataSource_GetDSByName
(
  pDataSourceName IN varchar2
) return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
       select * from datasource_sources
       where (isdeleted is null) AND DisplayName =  pDataSourceName;
  return l_Cursor;

END;
/

