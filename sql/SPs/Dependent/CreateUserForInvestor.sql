IF OBJECT_ID('CreateUserForInvestor') IS NULL
	EXECUTE('CREATE PROCEDURE CreateUserForInvestor AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE CreateUserForInvestor
@UserName NVARCHAR(250),
@EzPassword VARCHAR(255),
@Salt VARCHAR(255),
@CycleCount VARCHAR(255),
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
	DECLARE @ErrorConflictsWithLead     INT = -5

	------------------------------------------------------------------------------
	--
	-- CONST
	--
	------------------------------------------------------------------------------

	DECLARE @BranchID INT = 2 -- this SP is for investors only

	------------------------------------------------------------------------------
	--
	-- VARIABLES
	--
	------------------------------------------------------------------------------

	DECLARE @UserID INT = 0

	------------------------------------------------------------------------------
	--
	-- Start.
	--
	------------------------------------------------------------------------------

	DECLARE @RoleID INT = (SELECT r.RoleId FROM Security_Role r WHERE r.Name = 'InvestorWeb')

	------------------------------------------------------------------------------

	IF @RoleID IS NULL
	BEGIN
		SELECT
			UserID = @ErrorRoleNotFound

		RETURN
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	IF EXISTS (SELECT u.UserId FROM Security_User u WHERE LOWER(u.UserName) = @UserName)
	BEGIN
		SELECT
			UserID = @ErrorDuplicateUser

		RETURN
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	IF EXISTS (SELECT bl.BrokerLeadID FROM BrokerLeads bl WHERE LOWER(bl.Email) = @UserName)
	BEGIN
		SELECT
			UserID = @ErrorConflictsWithLead

		RETURN
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	BEGIN TRY
		INSERT INTO Security_User (
			PassSetTime, UserName, FullName, Email, BranchId,
			EzPassword, Salt, CycleCount
		) VALUES (
			@Now, @UserName, @UserName, @UserName, @BranchID,
			@EzPassword, @Salt, @CycleCount
		)

		SET @UserID = SCOPE_IDENTITY()
	END TRY
	BEGIN CATCH
		SELECT
			UserID = @ErrorFailedToCreateUser

		RETURN
	END CATCH

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	BEGIN TRY
		INSERT INTO Security_UserRoleRelation (UserId, RoleId) VALUES (@UserID, @RoleID)
	END TRY
	BEGIN CATCH
		SELECT
			UserID = @ErrorFailedToAttachRole

		RETURN
	END CATCH

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		UserID = @UserID
END
GO
