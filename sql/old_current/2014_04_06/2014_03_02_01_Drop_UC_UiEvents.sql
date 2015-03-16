IF EXISTS (SELECT * FROM sysobjects WHERE name = 'UC_UiEvents')
	ALTER TABLE UiEvents DROP CONSTRAINT UC_UiEvents
GO
