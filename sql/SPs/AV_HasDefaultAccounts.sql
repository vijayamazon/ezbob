IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_HasDefaultAccounts]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_HasDefaultAccounts]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AV_HasDefaultAccounts] 
	(@CustomerId INT,
 @MinDefBalance INT,
 @Months INT)
AS
BEGIN
	IF EXISTS (SELECT * 
			   FROM ExperianDefaultAccount d 
			   WHERE CustomerId = @CustomerId 
			   AND dateadd(month, @Months, d.Date) > getdate() 
			   AND CurrentDefBalance > @MinDefBalance
			  )
		SELECT 'true'
	ELSE
	    SELECT 'false'
END
GO
