IF OBJECT_ID (N'dbo.AuditingLog') IS NOT NULL
	DROP VIEW dbo.AuditingLog
GO

CREATE VIEW [dbo].[AuditingLog]
AS
SELECT        Id, Date AS [Date], Message, Exception
FROM            dbo.Log4Net
WHERE        (Logger = 'Scorto.Security.UserManagement.Sessions.SessionManager')

GO

