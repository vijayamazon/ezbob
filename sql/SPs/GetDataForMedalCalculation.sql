IF OBJECT_ID('GetDataForMedalCalculation') IS NULL
	EXECUTE('CREATE PROCEDURE GetDataForMedalCalculation AS SELECT 1')
GO

ALTER PROCEDURE GetDataForMedalCalculation
@CustomerId INT
AS
BEGIN
	DECLARE 
		@FirstRepaymentDate DATETIME, 
		@FirstRepaymentDatePassed BIT, 
		@OnTimeLoans INT, 
		@NumOfLatePayments INT, 
		@NumOfEarlyPayments INT,
		@BusinessScore INT,
		@ConsumerScore INT,
		@TangibleEquity DECIMAL(18,6),
		@BusinessSeniority DATETIME,
		@EzbobSeniority DATETIME,
		@MaritalStatus NVARCHAR(50),
		@HmrcId INT,
		@YodleeTotalAggrgationFuncId INT,
		@YodleeTurnover DECIMAL(18,6),
		@ZooplaEstimate NVARCHAR(30), 
		@AverageSoldPrice1Year INT

	SELECT 
		@BusinessScore = Score, 
		@TangibleEquity = TangibleEquity, 
		@BusinessSeniority = IncorporationDate 
	FROM 
		CustomerAnalyticsCompany 
	WHERE 
		CustomerID = @CustomerId

	SELECT 
		@ConsumerScore = MIN(ExperianScore) 
	FROM 
		MP_ExperianDataCache 
	WHERE 
		CustomerId = @CustomerId AND 
		Name IS NOT NULL
		
	SELECT 
		@EzbobSeniority = GreetingMailSentDate, 
		@MaritalStatus = MaritalStatus
	FROM 
		Customer 
	WHERE 
		Id = @CustomerId

	SELECT @FirstRepaymentDatePassed = 0
	SELECT 
		@FirstRepaymentDate = MIN(LoanSchedule.Date) 
	FROM 
		LoanSchedule, 
		Loan 
	WHERE 
		Loan.Id = LoanSchedule.LoanId AND 
		Loan.CustomerId = @CustomerId

	IF @FirstRepaymentDate IS NOT NULL AND @FirstRepaymentDate < GETUTCDATE()
		SELECT @FirstRepaymentDatePassed = 1

	SELECT 
		@OnTimeLoans = COUNT(Loan.Id) 
	FROM 
		Loan,
		LoanSchedule
	WHERE
		Loan.CustomerId = @CustomerId AND
		Loan.Id = LoanSchedule.LoanId AND
		(LoanSchedule.Status = 'PaidOnTime' OR LoanSchedule.Status = 'PaidEarly') AND 
		LoanSchedule.Id = 
		(
			SELECT 
				MAX(LoanSchedule.Id) 
			FROM 
				Customer,
				LoanSchedule, 
				Loan
			WHERE 
				Loan.CustomerId = Customer.Id AND
				LoanSchedule.LoanId = Loan.Id AND
				Customer.Id = Loan.CustomerId
		)

	SELECT 
		@NumOfLatePayments = COUNT(LoanCharges.Id) 
	FROM 
		LoanCharges,
		ConfigurationVariables,  
		Loan
	WHERE 
		ConfigurationVariables.Name = 'LatePaymentCharge' AND
		ConfigurationVariables.Id = LoanCharges.ConfigurationVariableId AND
		Loan.Id = LoanCharges.LoanId AND
		Loan.CustomerId = @CustomerId

	SELECT 
		@NumOfEarlyPayments = COUNT(LoanSchedule.id) 
	FROM 
		LoanSchedule, 
		Loan
	WHERE 
		LoanSchedule.Status = 'PaidEarly' AND
		Loan.Id = LoanSchedule.LoanId AND
		Loan.CustomerId = @CustomerId
		
	SELECT TOP 1
		@HmrcId = MP_CustomerMarketplace.Id
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		CustomerId = @CustomerId AND
		MP_CustomerMarketplace.MarketPlaceId = MP_MarketplaceType.Id AND
		MP_MarketplaceType.Name = 'HMRC'
		
	IF @HmrcId IS NULL
		SELECT @HmrcId = 0
		
	SELECT @YodleeTotalAggrgationFuncId = MP_AnalyisisFunction.Id FROM MP_AnalyisisFunction, MP_MarketplaceType WHERE MP_AnalyisisFunction.MarketPlaceId=MP_MarketplaceType.Id AND MP_MarketplaceType.Name = 'Yodlee' AND MP_AnalyisisFunction.Name='TotalIncomeAnnualized'
	
	SELECT TOP 1 
		@YodleeTurnover = ValueFloat 
	FROM 
		MP_AnalyisisFunctionValues, 
		MP_CustomerMarketPlace 
	WHERE 
		MP_AnalyisisFunctionValues.CustomerMarketPlaceId = MP_CustomerMarketPlace.Id AND 
		MP_CustomerMarketPlace.CustomerId = @CustomerId AND 
		AnalyisisFunctionId = @YodleeTotalAggrgationFuncId 
	ORDER BY 
		AnalysisFunctionTimePeriodId DESC
	
	SELECT TOP 1 @ZooplaEstimate = Zoopla.ZooplaEstimate, @AverageSoldPrice1Year = Zoopla.AverageSoldPrice1Year FROM Zoopla, CustomerAddress WHERE CustomerId = @CustomerId AND CustomerAddress.addressId = Zoopla.CustomerAddressId AND CustomerAddress.addressType = 1 ORDER BY UpdateDate DESC 
				
	SELECT
		@FirstRepaymentDatePassed AS FirstRepaymentDatePassed, 
		@OnTimeLoans AS OnTimeLoans, 
		@NumOfLatePayments AS NumOfLatePayments, 
		@NumOfEarlyPayments AS NumOfEarlyPayments,
		@BusinessScore AS BusinessScore,
		@ConsumerScore AS ConsumerScore,
		@TangibleEquity AS TangibleEquity,
		@BusinessSeniority AS BusinessSeniority,
		@EzbobSeniority AS EzbobSeniority,
		@MaritalStatus AS MaritalStatus,
		@HmrcId AS HmrcId,
		@YodleeTurnover AS YodleeTurnover,
		@ZooplaEstimate AS ZooplaEstimate,
		@AverageSoldPrice1Year AS AverageSoldPrice1Year
END
GO
