IF OBJECT_ID ('dbo.CustomerInviteFriend') IS NULL
BEGIN 	
	CREATE TABLE dbo.CustomerInviteFriend
		(
		  Id                     INT IDENTITY NOT NULL
		, Created                DATETIME DEFAULT (getdate()) NOT NULL
		, CustomerId             INT NOT NULL
		, InviteFriendSource     NVARCHAR (50)
		, InvitedByFriendSource  NVARCHAR (50)
		, CONSTRAINT PK_CustomerInviteFriend PRIMARY KEY (CustomerId)
		, CONSTRAINT FK_CustomerInviteFriend_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id)
		)
END
GO  

