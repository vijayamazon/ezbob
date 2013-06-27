IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetNumOfDefaultAccounts]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetNumOfDefaultAccounts]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Oleg Zemskyi
-- Create date: 27.06.2013
-- Description:	Get count from [ExperianDefaultAccount]
-- =============================================
CREATE FUNCTION [dbo].[GetNumOfDefaultAccounts]
(
	@CustomerId INT, @Months INT, @Amount int
)
RETURNS TABLE 
AS
RETURN 
(
	select COUNT(eda.Id) as NumOfDefaultAccounts
	FROM [ExperianDefaultAccount] eda
	where eda.CustomerId = @CustomerId and
	eda.date > dateadd(MM, -@Months, getdate()) and Balance > @Amount
)
GO
