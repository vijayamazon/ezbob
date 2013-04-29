
/****** Object:  UserDefinedFunction [dbo].[fn_GetCustomerAdress]    Script Date: 4/29/2013 1:19:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[fn_GetCustomerAdress] (@CustId int)
/*
Функция возвращает адреса кастомера
Author: Oleg Zemskyi
Date Created: 29.09.2012
*/
RETURNS @CustAdress TABLE
(
	Line1 VARCHAR (255), 
	Line2 VARCHAR (255),
	Line3 VARCHAR (255),
	Line4 VARCHAR (255),
	Line5 VARCHAR (255),
	Line6 VARCHAR (255),
	Line1Prev VARCHAR (255), 
	Line2Prev VARCHAR (255),
	Line3Prev VARCHAR (255),
	Line4Prev VARCHAR (255),
	Line5Prev VARCHAR (255),
	Line6Prev VARCHAR (255)	
)
AS BEGIN

DECLARE
@Line1 VARCHAR (255), 
@Line2 VARCHAR (255),
@Line3 VARCHAR (255),
@Line4 VARCHAR (255),
@Line5 VARCHAR (255),
@Line6 VARCHAR (255),
@Line1Prev VARCHAR (255), 
@Line2Prev VARCHAR (255),
@Line3Prev VARCHAR (255),
@Line4Prev VARCHAR (255),
@Line5Prev VARCHAR (255),
@Line6Prev VARCHAR (255)

select 
@line1=ca.Line1, @Line2=ca.Line2, @Line3 =ca.Line3,@Line4 =ca.Town , @Line5 =ca.County, @Line6 =ca.Postcode
from CustomerAddress ca
where ca.addressType= '1'
AND ca.CustomerId = @CustId AND ca.Line1 is NOT NULL

select 
@Line1Prev =ca.Line1, @Line2Prev =ca.Line2, 
@Line3Prev =ca.Line3, @Line4Prev =ca.Town, 
@Line5Prev =ca.County, @Line6Prev =ca.Postcode
from CustomerAddress ca
WHERE ca.addressType = '2'
AND ca.CustomerId = @CustId AND ca.Line1 is NOT NULL


INSERT into @CustAdress
(
	Line1,
	Line2,
	Line3,
	Line4,
	Line5,
	Line6,
	Line1Prev,
	Line2Prev,
	Line3Prev,
	Line4Prev,
	Line5Prev,
	Line6Prev
)
VALUES
(
	@Line1, 
@Line2,
@Line3,
@Line4,
@Line5,
@Line6,
@Line1Prev, 
@Line2Prev,
@Line3Prev,
@Line4Prev,
@Line5Prev,
@Line6Prev
)        
RETURN
end

GO
