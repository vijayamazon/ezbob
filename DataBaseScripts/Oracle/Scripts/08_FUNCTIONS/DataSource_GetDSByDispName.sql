CREATE OR REPLACE FUNCTION DataSource_GetDSByDispName
(pDisplayName varchar2)
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
       select * from DataSource_Sources
       where DataSource_Sources.DisplayName = pDisplayName
        AND (DataSource_Sources.IsDeleted IS NULL OR DataSource_Sources.IsDeleted = 0)
  AND DataSource_Sources.TerminationDate IS NULL;

  return l_Cursor;

END;
/