SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS(SELECT * FROM syscolumns WHERE name = 'AnnualTurnover' AND id = OBJECT_ID('CustomerManualUwData'))
	EXEC sp_RENAME 'CustomerManualUwData.AnnualTurnover' , 'AnnualizedRevenue', 'COLUMN'
GO
