ALTER PROCEDURE [dbo].[GetCaisData]
AS
BEGIN
	SELECT 
		loanID,	
		StartDate,	
		DateClose,		
		CurrentBalance,	
		Gender,	
		FirstName,	
		MiddleInitial,	
		Surname,	
		RefNumber,	
		Line1,	
		Line2,	
		Line3,	
		Town,	
		County,	
		Postcode,	
		DateOfBirth,	
		MinLSDate,
		LoanAmount,
		ScheduledRepayments,
		CompanyType,
		LimitedRefNum,
		NonLimitedRefNum,
		CustomerState,
		SortCode,
		CONVERT(BIT, IsDefaulted) AS IsDefaulted,
		CaisAccountStatus,
		CustomerStatusIsEnabled,
		MaritalStatus,
		CustomerId
	FROM 
		vw_NotClose
END

GO



