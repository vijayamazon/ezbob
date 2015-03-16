UPDATE Security_User SET
	EzPassword = b.Password
FROM 
	Security_User u
	INNER JOIN Broker b ON u.UserId = b.UserID
GO
