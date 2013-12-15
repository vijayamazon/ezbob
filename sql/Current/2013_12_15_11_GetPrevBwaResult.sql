IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPrevBwaResult]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPrevBwaResult]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPrevBwaResult]
	(@CustomerId INT) 
AS
BEGIN
	SELECT 
		BWAResult 
	FROM 
		Customer
	WHERE 
		Id = @CustomerId
END
GO
