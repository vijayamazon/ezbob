CREATE OR REPLACE FUNCTION Export_GetTemplateById
(
  pTemplateId in number
)
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
       select id, uploaddate, exceptiontype,variablesxml, binarybody, signedDocument
       from export_templateslist
       where id = pTemplateId;
  return l_Cursor;

END;
/

