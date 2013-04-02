CREATE OR REPLACE FUNCTION Export_GetLinkedNodeNames
(
  pTemplateId number
)
 return varchar2
AS
  retVal varchar2(20000);
BEGIN

for rec in (select b.name  from  export_templatenoderel a, strategy_node b
where a.templateid = pTemplateId and a.nodeid = b.nodeid)
   loop
   retVal := retVal||Nvl(rec.name, '') || ';';
   end loop;
 
  return retVal;

END;
/

