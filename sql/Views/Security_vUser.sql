IF OBJECT_ID (N'dbo.Security_vUser') IS NOT NULL
	DROP VIEW dbo.Security_vUser
GO

CREATE VIEW [dbo].[Security_vUser]
AS
SELECT     u.UserId, u.UserName, u.FullName, u.EMail, u.IsDeleted, s.SessionCreationDate, u.CreationDate, u.PassExpPeriod, u.ForcePassChange, 
           u.DisablePassChange, u.CertificateThumbprint
FROM         Security_User AS u LEFT OUTER JOIN
                          (SELECT    UserId, MAX(CreationDate) AS SessionCreationDate
                            FROM          Security_Session
                            GROUP BY UserId) AS s ON s.UserId = u.UserId

GO

