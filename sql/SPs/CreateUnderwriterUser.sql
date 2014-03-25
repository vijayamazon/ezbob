IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateUnderwriterUser]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreateUnderwriterUser]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateUnderwriterUser] 
	(@CreationTime DATETIME,
	 @Name NVARCHAR(250),
	 @EncryptedPassword NVARCHAR(200),
	 @RoleId INT)
AS
BEGIN
	INSERT INTO Security_User
	(
		UserName, 
		FullName, 
		Password, 
		CreationDate, 
		IsDeleted, 
		EMail, 
		CreateUserId, 
		DeletionDate, 
		DeleteUserId, 
		BranchId, 
		PassSetTime, 
		LoginFailedCount, 
		DisableDate, 
		LastBadLogin, 
		PassExpPeriod, 
		ForcePassChange, 
		DisablePassChange, 
		DeleteId, 
		CertificateThumbprint, 
		DomainUserName, 
		SecurityQuestion1Id, 
		SecurityAnswer1, 
		IsPasswordRestored
	)
	VALUES
	(
		@Name, 
		@Name, 
		@EncryptedPassword, 
		@CreationTime, 
		0, 
		NULL, 
		NULL, 
		NULL, 
		NULL, 
		1, 
		@CreationTime, 
		0, 
		NULL, 
		NULL, 
		NULL, 
		NULL, 
		NULL, 
		NULL, 
		NULL, 
		NULL, 
		NULL, 
		NULL, 
		0
	)
	
	INSERT INTO Security_UserRoleRelation
	(
		UserId,
		RoleId
	)
	VALUES
	(
		@@Identity,
		@RoleId
	)
END
GO
