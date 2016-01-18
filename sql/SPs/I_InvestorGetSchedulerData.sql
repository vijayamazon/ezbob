IF OBJECT_ID('[I_InvestorGetSchedulerData]') IS NULL
	EXECUTE('CREATE PROCEDURE [I_InvestorGetSchedulerData] AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE I_InvestorGetSchedulerData
	@InvestorID INT
AS

BEGIN
	SET NOCOUNT ON

	SELECT 
		i.monthlyFundingCapital AS MonthlyFundingCapital,
		i.FundsTransferDate AS FundsTransferDate,
		i.FundsTransferSchedule AS FundsTransferSchedule,
		i.RepaymentsTransferSchedule AS RepaymentsTransferSchedule
	FROM
		I_Investor i
	WHERE 
		i.InvestorID = @InvestorID
END
GO
