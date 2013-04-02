create or replace function export_getexportresults
(
       pApplicationId in number,
       pNodeId in number
)
return sys_refcursor
AS
  l_Cursor sys_refcursor;
begin
  open l_Cursor for
   select export_results.id,
          export_results.filename,
          export_results.binarybody,
          export_results.filetype,
          export_results.creationdate,
          export_results.applicationid,
          export_results.sourcetemplateid,
          export_results.status,
          export_results.nodename,
          export_results.signeddocumentid
   from export_results
        inner join export_templatenoderel on
          export_templatenoderel.templateid = export_results.sourcetemplateid
          and export_templatenoderel.outputtype = export_results.filetype
   where export_results.applicationid = pApplicationId
     and export_templatenoderel.nodeid = pNodeId
     and export_results.statusmode is null
   union all
   select export_results.id,
          export_results.filename,
          export_results.binarybody,
          export_results.filetype,
          export_results.creationdate,
          export_results.applicationid,
          export_results.sourcetemplateid,
          export_results.status,
          export_results.nodename,
          export_results.signeddocumentid
   from export_results
        inner join entityLink l ON l.EntityType='ExportTemplate'
          and export_results.sourcetemplateid = l.seriaId
        inner join export_templatenoderel on export_templatenoderel.templateid = l.entityId
          and export_templatenoderel.outputtype = export_results.filetype
   where export_results.applicationid = pApplicationId
     and export_templatenoderel.nodeid = pNodeId
     and export_results.statusmode is null
   order by creationdate desc;
  return l_Cursor;
end export_getexportresults;
/
