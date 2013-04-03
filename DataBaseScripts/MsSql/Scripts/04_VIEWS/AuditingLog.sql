IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[AuditingLog]'))
DROP VIEW [dbo].[AuditingLog]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[AuditingLog]
AS
SELECT        Id, Date AS [Date], Message, Exception
FROM            dbo.Log4Net
WHERE        (Logger = 'Scorto.Security.UserManagement.Sessions.SessionManager')
GO
