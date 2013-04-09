IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMarketPlaceStatus]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetMarketPlaceStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Oleg Zemskyi
-- Create date: 2013-01-22
-- Description:	Get MarketPlace status
-- 23.01.2013	OZ1	Optimization
-- =============================================
create FUNCTION [dbo].[GetMarketPlaceStatus]
(	
  @marketplaceId INT, @customerid INT
)
RETURNS NVARCHAR(16)
AS BEGIN
	DECLARE @status NVARCHAR(64)
	DECLARE @mt TABLE (
	MarketPlaceId INT,
	CustomerId INT,
	STATUS nvarchar (64),
	DisplayName NVARCHAR (64)
	)

INSERT INTO @mt
SELECT MarketPlaceId, CustomerId, 
			   case when updateError is not null or updateError = ''  then 'Error'  
               when UpdatingStart is not null and UpdatingEnd is null then  'Updating' 
               when UpdatingStart is not null and UpdatingEnd is not null then  'Completed'  
			   END
			   'Status', DisplayName 
FROM [dbo].[MP_CustomerMarketPlace] where id IN 
(
	SELECT id  FROM MP_CustomerMarketPlace WHERE marketplaceId = @marketplaceId and customerid = @customerid

)

IF (select COUNT (*) FROM @mt WHERE STATUS='Error')>0 
set @status='Error'
ELSE
	IF (select COUNT (*) FROM @mt WHERE STATUS='Updating')>0
	SET @status= 'Updating'
	ELSE
		IF (select COUNT (*) FROM @mt WHERE STATUS='Completed')>0
		SET @status= 'Completed'
		ELSE 
			SET @status ='N/A'
RETURN @status
end
GO
