CREATE OR REPLACE FUNCTION Export_GetLinkedFromTemplates
(
  pTemplateId number
)
 return varchar2
AS
  retVal varchar2(20000);
BEGIN

for rec in (
      select t.id
      from
        entitylink l inner join Export_TemplatesList t on 
            l.EntityId = t.id 
        and l.EntityType='ExportTemplate'
        and t.IsDeleted is null
      where
        l.seriaId=pTemplateId)
   loop
   retVal := retVal||Nvl(rec.id, '') || ';';
   end loop;
 
  return retVal;

END;
/

