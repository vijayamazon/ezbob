IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllCustomersWithCompany]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAllCustomersWithCompany]
GO
