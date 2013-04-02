CREATE OR REPLACE FUNCTION Security_GetSession
(
	pSessionId IN VARCHAR2
) return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
       select security_session.userid,
              appid,
              state,
              sessionid,
              security_session.creationdate,
              security_user.username
         from security_session,
              security_user
        where security_user.userid = security_session.userid
        AND security_session.sessionid = psessionid;
  return l_Cursor;

END;
/

