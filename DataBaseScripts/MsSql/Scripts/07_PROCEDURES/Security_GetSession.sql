IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_GetSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Security_GetSession]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Security_GetSession]
(
	@pSessionId nvarchar(MAX)
) 
AS

BEGIN

       select security_session.userid,
              appid,
              state,
              sessionid,
              security_session.creationdate,
              security_user.username
         from security_session,
              security_user
        where security_user.userid = security_session.userid
        AND security_session.sessionid = @pSessionId;

END
GO
