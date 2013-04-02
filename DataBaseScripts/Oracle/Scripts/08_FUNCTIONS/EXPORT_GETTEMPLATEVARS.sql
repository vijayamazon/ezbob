CREATE OR REPLACE FUNCTION Export_GetTemplateVars
(
  pTemplateId in number
)
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
       select variablesxml
       from export_templateslist
       where id = pTemplateId;
  return l_Cursor;

END;
/

