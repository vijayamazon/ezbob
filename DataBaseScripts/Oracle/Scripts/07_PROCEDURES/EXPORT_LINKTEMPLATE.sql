CREATE OR REPLACE PROCEDURE Export_LinkTemplate
  (
    pTemplateId in number,
    pNodeId in number,
    pOutputType in number,
    pIsLinked in number
  )
AS
BEGIN
   delete from export_templatenoderel
   where templateid = pTemplateId and nodeid = pNodeId;
   
   if pIsLinked is not null then
       insert into export_templatenoderel
         (templateid, nodeid, outputtype)
       values
         (pTemplateId, pNodeId, pOutputType);
   end if;
END;
/

