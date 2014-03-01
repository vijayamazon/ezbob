IF OBJECT_ID('dbo.udfGetErrorMsg') IS NOT NULL
	DROP FUNCTION dbo.udfGetErrorMsg
GO

CREATE FUNCTION dbo.udfGetErrorMsg()
RETURNS NVARCHAR(1024)
AS
BEGIN
	RETURN
		'Error #' + CONVERT(NVARCHAR, ERROR_NUMBER()) +
			' severity ' + CONVERT(NVARCHAR, ERROR_SEVERITY()) +
			' state ' + CONVERT(NVARCHAR, ERROR_STATE()) +
			' in procedure ' + ERROR_PROCEDURE() +
			' at line ' + CONVERT(NVARCHAR, ERROR_LINE()) +
			': ' + ERROR_MESSAGE()
END
GO

IF OBJECT_ID('UiEventSave') IS NULL
	EXECUTE('CREATE PROCEDURE UiEventSave AS SELECT 1')
GO

ALTER PROCEDURE UiEventSave
@ActionName NVARCHAR(255), -- must exist
@UserName NVARCHAR(250), -- can be null
@ControlName NVARCHAR(255), -- add if not found
@EventArgs NTEXT,
@BrowserVersionID INT,
@HtmlID NVARCHAR(255),
@RefNum BIGINT,
@RemoteIP NVARCHAR(64),
@SessionCookie NVARCHAR(255),
@EventTime DATETIME
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
