CREATE OR REPLACE FUNCTION Cert_Thumbprint_By_UserName
(
  pLogin in varchar
)
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
    select certificateThumbprint as thumbprint, domainUserName
    from security_user
    WHERE UPPER(username) = UPPER(pLogin) AND isdeleted != 2;
    
 return l_Cursor;
 
END;
/
