IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreNewMedalForComparison1]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StoreNewMedalForComparison1]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreNewMedalForComparison2]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StoreNewMedalForComparison2]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDataForMedalCalculation2]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetDataForMedalCalculation2]
GO

IF OBJECT_ID('NewMedalComparison1') IS NOT NULL
	DROP TABLE NewMedalComparison1
GO

IF OBJECT_ID('NewMedalComparison2') IS NOT NULL
	DROP TABLE NewMedalComparison2
GO
