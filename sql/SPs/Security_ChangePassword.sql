IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_ChangePassword]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Security_ChangePassword]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Security_ChangePassword] 
	(@pLogin nvarchar(250),
 @pOldPassword nvarchar(max),
 @pNewPassword nvarchar(max),
 @pMaxOldPassCheck bigint)
AS
BEGIN
	DECLARE  @pUserId bigint,
	@pDisabledChange bigint,
	@pForceChange bigint;
  -- Check login and old password-------------------------------------------------------------
     select @pUserId=userid, @pDisabledChange=disablepasschange, @pForceChange=forcepasschange
       from security_user
     where  upper(username)  = upper(@pLogin) and password = @pOldPassword and isdeleted <> 1;
	 IF (@pUserId is null)
		BEGIN
			SELECT 2; -- invalid login
			RETURN 2;
		END
   -- Check change permission ------------------------------------------------------------------
   if (@pDisabledChange is not null and @pForceChange is null) 
	BEGIN
		SELECT 3; -- not allowed
		RETURN 3;
	END
 
 -- Old passwords check ----------------------------------------------------------------------
  /*if (@pMaxOldPassCheck is not null)
      BEGIN
        IF EXISTS(
        			SELECT * FROM (select TOP(@pMaxOldPassCheck+1) * from security_accountlog
							where eventtype = 1 and userid=@pUserId 
					order by eventdate DESC) AS tbl1 WHERE tbl1.[Data]=@pNewPassword
				)
			BEGIN
				SELECT 4;
				RETURN 4; -- Old pass validation failed
			END
      END*/
 
 update security_user
                set password = @pNewPassword,
                      passsettime = GETDATE(),
                      loginfailedcount = null,
                      lastbadlogin = null,
                      forcepasschange = null
                where userid = @pUserId; 
 -- add log entry for changing password
 insert into security_accountlog (eventtype, userid, data)
                values (1, @pUserId, @pNewPassword);
SELECT 1; 
RETURN 1; -- change ok  
END
GO
