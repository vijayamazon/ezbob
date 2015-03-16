IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCaisData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCaisData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCaisData]
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
		CONVERT(BIT, IsDefaulted),
		CaisAccountStatus,
		CustomerStatusIsEnabled,
		MaritalStatus
	FROM 
		vw_NotClose
END
GO
