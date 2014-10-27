IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MainStrategyGetConfigs]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[MainStrategyGetConfigs]
GO

