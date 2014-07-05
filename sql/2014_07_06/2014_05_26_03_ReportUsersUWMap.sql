UPDATE ReportUsers SET ReportUsers.UnderwriterId = Security_User.UserId FROM ReportUsers INNER JOIN Security_User ON ReportUsers.UserName=Security_User.UserName
UPDATE ReportUsers SET UnderwriterId = 2561 WHERE UserName='emanuellea'
GO
