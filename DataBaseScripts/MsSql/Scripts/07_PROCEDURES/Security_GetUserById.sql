IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_GetUserById]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Security_GetUserById]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Security_GetUserById]
(
  @pId bigint
)
AS
BEGIN
    select userid, username, fullname, password, creationdate, isdeleted, email, createuserid, deletiondate, deleteuserid, branchid, passsettime, loginfailedcount, disabledate, lastbadlogin, passexpperiod, forcepasschange, disablepasschange, certificateThumbprint
     from security_user
    WHERE userId = @pId;
END;
GO
