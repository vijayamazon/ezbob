IF EXISTS (SELECT * FROM sysindexes WHERE id = OBJECT_ID('UiControls') AND name = 'IDX_UiControls')
BEGIN
	DROP INDEX UiControls.IDX_UiControls
	DROP INDEX UiActions.IDX_UiActions
	DROP INDEX BrowserVersions.IDX_BrowserVersions
	DROP INDEX UiEvents.IDX_UiEvents

	ALTER TABLE UiControls ADD CONSTRAINT UC_UiControls UNIQUE (UiControlName)
	ALTER TABLE UiActions ADD CONSTRAINT UC_UiActions UNIQUE (UiActionName)
	ALTER TABLE BrowserVersions ADD CONSTRAINT UC_BrowserVersions UNIQUE (BrowserVersion)
	ALTER TABLE UiEvents ADD CONSTRAINT UC_UiEvents UNIQUE (UserID, EventRefNum, SessionCookie)
END
GO
