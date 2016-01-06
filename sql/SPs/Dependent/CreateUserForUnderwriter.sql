IF OBJECT_ID('CreateUserForUnderwriter') IS NULL
	EXECUTE('CREATE PROCEDURE CreateUserForUnderwriter AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE CreateUserForUnderwriter
@UserName NVARCHAR(250),
@EzPassword VARCHAR(255),
@Salt VARCHAR(255),
@CycleCount VARCHAR(255),
@RoleName NVARCHAR(255),
@Now DATETIME
AS
BEGIN
	------------------------------------------------------------------------------
	--
	-- Error codes
	--
	------------------------------------------------------------------------------

	DECLARE @ErrorDuplicateUser         INT = -1
	DECLARE @ErrorRoleNotFound          INT = -2
	DECLARE @ErrorFailedToCreateUser    INT = -3
	DECLARE @ErrorFailedToAttachRole    INT = -4
	DECLARE @ErrorFailedToCreateSession INT = -5

	------------------------------------------------------------------------------
	--
	-- CONST
	--
	------------------------------------------------------------------------------

	DECLARE @BranchID INT = 1 -- this SP is for underwriters only
	DECLARE @OriginID INT = NULL

	------------------------------------------------------------------------------
	--
	-- VARIABLES
	--
	------------------------------------------------------------------------------

	DECLARE @UserID INT = 0
	DECLARE @SessionID INT = 0

	------------------------------------------------------------------------------
	--
	-- Start.
	--
	------------------------------------------------------------------------------

	DECLARE @RoleID NVARCHAR(255) = (SELECT r.RoleId FROM Security_Role r WHERE LOWER(r.Name) = @RoleName)

	------------------------------------------------------------------------------

	IF @RoleID IS NULL
	BEGIN
		SELECT
			UserID = @ErrorRoleNotFound,
			SessionID = @SessionID

		RETURN
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	IF EXISTS (SELECT u.UserId FROM Security_User u WHERE LOWER(u.UserName) = @UserName)
	BEGIN
		SELECT
			UserID = @ErrorDuplicateUser,
			SessionID = @SessionID

		RETURN
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	BEGIN TRY
		INSERT INTO Security_User (
			PassSetTime, UserName, FullName, Email, BranchId,
			EzPassword, Salt, CycleCount, OriginID
		) VALUES (
			@Now, @UserName, @UserName, @UserName, @BranchID,
			@EzPassword, @Salt, @CycleCount, @OriginID
		)

		SET @UserID = SCOPE_IDENTITY()
	END TRY
	BEGIN CATCH
		SELECT
			UserID = @ErrorFailedToCreateUser,
			SessionID = @SessionID

		RETURN
	END CATCH

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	BEGIN TRY
		INSERT INTO Security_UserRoleRelation (UserId, RoleId) VALUES (@UserID, @RoleID)
	END TRY
	BEGIN CATCH
		SELECT
			UserID = @ErrorFailedToAttachRole,
			SessionID = @SessionID

		RETURN
	END CATCH

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	BEGIN TRY
		EXECUTE CreateCustomerSession @UserID, @Now, '::1', 1, 'Registration', NULL, @SessionID OUTPUT
	END TRY
	BEGIN CATCH
		SELECT
			UserID = @ErrorFailedToCreateSession,
			SessionID = @SessionID

		RETURN
	END CATCH

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		UserID = @UserID,
		SessionID = @SessionID
END
GO
