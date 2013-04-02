CREATE OR REPLACE FUNCTION BehavioralReport_GetPath
(
pId IN number
)
 return varchar2
AS
  l_Path varchar2(2048);

BEGIN
  select path into l_Path
  from BehavioralReports
  where id = pId;

  return l_Path;

END;
/

