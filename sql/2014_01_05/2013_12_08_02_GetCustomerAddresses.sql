IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomerAddresses]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomerAddresses]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCustomerAddresses] 
	(@CustomerId INT)
AS
BEGIN
	SELECT * FROM fn_GetCustomerAdress (@CustomerId)		
END
GO
