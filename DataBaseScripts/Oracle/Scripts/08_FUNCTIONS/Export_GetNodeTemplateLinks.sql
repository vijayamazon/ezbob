CREATE OR REPLACE FUNCTION Export_GetNodeTemplateLinks
(
  pTemplateId in number
)
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
		select * from Export_TemplateNodeRel
		where TemplateId = pTemplateId;
  return l_Cursor;

END;
/

