create or replace function Export_GetResultsByNodeName
(
       pApplicationId in number,
       pNodeName in varchar2
)
return sys_refcursor
AS 
l_Cursor sys_refcursor;
BEGIN
open l_Cursor for
   select export_results.id,
          export_results.filename,
          export_results.filetype,
          export_results.creationdate,
          export_results.applicationid,
          export_results.sourcetemplateid,
          export_results.status,
          export_results.nodename,
          export_results.signeddocumentid
   from export_results
   where export_results.applicationid = pApplicationId
     and export_results.nodename = pNodeName;
  return l_Cursor;
END Export_GetResultsByNodeName;
/