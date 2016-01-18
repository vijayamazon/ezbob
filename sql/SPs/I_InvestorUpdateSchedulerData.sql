IF OBJECT_ID('I_InvestorUpdateSchedulerData') IS NULL
	EXECUTE('CREATE PROCEDURE I_InvestorUpdateSchedulerData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE I_InvestorUpdateSchedulerData
	@InvestorID INT,
	@MonthlyFundingCapital DECIMAL(18,6),
	@FundsTransferDate INT,
	@FundsTransferSchedule NVARCHAR(255),
	@RepaymentsTransferSchedule NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;		

	UPDATE I_Investor set 
			MonthlyFundingCapital = @MonthlyFundingCapital, 
			FundsTransferDate = @FundsTransferDate,
			FundsTransferSchedule= @FundsTransferSchedule,
			RepaymentsTransferSchedule = @RepaymentsTransferSchedule
			WHERE 
			InvestorID = @InvestorID	
	
	SELECT @InvestorID AS InvestorID
END
GO
