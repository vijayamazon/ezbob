IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_IsCustomerOffline]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_IsCustomerOffline]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AV_IsCustomerOffline] 
	(@CustomerId INT)
AS
BEGIN
	SELECT (CASE IsOffline WHEN 1 THEN 'True' ELSE 'False' END) AS IsOffline FROM Customer WHERE Id=@CustomerId
END
GO
