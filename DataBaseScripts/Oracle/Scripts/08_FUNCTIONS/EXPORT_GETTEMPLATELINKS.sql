CREATE OR REPLACE FUNCTION Export_GetTemplateLinks
(
  pNodeId in number,
  pShowHistory in number
)
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
       select id, displayname, terminationdate,
       (select outputtype from export_templatenoderel
       where templateid=id and nodeid = pNodeId) as "outputtype"
       from export_templateslist
       where isdeleted is null
       and 
       ((pShowHistory is null and terminationdate is null)
       or (pShowHistory is not null))
       order by displayname asc, terminationdate desc; 
  return l_Cursor;

END;
/

