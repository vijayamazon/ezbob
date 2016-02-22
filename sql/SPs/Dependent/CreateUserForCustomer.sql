IF OBJECT_ID('CreateUserForCustomer') IS NULL
	EXECUTE('CREATE PROCEDURE CreateUserForCustomer AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE CreateUserForCustomer
@Email NVARCHAR(250),
@OriginID INT,
@EzPassword VARCHAR(255),
@Salt VARCHAR(255),
@CycleCount VARCHAR(255),
@SecurityQuestionID INT,
@SecurityAnswer VARCHAR(200),
@Ip NVARCHAR(50),
@Now DATETIME
AS
BEGIN
	------------------------------------------------------------------------------
	--
	-- Error codes
	--
	------------------------------------------------------------------------------

	DECLARE @ErrorDuplicateUser         INT = -1
	DECLARE @ErrorOriginNotFound        INT = -2
	DECLARE @ErrorRoleNotFound          INT = -3
	DECLARE @ErrorFailedToCreateUser    INT = -4
	DECLARE @ErrorFailedToAttachRole    INT = -5
	DECLARE @ErrorFailedToCreateSession INT = -6
	DECLARE @ErrorConflictsWithInternal INT = -7
	DECLARE @ErrorConflictsWithBroker   INT = -8

	------------------------------------------------------------------------------
	--
	-- CONST
	--
	------------------------------------------------------------------------------

	DECLARE @BranchID INT = 0 -- this SP is for customers only

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

	IF NOT EXISTS (SELECT * FROM CustomerOrigin WHERE CustomerOriginID = @OriginID)
	BEGIN
		SELECT
			UserID = @ErrorOriginNotFound,
			SessionID = @SessionID

		RETURN
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DECLARE @RoleID INT = (SELECT r.RoleId FROM Security_Role r WHERE r.Name = 'Web')

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

	IF EXISTS (SELECT u.UserId FROM Security_User u WHERE LOWER(u.UserName) = @Email AND OriginID = @OriginID)
	BEGIN
		SELECT
			UserID = @ErrorDuplicateUser,
			SessionID = @SessionID

		RETURN
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	IF EXISTS (SELECT b.BrokerID FROM Broker b WHERE LOWER(b.ContactEmail) = @Email)
	BEGIN
		SELECT
			UserID = @ErrorConflictsWithBroker,
			SessionID = @SessionID

		RETURN
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	IF EXISTS (SELECT u.UserId FROM Security_User u WHERE LOWER(u.UserName) = @Email AND OriginID IS NULL)
	BEGIN
		SELECT
			UserID = @ErrorConflictsWithInternal,
			SessionID = @SessionID

		RETURN
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	BEGIN TRY
		INSERT INTO Security_User (
			PassSetTime, UserName, FullName, Email, BranchId,
			SecurityQuestion1ID, SecurityAnswer1,
			EzPassword, Salt, CycleCount, OriginID
		) VALUES (
			@Now, @Email, @Email, @Email, @BranchID,
			@SecurityQuestionID, @SecurityAnswer,
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
		EXECUTE CreateCustomerSession @UserID, @Now, @Ip, 1, 'Registration', NULL, @SessionID OUTPUT
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
