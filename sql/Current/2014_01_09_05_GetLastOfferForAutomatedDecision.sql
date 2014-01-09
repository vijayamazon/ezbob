IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.GetLastOfferForAutomatedDecision') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.GetLastOfferForAutomatedDecision
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.GetLastOfferForAutomatedDecision 
	(@CustomerId INT)
AS
BEGIN
	DECLARE 
		@RowNum INT,
		@ManualDecisionDate DATETIME,
		@count INT,
		@ReApprovalFullAmountNew FLOAT,
		@ReApprovalRemainingAmountNew FLOAT,
		@ReApprovalFullAmountOld FLOAT,
		@ReApprovalRemainingAmountOld  FLOAT,
		@ManualFundsAdded FLOAT
	
	SET @RowNum = 2

	SELECT 
		@ManualDecisionDate = max(cr.UnderwriterDecisionDate)
	FROM 
		CashRequests cr 
	WHERE 
		cr.IdCustomer = @CustomerId AND 
		cr.UnderwriterDecision = 'Approved'

	SELECT 
		@count=COUNT(1) 
	FROM 
		LoanSchedule
	WHERE 
		Status='Late' AND
		LoanId IN
		(
			SELECT 
				Id 
			FROM 
				Loan 
			WHERE 
				RequestCashId =
				(
					SELECT 
						Id 
					FROM
					(
						SELECT 
							ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
							cr.Id
						FROM 
							CashRequests cr
						WHERE 
							cr.IdCustomer = @CustomerId
					) p
				    WHERE 
						p.row = @RowNum
				)
		) 

	IF @count = 0
	BEGIN
		SELECT 
			@ReApprovalFullAmountNew = 
			(
				SELECT DISTINCT 
					ReApprovalFullAmountNew 
				FROM
					(
						SELECT
							cr.ManagerApprovedSum as ReApprovalFullAmountNew
						FROM 
							CashRequests cr 
							LEFT JOIN Loan l ON l.CustomerId = cr.IdCustomer
						WHERE 
							cr.IdCustomer = @CustomerId AND
							l.Id IS NULL AND 
							cr.id = 
							(
								SELECT
									Id 
								FROM
									(
										SELECT 
											ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
											cr.Id 
										FROM 
											CashRequests cr
										WHERE 
											cr.IdCustomer = @CustomerId
									) p
								WHERE p.row = @RowNum
							) AND
							cr.UnderwriterDecision= 'Approved' AND
							l.Id IS NULL AND
							DATEADD(DD, 30, @ManualDecisionDate) >= GETUTCDATE() AND
							@ManualDecisionDate >= 
							(
								SELECT 
									max(created) 
								FROM 
									MP_CustomerMarketPlace 
								WHERE 
									CustomerId = @CustomerId
							)
					) AS ReApprovalFullAmountNew
			)
	END
	ELSE
	BEGIN
		SELECT @ReApprovalFullAmountNew = NULL
	END
	
	IF @count = 0
	BEGIN
		SELECT 
			@ReApprovalRemainingAmountNew =
			(
				(
					SELECT DISTINCT 
						ReApprovalRemainingAmountNew 
					FROM 
					(
						SELECT 
							cr.ManagerApprovedSum - sum(l.LoanAmount) AS ReApprovalRemainingAmountNew
						FROM 
							CashRequests cr
							LEFT JOIN Loan l ON l.CustomerId = cr.IdCustomer
						WHERE 
							cr.IdCustomer = @CustomerId AND 
							cr.id = 
							(
								SELECT 
									Id 
								FROM
								(
									SELECT 
										ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
										cr.Id 
									FROM 
										CashRequests cr
									WHERE 
										cr.IdCustomer = @CustomerId
								) p
								WHERE 
									p.row = @RowNum
							) AND 
							cr.UnderwriterDecision = 'Approved' AND 
							cr.HasLoans = 1 AND 
							cr.CreationDate <= 
							(
								SELECT 
									Min(l1.date) 
								FROM 
									Loan l1
							) AND 
							DATEADD(DD, 30, @ManualDecisionDate) >= GETUTCDATE() AND 
							@ManualDecisionDate >= 
							(
								SELECT 
									max(created) 
								FROM 
									MP_CustomerMarketPlace 
								WHERE 
									CustomerId = @CustomerId
							) AND 
							l.Status != 'Late' 
						GROUP BY
							ManagerApprovedSum
					) AS ReApprovalRemainingAmountNew
				)
			)
	END
	ELSE 
	BEGIN
		SELECT @ReApprovalRemainingAmountNew = NULL
	END
	
	IF @count = 0
	BEGIN
		SELECT 
			@ReApprovalFullAmountOld = 
			(
				SELECT DISTINCT 
					ReApprovalFullAmount 
				FROM
				(
					SELECT
						cr.ManagerApprovedSum AS ReApprovalFullAmount
					FROM 
						CashRequests cr 
						LEFT JOIN Loan l ON l.CustomerId = cr.IdCustomer
					WHERE 
						cr.IdCustomer = @CustomerId AND 
						cr.id = 
						(
							SELECT 
								Id 
							FROM
								(
									SELECT 
										ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
										cr.Id
									FROM 
										CashRequests cr
									WHERE 
										cr.IdCustomer = @CustomerId
								) p
							WHERE 
								p.row = @RowNum
						) AND 
						cr.UnderwriterDecision= 'Approved' AND 
						@ManualDecisionDate >= 
						(
							SELECT 
								max(created) 
							FROM 
								MP_CustomerMarketPlace 
							WHERE 
								CustomerId = @CustomerId
						) AND 
						cr.HasLoans = 0 AND 
						l.Id IS NOT NULL AND 
						DATEADD(DD, 28, @ManualDecisionDate) >= GETUTCDATE()) AS ReApprovalFullAmount
			)
	END
	ELSE 
	BEGIN
		SELECT @ReApprovalFullAmountOld = NULL
	END

	IF @count=0
	BEGIN
		SELECT 
			@ReApprovalRemainingAmountOld=
			(
				(
					SELECT DISTINCT 
						ReApprovalRemainingAmountOld 
					FROM 
					(
						SELECT 
							cr.ManagerApprovedSum - sum(l.LoanAmount) AS ReApprovalRemainingAmountOld
						FROM 
							CashRequests cr 
							LEFT JOIN Loan l ON l.RequestCashId = cr.id
						WHERE 
							cr.IdCustomer = @CustomerId AND 
							cr.id = 
							(
								SELECT 
									Id 
								FROM
									(
										SELECT 
											ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
											cr.Id 
										FROM 
											CashRequests cr
										WHERE 
											cr.IdCustomer = @CustomerId
									) p
								WHERE 
									p.row = @RowNum
							) AND 
							cr.UnderwriterDecision = 'Approved' AND 
							@ManualDecisionDate >= (SELECT max(created) FROM MP_CustomerMarketPlace WHERE CustomerId=@CustomerId) AND 
							l.Id IS NOT NULL AND 
							cr.CreationDate >= (SELECT Min(l1.date) FROM Loan l1) AND 
							DATEADD(DD, 28, @ManualDecisionDate) >= GETUTCDATE() AND l.Status != 'Late' 
						GROUP BY
							ManagerApprovedSum
					) AS ReApprovalRemainingAmountOld
				)
			)
	END
	ELSE 
	BEGIN
		SELECT @ReApprovalRemainingAmountOld = null
	END
		
	SELECT
		@ReApprovalFullAmountNew AS 'ReApprovalFullAmountNew', 
		@ReApprovalRemainingAmountNew AS 'ReApprovalRemainingAmount', 
		@ReApprovalFullAmountOld AS 'ReApprovalFullAmountOld',
		@ReApprovalRemainingAmountOld AS 'ReApprovalRemainingAmountOld',
		APR,
		RepaymentPeriod,
		ExpirianRating,
		InterestRate,
		UseSetupFee,
		LoanTypeId,
		IsLoanTypeSELECTionAllowed,
		DiscountPlanId,
		LoanSourceID,
		Convert(INT,IsCustomerRepaymentPeriodSELECTionAllowed) AS IsCustomerRepaymentPeriodSELECTionAllowed,
		UseBrokerSetupFee
	FROM 
		CashRequests cr
	WHERE 
		Id =
		(
			SELECT 
				Id 
			FROM
			(
				SELECT 
					ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
					cr.Id 
				FROM 
					CashRequests cr
				WHERE 
					cr.IdCustomer = @CustomerId
			) p
			WHERE 
				p.row = @RowNum
		)
END
GO
