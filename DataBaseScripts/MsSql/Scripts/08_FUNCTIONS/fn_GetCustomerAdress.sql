IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fn_GetCustomerAdress]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fn_GetCustomerAdress]
GO
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
from CustomerAddressRelation car
left join CustomerAddress ca on ca.addressId = car.addressId and ca.addressType= '1'
where car.id  IN (select car1.id 
                from CustomerAddressRelation car1 
                where  car1.CustomerId = @CustId)--932)
                AND ca.Line1 is NOT NULL

select 
@Line1Prev =ca.Line1, @Line2Prev =ca.Line2, 
@Line3Prev =ca.Line3, @Line4Prev =ca.Town, 
@Line5Prev =ca.County, @Line6Prev =ca.Postcode
from CustomerAddressRelation car
left join CustomerAddress ca on ca.addressId = car.addressId and ca.addressType = '2'
where car.id  IN (select car1.id 
                from CustomerAddressRelation car1 
                where  car1.CustomerId = @CustId)--932)
                AND ca.Line1 is NOT NULL


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
