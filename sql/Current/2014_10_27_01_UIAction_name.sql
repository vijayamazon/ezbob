IF NOT EXISTS (SELECT * FROM UiActions WHERE UiActionName = 'pageload')
	INSERT INTO UiActions (UiActionName) VALUES ('pageload')
GO
