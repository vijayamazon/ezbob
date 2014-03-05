IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_GetUserByLogin]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Security_GetUserByLogin]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Security_GetUserByLogin] 
	(@pLogin nvarchar(MAX))
AS
BEGIN
	select userid, username, fullname, password, creationdate, isdeleted, email, createuserid, deletiondate, deleteuserid, branchid, passsettime, loginfailedcount, disabledate, lastbadlogin, passexpperiod, forcepasschange, disablepasschange, certificateThumbprint
     from security_user
    WHERE UPPER(username) = UPPER(@pLogin) AND isdeleted != 1
END
GO
