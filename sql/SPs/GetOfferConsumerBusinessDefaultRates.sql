IF OBJECT_ID('GetOfferConsumerBusinessDefaultRates') IS NULL
	EXECUTE('CREATE PROCEDURE GetOfferConsumerBusinessDefaultRates AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetOfferConsumerBusinessDefaultRates
@CustomerId INT
AS
BEGIN
	
	DECLARE @TypeOfBusiness NVARCHAR(20)
	DECLARE @ConsumerScore INT 
	DECLARE @BusinessScore INT 
	SELECT  @TypeOfBusiness = TypeOfBusiness FROM Customer WHERE Id=@CustomerId
	
	SELECT @ConsumerScore = isnull(min(x.ExperianConsumerScore),0) FROM
	(
		SELECT ExperianConsumerScore FROM Customer WHERE Id=@CustomerId AND ExperianConsumerScore IS NOT NULL
		UNION
		SELECT ExperianConsumerScore FROM Director WHERE CustomerId=@CustomerId AND ExperianConsumerScore IS NOT NULL
	) x
	
	IF(@TypeOfBusiness = 'Limited' OR @TypeOfBusiness = 'LLP')
	BEGIN
		SELECT @BusinessScore = isnull(Score,0) FROM CustomerAnalyticsCompany WHERE CustomerID=@CustomerId AND IsActive=1
	END 
	ELSE
	BEGIN
		SELECT @BusinessScore = isnull(e.RiskScore,0) FROM Customer c INNER JOIN Company co ON c.CompanyId=co.Id 
		INNER JOIN ExperianNonLimitedResults e ON co.ExperianRefNum=e.RefNumber
		WHERE c.Id=@CustomerId AND e.IsActive=1
	END
	
	IF @ConsumerScore IS NULL SET @ConsumerScore=0
	IF @BusinessScore IS NULL SET @BusinessScore=0
	
	DECLARE @ConsumerDefaultRate DECIMAL(18,6) = 0
	DECLARE @BusinessDefaultRate DECIMAL(18,6) = 0
	
	SELECT @BusinessDefaultRate = Value FROM DefaultRateCompany d WHERE @BusinessScore >= d.Start AND @BusinessScore <= d.[End]
	SELECT @ConsumerDefaultRate = Value FROM DefaultRateCustomer d WHERE @ConsumerScore >= d.Start AND @ConsumerScore <= d.[End]
	
	SELECT @ConsumerDefaultRate AS ConsumerDefaultRate, @BusinessDefaultRate AS BusinessDefaultRate
END