CREATE OR REPLACE FUNCTION BehavioralLog_GetPath
(
pId IN number
)
 return varchar2
AS
  l_Path varchar2(2048);

BEGIN
  select path into l_Path
  from BehavioralLogs
  where id = pId;

  return l_Path;

END;
/

