IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertStrategyCustomerUpdateTime]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertStrategyCustomerUpdateTime]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertStrategyCustomerUpdateTime] 
(@CustomerId int,
 @StartDate datetime,
 @EndDate datetime)
      
           
AS
BEGIN

insert INTO  [dbo].[Strategy_CustomerUpdateHistory] (CustomerId, StartDate, EndDate)
     VALUES (@CustomerId, @StartDate, @EndDate);

SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO
