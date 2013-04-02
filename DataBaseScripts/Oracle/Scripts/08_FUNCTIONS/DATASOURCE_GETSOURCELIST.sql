CREATE OR REPLACE FUNCTION DataSource_GetDataSourcesList
(
  pDataSourceType IN varchar2
) return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
       select
        ds.ID,
        ds.NAME,
        ds.Description,
        ds.TYPE,
        ds.CREATIONDATE,
        ds.TERMINATIONDATE,
        ds.DISPLAYNAME,
        usr.username
       from datasource_sources ds 
        left outer join security_user usr on ds.userid = usr.userid
       where (ds.isdeleted is null) AND ds.Type=pDataSourceType;
  return l_Cursor;

END;
/

