create or replace function Security_ChangePassword
(
 pLogin varchar2,
 pOldPassword varchar2,
 pNewPassword varchar2,
 pMaxOldPassCheck number
 )
return number
AS
 pUserId number;
 pDisabledChange number;
 pTempId number;
 pForceChange number;
BEGIN
  -- Check login and old password-------------------------------------------------------------
   begin
     select userid, disablepasschange, forcepasschange
       into pUserId, pDisabledChange, pForceChange from security_user
     where  upper(username)  = upper(pLogin) and password = pOldPassword and isdeleted <> 1;
      exception
        when no_data_found then return 2; -- invalid login
   end;
   -- Check change permission ------------------------------------------------------------------
   if (pDisabledChange is not null and pForceChange is null) then return 3; -- not allowed
   end if;
 
 -- Old passwords check ----------------------------------------------------------------------
  if (pMaxOldPassCheck is not null) then
      begin
        select id into pTempId from (select id, rank() over (order by eventdate desc) as rank, eventdate, eventtype, userid, data from security_accountlog
        where eventtype = 1 and userid=pUserId)
        where rank <= pMaxOldPassCheck+1 and data = pNewPassword;
        return 4; -- Old pass validation failed
        exception
            when no_data_found then pTempId := null; -- no equals founded, validation passed
      end;
  end if;
 
 update security_user
                  set password = pNewPassword,
                      passsettime = sysdate,
                      loginfailedcount = null,
                      lastbadlogin = null,
                      forcepasschange = null
                where userid = pUserId; 
                -- add log entry for changing password
                insert into security_accountlog
                  (id, eventtype, userid, data)
                values
                  (seq_security_accountlog.nextval, 1, pUserId, pNewPassword);
 return 1; -- change ok     

END;
/
