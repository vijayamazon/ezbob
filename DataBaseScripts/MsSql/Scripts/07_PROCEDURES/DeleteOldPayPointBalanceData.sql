IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteOldPayPointBalanceData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteOldPayPointBalanceData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE DeleteOldPayPointBalanceData
@Date DATE
AS
BEGIN
	DELETE FROM
		PayPointBalance
	WHERE
		CONVERT(DATE, date) = @Date
	
	UPDATE LoanTransaction SET
		Reconciliation = 'not tested'
	WHERE
		CONVERT(DATE, PostDate) = @Date
		AND
		Type = 'PaypointTransaction'
END
GO
