CREATE OR REPLACE FUNCTION datasource_getdatasourcebyname
(pDataSourceName varchar2)
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
       select * from datasource_sources
       where name = pDataSourceName
        AND (DataSource_Sources.IsDeleted IS NULL OR DataSource_Sources.IsDeleted = 0);

  return l_Cursor;

END;
/

