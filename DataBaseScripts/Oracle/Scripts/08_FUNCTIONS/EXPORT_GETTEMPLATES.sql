CREATE OR REPLACE FUNCTION Export_GetTemplates
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
   select
     lst.id,
     lst.filename,
     lst.description,
     lst.uploaddate,
     lst.exceptiontype,
     lst.terminationdate,
     lst.displayname,
     (Export_GetEntityLinkSeries(lst.id)) as "LinkedTo",
     (select count(nodeid) from export_templatenoderel where templateid=lst.id) as "nodescount",
     (select count(strategyid) from export_templatestratrel where templateid=lst.id) as "stratcount",
     (Export_GetLinkedNodeNames(lst.id)) as "nodeslist",
     (Export_GetLinkedStrategies(lst.id)) as "stratlist",
     (select fullname from security_user where userid = lst.userid) as "username",
     (Export_GetLinkedFromTemplates(lst.id)) as "linkedfromlist"
   from export_templateslist lst 
   where lst.isdeleted is null;

  return l_Cursor;

END;
/