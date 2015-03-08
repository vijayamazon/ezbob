IF OBJECT_ID('AlibabaCustomerDataSharing') IS NULL
	EXECUTE('CREATE PROCEDURE AlibabaCustomerDataSharing AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[AlibabaCustomerDataSharing]
	@CustomerID int, @GetFinalDecision bit
AS
BEGIN
	
	SET NOCOUNT ON;	
	
	--create TABLE #customer_data(	
	--	aliMemberId int,
	--	aId int , --default @CustomerID,
	--	loanId int default NULL,
	--	requestedAmt decimal (18,2) default NULL,

	--	firstName nvarchar(250) default NULL, -- c.FirstName
	--	lastName nvarchar(50) default NULL,  -- c.Surname

	--	-- adress type 1 by customer id
	--	personalStreet1 nvarchar(200) default NULL,  -- ad.Line1
	--	personalStreet2 nvarchar(200) default NULL,  -- ad.Line2
	--	personalCity nvarchar(200) default NULL,
	--	personalState nvarchar(200) default NULL,
	--	personalZip nvarchar(200) default NULL,
	--	personalPhone nvarchar(50), -- c.DaytimePhone or c.MobilePhone
	--	personalphoneAlt nvarchar(50) default NULL, --c.DaytimePhone or c.MobilePhone
	--	email nvarchar(128) , -- c.Name
	--	countryId int default 'UK',

	--	-- adress type 3 or 5 by company id
	--	compName nvarchar(300) default NULL, -- co.ExperianCompanyName
	--	compStreetAddr1 nvarchar(200) default NULL, -- ad.Line1
	--	compStreetAddr2 nvarchar(200) default NULL, -- ad.Line2
	--	compCity nvarchar(200) default NULL, -- ad.Town
	--	compState nvarchar(200) default NULL,  -- ad.County
	--	compZip nvarchar(200) default NULL, -- ad.Postcode
	--	compPhone  nvarchar(50) default NULL, 

	--	compEstablished datetime default null , -- customerAnalyticsCompany  IncorporationDate
	--	compEstablishedYears int default null,   -- customerAnalyticsCompany  IncorporationDate
	--	compEmployees int default null,      -- table CompanyEmployeeCount
	--	compEntityType nvarchar(20),   --  	c.TypeOfBusiness as compEntityType TypeOfBusiness {
	----	Entrepreneur = 0, //consumer
	----	LLP = 1,          //company
	----	PShip3P = 2,      //consumer
	----	PShip = 3,        //company
	----	SoleTrader = 4,   //consumer
	----	Limited = 5       //company
	----} 

	----select * from syscolumns where id=OBJECT_ID('customer') and name like '%turnover%'
	
	--	compType nvarchar(50) default NULL,	-- c.IndustryType
	--	compRevenue decimal(18,0) default null, --c.OverallTurnOver
	--	compNetProfit decimal(18, 2) default null, -- not exists
	--	compCreLoans  decimal(18, 2) default null, -- not exists
	--	compRent decimal(18, 2) default null, -- not exists
	--	compOtherLoans decimal(18, 2) default null, -- not exists
	--	compOtherLeases decimal(18, 2) default null, -- not exists

	--	personalIncome decimal(18,0) default null, --c.OverallTurnOver
	--	compOwnershipPercent decimal(18, 2) default null, -- not exists

	--	applicationDate datetime , -- c.GreetingMailSentDate

	--	--“Offer Selected” “App Submitted” “No Loan” “Incomplete” “More Information Needed”

	--	locOfferStatus nvarchar(50), -- dbo.CashRequests r  --r.UnderwriterDecision  dbo.CashRequests r left join dbo.AlibabaBuyer a on r.IdCustomer = a.CustomerId
	--	locOfferAmount decimal(18, 2) default null, -- r.ManagerApprovedSum OR (r.ManagerApprovedSum - ll.loanOpened) as unusedCreditAmount,	
	--	locOfferCurrency nvarchar default 'GBP',
	--	locOfferDate datetime default null, -- r.OfferStart
	--	locOfferExpireDate datetime default null, -- r.OfferValidUntil
	--);
	
	-- public enum TypeOfBusiness {
	--	Entrepreneur = 0, //consumer
	--	LLP = 1,          //company
	--	PShip3P = 2,      //consumer
	--	PShip = 3,        //company
	--	SoleTrader = 4,   //consumer
	--	Limited = 5       //company
	--} 

	--select co.Id, ad.* from dbo.Company co inner join dbo.customer c on co.Id = c.CompanyId inner join dbo.CustomerAddress ad on ad.CompanyId = co.Id where c.Id = 211 and ad.addressType in (3,5);
	--select co.Id, ad.* from dbo.Company co inner join dbo.customer c on co.Id = c.CompanyId inner join dbo.CustomerAddress ad on ad.CustomerId= c.Id where c.Id = 211 and ad.addressType = 1;

	SELECT 
		(select AliId from AlibabaBuyer  where CustomerID = @CustomerID) as  aliMemberId,		
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
		c.TypeOfBusiness as compEntityType, --Possible values: NONE, SOLE_PROPRIETOR, LLC, PARTNERSHIP, LLP, S_CORP, C_CORP

		c.IndustryType as compType,
		c.OverallTurnOver as compRevenue,
	
		c.OverallTurnOver as personalIncome, 

		c.GreetingMailSentDate as applicationDate,
	
		r.UnderwriterDecision as locOfferStatus ,	--  dbo.CashRequests r left join dbo.AlibabaBuyer a on r.IdCustomer = a.CustomerId
		r.ManagerApprovedSum as locOfferAmount,		--  OR (r.ManagerApprovedSum - ll.loanOpened) as unusedCreditAmount,			
		r.OfferStart as locOfferDate , 
		r.OfferValidUntil as locOfferExpireDate 
		
	into
		 #customer_data
	FROM 
		dbo.Customer as c INNER JOIN dbo.CustomerAddress ad on (ad.CustomerId = c.Id and ad.addressType = 1)
		INNER JOIN CustomerStatuses st on st.Id = c.CollectionStatus 		
		LEFT JOIN dbo.Company co on c.CompanyId = co.Id INNER JOIN dbo.CustomerAddress adcomp on (adcomp.CompanyId = co.Id and adcomp.addressType in (3,5))		
		LEFT JOIN (select top 1 IncorporationDate, CustomerID from dbo.CustomerAnalyticsCompany where CustomerID = @CustomerID and IsActive = 1 order by AnalyticsDate desc) as analytic on analytic.CustomerID = c.Id		
		LEFT JOIN (select top 1 			
			IdCustomer, 		
			UnderwriterDecision,		 
			UnderwriterDecisionDate,
			ManagerApprovedSum ,
		--	(r.ManagerApprovedSum - ll.loanOpened) as unusedCreditAmount,						  		
			OfferStart,	
			OfferValidUntil				
		FROM dbo.CashRequests where UnderwriterDecision is not null and IdCustomer= @CustomerID and OfferValidUntil >= GETDATE() order by Id desc) as r on r.IdCustomer = c.Id 
	WHERE 
		c.Id = 211 and st.IsEnabled = 1

	select * from #customer_data;
	
	DROP TABLE #customer_data;

END