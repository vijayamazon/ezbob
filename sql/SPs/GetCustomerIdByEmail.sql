USE [prod_auto]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetCustomerIdByEmail]
@CustomerEmail NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Id  FROM Customer WHERE Name = LTRIM(RTRIM(@CustomerEmail))
	
END
