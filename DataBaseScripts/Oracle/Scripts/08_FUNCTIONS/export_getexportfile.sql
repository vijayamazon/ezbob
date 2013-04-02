create or replace function export_getexportfile
(
       fileId in number
)
return sys_refcursor
AS
  l_Cursor sys_refcursor;
begin
  open l_Cursor for
       select export_results.filename,
              export_results.binarybody,
              export_results.applicationid,
              export_results.nodename,
              export_results.signeddocumentid
       from export_results
       where export_results.id = fileId;
  return l_Cursor;
end export_getexportfile;
/
