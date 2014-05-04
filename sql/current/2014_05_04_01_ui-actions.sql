IF NOT EXISTS (SELECT * FROM UiActions WHERE UiActionName = 'slidestart')
	INSERT INTO UiActions (UiActionName) VALUES ('slidestart')
GO

IF NOT EXISTS (SELECT * FROM UiActions WHERE UiActionName = 'slidestop')
	INSERT INTO UiActions (UiActionName) VALUES ('slidestop')
GO

IF NOT EXISTS (SELECT * FROM UiActions WHERE UiActionName = 'slide')
	INSERT INTO UiActions (UiActionName) VALUES ('slide')
GO
