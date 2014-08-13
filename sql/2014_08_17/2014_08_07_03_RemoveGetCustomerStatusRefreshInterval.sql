IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomerStatusRefreshInterval]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomerStatusRefreshInterval]
GO
