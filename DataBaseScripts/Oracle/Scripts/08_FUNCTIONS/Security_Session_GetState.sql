create or replace function Security_Session_GetState
(
 pAppId number,
 pSessionId varchar2
 )
return number
AS
 l_SessionState NUMBER;
BEGIN

  SELECT s.state INTO l_SessionState FROM security_session s
  WHERE s.appid = pAppId AND s.sessionid = pSessionId;
  
  return l_SessionState;
  
END;
/

