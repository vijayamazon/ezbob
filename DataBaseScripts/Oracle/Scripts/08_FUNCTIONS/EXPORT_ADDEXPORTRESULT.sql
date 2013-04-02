CREATE OR REPLACE function Export_AddExportResult
  (
    pFileName in varchar2,
    pFileType in number,
    pSourceTemplateId in number,
    pBinaryBody in blob,
    pApplicationId in number,
    pStatus in number,
    pNodeName in varchar2
  )  return number
AS
 l_export_result_id Number;
BEGIN
   update export_results
     set statusmode = 1
   where id in (
	   select id from export_results
	   where applicationid = pApplicationId and sourcetemplateid = pSourceTemplateId
     and filetype = pFileType
    union
       select e.Id from export_results e
				inner join entityLink l ON l.EntityType='ExportTemplate'
				  and e.sourcetemplateid = l.EntityId
			where e.applicationid = pApplicationId 
			and l.SeriaId = pSourceTemplateId
			and e.filetype = pFileType
   );
   
   Select seq_export_results.Nextval into l_export_result_id from dual;

   insert into export_results
     (id, filename, binarybody, filetype,
      sourcetemplateid, applicationid, status, nodename)
   values
     (l_export_result_id, pFileName, pBinaryBody, pFileType,
       pSourceTemplateId, pApplicationId, pStatus, pNodeName);
   return l_export_result_id;
   
END;
/

