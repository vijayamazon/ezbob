CREATE OR REPLACE function GetEntityLinks
(
 pEntitySeriaId number,
 pEntityType VARCHAR2
)
 return sys_refcursor
as
  l_cur sys_refcursor;
begin
 open l_cur for
   SELECT LinksDoc AS DescriptionDocument
   FROM EntityLink
   WHERE EntityType = pEntityType
     AND (   (SeriaId = pEntitySeriaId 
              AND NOT (pEntitySeriaId is null))
          OR (pEntitySeriaId is null));

  return l_cur;
end;
/
