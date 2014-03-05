IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UiEventSave]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UiEventSave]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UiEventSave] 
	(@ActionName NVARCHAR(255), -- must exist
@UserName NVARCHAR(250), -- can be null
@ControlName NVARCHAR(255), -- add if not found
@EventArgs NTEXT,
@BrowserVersionID INT,
@HtmlID NVARCHAR(255),
@RefNum BIGINT,
@RemoteIP NVARCHAR(64),
@SessionCookie NVARCHAR(255),
@EventTime DATETIME)
AS
BEGIN
	DECLARE @ErrMsg NVARCHAR(255) = ''

	DECLARE @ActionID INT = NULL
	DECLARE @UserID INT = NULL
	DECLARE @ControlID INT = NULL

	IF @ErrMsg = ''
	BEGIN
		SELECT
			@ActionID = UiActionID
		FROM
			UiActions
		WHERE
			UiActionName = @ActionName

		IF @ActionID IS NULL
			SET @ErrMsg = 'Action not found by name: ' + @ActionName
	END

	IF @ErrMsg = ''
	BEGIN
		SELECT
			@ControlID = UiControlID
		FROM
			UiControls
		WHERE
			UiControlName = @ControlName

		IF @ControlID IS NULL
		BEGIN
			INSERT INTO UiControls (UiControlName) VALUES (@ControlName)
			SET @ControlID = SCOPE_IDENTITY()
		END

		IF @ControlID IS NULL
			SET @ErrMsg = 'Control not found by name "' + @ControlName + '" and failed to create one.'
	END

	IF @ErrMsg = ''
	BEGIN
		SELECT
			@UserID = UserId
		FROM
			Security_User
		WHERE
			UserName = @UserName
	END

	IF @ErrMsg = ''
	BEGIN
		INSERT INTO UiEvents(
			UiControlID, UiActionID, EventTime, ControlHtmlID, BrowserVersionID,
			UserID, EventRefNum, EventArguments, RemoteIP, SessionCookie
		) VALUES (
			@ControlID, @ActionID, @EventTime, @HtmlID, @BrowserVersionID,
			@UserID, @RefNum, @EventArgs, @RemoteIP, @SessionCookie
		)
	END

	SELECT @ErrMsg AS ErrorMsg
END
GO
