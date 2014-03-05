IF OBJECT_ID (N'dbo.vPacnetBalance') IS NOT NULL
	DROP VIEW dbo.vPacnetBalance
GO

CREATE VIEW [dbo].[vPacnetBalance]
AS
select * from fnPacnetBalance()

GO

