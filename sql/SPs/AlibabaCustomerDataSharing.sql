IF OBJECT_ID('AlibabaCustomerDataSharing') IS NULL
	EXECUTE('CREATE PROCEDURE AlibabaCustomerDataSharing AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[AlibabaCustomerDataSharing]
	@CustomerID int, @FinalDecision bit
AS
BEGIN
	
	SET NOCOUNT ON;	
	DECLARE @AlibabaMemberID int;	

	SET @AlibabaMemberID = (select AliId from AlibabaBuyer where CustomerID = @CustomerID);	

	-- not an Alibaba's customer
	IF @AlibabaMemberID IS NULL
		RETURN;	
		
	SELECT 
		@AlibabaMemberID as  aliMemberId,		
		@CustomerID as aId ,
		
		--loanId int default NULL,		
		(select top 1 l.Amount from CustomerRequestedLoan l where l.CustomerId = @CustomerID order by l.Id desc) as requestedAmt,

		c.FirstName as firstName,
		c.Fullname as lastName,		
		 
		--  adress type 1 by customer id
		ad.Line1  as personalStreet1,
		ad.Line2 as personalStreet2,
		ad.Town	as personalCity,
		ad.County as personalState,
		ad.Postcode	as personalZip,
		ad.Country as countryId,			

		c.DaytimePhone as personalPhone,		
		c.MobilePhone as personalphoneAlt,

		c.Name as email, 
			
		CASE WHEN @FinalDecision = 1 THEN c.Gender END as gender,	 
		CASE WHEN @FinalDecision = 0 THEN c.GreetingMailSentDate END as applicationDate,		

		co.ExperianCompanyName as  compName,  

		-- adress type 3 or 5 by company id		
		adcomp.Line1 as compStreetAddr1 , 
		adcomp.Line2 as compStreetAddr2,
		adcomp.Town as	compCity , 
		adcomp.County as compState,  
		adcomp.Postcode as compZip ,

		c.DaytimePhone as compPhone ,	

		analytic.IncorporationDate as compEstablished ,		
		datediff(YEAR, analytic.IncorporationDate, GETDATE()) as compEstablishedYears, 		
		(select top 1 EmployeeCount from dbo.CompanyEmployeeCount where CustomerId = @CustomerID order by Id desc) as compEmployees,
		c.TypeOfBusiness as compEntityType, 

		c.IndustryType as compType,		
		r.AnualTurnover as compRevenue, -- Business revenue last year 
		c.OverallTurnOver as compNetProfit, -- Business net profit before taxes last year ???				
	
		--r.UnderwriterDecision as locOfferStatus,	-- auto decision
		--r.ManagerApprovedSum as locOfferAmount,			
		--r.OfferStart as locOfferDate , 
		--r.OfferValidUntil as locOfferExpireDate	,

		r.SystemDecision as locOfferStatus,	-- auto decision 001 RR, R, RA, A
		r.ManagerApprovedSum as locOfferAmount,			
		r.SystemDecisionDate as locOfferDate , 
		r.OfferValidUntil as locOfferExpireDate	,

		r.UnderwriterDecision as locAppriveStatus,	-- manual or final auto -  for 002
		r.ManagerApprovedSum as locAppriveAmount,				
		r.UnderwriterDecisionDate as locAppriveDate , 
		r.OfferValidUntil as locAppriveExpireDate,		
		
		CASE WHEN @FinalDecision = 1 THEN r.UnderwriterComment END as remarks,	
		CASE WHEN @FinalDecision = 1 THEN r.UnderwriterComment END as rejectReason
		
	into
		 #customer_data
	FROM 
		dbo.Customer as c INNER JOIN dbo.CustomerAddress ad on (ad.CustomerId = c.Id and ad.addressType = 1)												-- personal address
		INNER JOIN CustomerStatuses st on st.Id = c.CollectionStatus 																						-- customer disabled ?
		LEFT JOIN dbo.Company co on c.CompanyId = co.Id INNER JOIN dbo.CustomerAddress adcomp on (adcomp.CompanyId = co.Id and adcomp.addressType in (3,5))	-- company data including company address	
		LEFT JOIN (select top 1 IncorporationDate, CustomerID from dbo.CustomerAnalyticsCompany where CustomerID = @CustomerID and IsActive = 1 order by AnalyticsDate desc) as analytic on analytic.CustomerID = c.Id -- 	
		INNER JOIN (select top 1 			
			IdCustomer, 		
			UnderwriterDecision,		 
			UnderwriterDecisionDate,
			SystemDecision,		 
			SystemDecisionDate,
			ManagerApprovedSum ,
			AnualTurnover, 					  		
			OfferStart,	
			OfferValidUntil,
			UnderwriterComment					
		FROM dbo.CashRequests where UnderwriterDecision is not null and IdCustomer= @CustomerID and OfferValidUntil >= GETDATE() order by Id desc) as r on r.IdCustomer = c.Id 
	WHERE 
		c.Id = @CustomerID and st.IsEnabled = 1

	select * from #customer_data;
	
	DROP TABLE #customer_data;

END