IF OBJECT_ID('CreateWebUser') IS NULL
	EXECUTE('CREATE PROCEDURE CreateWebUser AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE CreateWebUser
@Email NVARCHAR(250),
@EzPassword VARCHAR(255),
@SecurityQuestionID INT,
@SecurityAnswer VARCHAR(200),
@RoleName NVARCHAR(255),
@BranchID INT,
@Ip NVARCHAR(50),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @UserID INT = 0
	DECLARE @SessionID INT = 0
	DECLARE @OriginID INT = 0
	DECLARE @rc INT

	BEGIN TRANSACTION

	IF EXISTS (SELECT u.UserId FROM Security_User u WHERE LOWER(u.UserName) = LOWER(@Email))
	BEGIN
		SET @UserID = -1
		IF EXISTS (SELECT 1 FROM Customer c WHERE LOWER(c.Name) = LOWER(@Email))
		BEGIN
			SET @OriginID = (SELECT c.OriginID FROM Customer c WHERE LOWER(c.Name) = LOWER(@Email))
		END
	END
	ELSE BEGIN
		BEGIN TRY
			INSERT INTO Security_User (
				PassSetTime, EzPassword, UserName, FullName, Email, BranchId,
				SecurityQuestion1ID, SecurityAnswer1		
			) VALUES (
				@Now, @EzPassword, @Email, @Email, @Email, @BranchID,
				@SecurityQuestionID, @SecurityAnswer
			)

			SET @UserID = SCOPE_IDENTITY()
		END TRY
		BEGIN CATCH
			SET @UserID = -1
		END CATCH

		IF @UserID > 0
		BEGIN
			INSERT INTO Security_UserRoleRelation (UserId, RoleId)
			SELECT
				@UserID, r.RoleId
			FROM
				Security_Role r
			WHERE
				r.Name = @RoleName

			SET @rc = @@ROWCOUNT

			IF @rc IS NULL OR @rc < 1
				SET @UserID = -2
			ELSE
				EXECUTE CreateCustomerSession @UserID, @Now, @Ip, 1, 'Registration', NULL, @SessionID OUTPUT
		END
	END

	IF @SessionID > 0
		COMMIT TRANSACTION
	ELSE
		ROLLBACK TRANSACTION

	SELECT
		@UserID AS UserID,
		@SessionID AS SessionID,
		@OriginID AS OriginID
END
GO
