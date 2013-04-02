CREATE OR REPLACE Procedure Security_GetUserByLogin
(
  pLogin in varchar,
  cur_OUT in out sys_refcursor
)
AS
BEGIN

  OPEN cur_OUT FOR
    select userid, username, fullname, password, creationdate, isdeleted, email, createuserid, deletiondate, deleteuserid, branchid, passsettime, loginfailedcount, disabledate, lastbadlogin, passexpperiod, forcepasschange, disablepasschange, certificateThumbprint
     from security_user
    WHERE UPPER(username) = UPPER(pLogin) AND isdeleted != 1;
END;

/