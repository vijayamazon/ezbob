IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAllCustomerMPUMI]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetAllCustomerMPUMI]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetAllCustomerMPUMI]
(
	@CustomerId  int,
	@iNumberOfMarcketPlaces int output,
	@iCustomerMarketPlaceId	int output

)
AS
BEGIN

   
	SELECT @iNumberOfMarcketPlaces = COUNT(cmp.Id)
	FROM MP_CustomerMarketPlace cmp
		LEFT JOIN Customer c ON cmp.CustomerId = c.Id
	WHERE c.Id = @CustomerId
	


	SELECT cmp.Id
	FROM MP_CustomerMarketPlace cmp
		LEFT JOIN Customer c ON cmp.CustomerId = c.Id
	WHERE c.Id = @CustomerId
	
		


END
GO
