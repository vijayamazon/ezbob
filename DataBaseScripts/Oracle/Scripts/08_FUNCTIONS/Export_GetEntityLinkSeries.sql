CREATE OR REPLACE FUNCTION Export_GetEntityLinkSeries
(
  pEntityLinkId number
)
 return varchar2
AS
  retVal varchar2(20000);
BEGIN

for rec in (
      select l.SERIAID
      from
        entitylink l 
      where
        l.EntityType='ExportTemplate'
        and l.EntityId=pEntityLinkId)
   loop
   retVal := retVal||Nvl(rec.SERIAID, '') || ';';
   end loop;
 
  return retVal;

END;
/