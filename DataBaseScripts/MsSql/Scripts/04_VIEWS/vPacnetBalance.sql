IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vPacnetBalance]'))
DROP VIEW [dbo].[vPacnetBalance]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create VIEW [dbo].[vPacnetBalance]
as

select * from fnPacnetBalance()
GO
