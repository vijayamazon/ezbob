IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetNumberOfDefaultAccounts]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetNumberOfDefaultAccounts]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetNumberOfDefaultAccounts] 
	(@CustomerId INT,
	 @Months INT, 
	 @Amount INT)
AS
BEGIN
	SELECT * FROM [GetNumOfDefaultAccounts] (@CustomerId, @Months, @Amount)
END
GO
