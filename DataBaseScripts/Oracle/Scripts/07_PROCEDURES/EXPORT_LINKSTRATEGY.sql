CREATE OR REPLACE PROCEDURE Export_LinkStrategy
  (
    pTemplateName in varchar,
    pStrategyId in number
  )
AS
  relCount number;
  l_templateId number;
BEGIN
  
  BEGIN
    select id into l_templateId from  export_templateslist
      where upper(filename) = upper(pTemplateName) and isdeleted is null;
    
    select count(*) into relCount from  export_templatestratrel
    where templateid = l_templateId and strategyid = pStrategyId;

   if relCount = 0 then
       insert into export_templatestratrel
         (templateid, strategyid)
       values
         (l_templateId, pStrategyId);
     end if;
      EXCEPTION
           WHEN no_data_found
           THEN RETURN;
   END;
 
END;
/

