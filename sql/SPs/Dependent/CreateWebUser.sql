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
	DECLARE @rc INT

	IF EXISTS (SELECT u.UserId FROM Security_User u WHERE LOWER(u.UserName) = LOWER(@Email) AND u.IsDeleted != 0)
	BEGIN
		SET @UserID = -1
	END
	ELSE BEGIN
		BEGIN TRAN

		INSERT INTO Security_User (
			PassSetTime, EzPassword, UserName, FullName, Email, BranchId,
			SecurityQuestion1ID, SecurityAnswer1		
		) VALUES (
			@Now, @EzPassword, @Email, @Email, @Email, @BranchID,
			@SecurityQuestionID, @SecurityAnswer
		)

		SET @UserID = SCOPE_IDENTITY()

		INSERT INTO Security_UserRoleRelation (UserId, RoleId)
		SELECT
			@UserID, r.RoleId
		FROM
			Security_Role r
		WHERE
			r.Name = @RoleName

		SET @rc = @@ROWCOUNT

		IF @rc IS NULL OR @rc < 1
		BEGIN
			ROLLBACK TRAN
			SET @UserID = -2
		END

		IF @UserID > 0
			EXECUTE CreateCustomerSession @UserID, @Now, @Ip, 1, 'Registration', NULL, @SessionID OUTPUT

		IF @SessionID > 0
			COMMIT TRAN
		ELSE
			ROLLBACK TRAN
	END

	SELECT
		@UserID AS UserID,
		@SessionID AS SessionID
END
GO
