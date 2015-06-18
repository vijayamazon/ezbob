SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BAR_LoadMedalCalculations') IS NULL
	EXECUTE('CREATE PROCEDURE BAR_LoadMedalCalculations AS SELECT 1')
GO

ALTER PROCEDURE BAR_LoadMedalCalculations
@TrailTagID BIGINT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF @TrailTagID IS NULL
	BEGIN
		SELECT
			@TrailTagID = MAX(TrailTagID)
		FROM
			DecisionTrailTags
		WHERE
			TrailTag LIKE '#BravoAutoRpt%'
	END

	;WITH medal_calc AS (
		SELECT
			RowNum = ROW_NUMBER() OVER(PARTITION BY CustomerId ORDER BY CustomerId, Id),
			CustomerId,
			OfferedLoanAmount, MaxOfferedLoanAmount,
			TotalScore, TotalScoreNormalized,
			Medal, MedalType,
			NumOfHmrcMps,
			AnnualTurnover, AnnualTurnoverWeight, AnnualTurnoverGrade, AnnualTurnoverScore,
			FreeCashFlow, FreeCashFlowWeight, FreeCashFlowGrade, FreeCashFlowScore,
			ValueAdded,
			BusinessScore, BusinessScoreWeight, BusinessScoreGrade, BusinessScoreScore,
			TangibleEquity, TangibleEquityWeight, TangibleEquityGrade, TangibleEquityScore,
			BusinessSeniority, BusinessSeniorityWeight, BusinessSeniorityGrade, BusinessSeniorityScore,
			ConsumerScore, ConsumerScoreWeight, ConsumerScoreGrade, ConsumerScoreScore,
			NetWorth, NetWorthWeight, NetWorthGrade, NetWorthScore,
			MaritalStatus, MaritalStatusWeight, MaritalStatusGrade, MaritalStatusScore,
			NumberOfStores, NumberOfStoresWeight, NumberOfStoresGrade, NumberOfStoresScore,
			PositiveFeedbacks, PositiveFeedbacksWeight, PositiveFeedbacksGrade, PositiveFeedbacksScore,
			EzbobSeniority, EzbobSeniorityWeight, EzbobSeniorityGrade, EzbobSeniorityScore,
			NumOfLoans, NumOfLoansWeight, NumOfLoansGrade, NumOfLoansScore,
			NumOfLateRepayments, NumOfLateRepaymentsWeight, NumOfLateRepaymentsGrade, NumOfLateRepaymentsScore,
			NumOfEarlyRepayments, NumOfEarlyRepaymentsWeight, NumOfEarlyRepaymentsGrade, NumOfEarlyRepaymentsScore,
			FirstRepaymentDatePassed,
			AmazonPositiveFeedbacks, EbayPositiveFeedbacks, NumberOfPaypalPositiveTransactions
		FROM
			MedalCalculationsAV mc
		WHERE
			mc.TrailTagID = @TrailTagID
	), no_medal_aa AS (
		SELECT DISTINCT
			t.CustomerID
		FROM
			DecisionTrace tc
			INNER JOIN DecisionTraceNames tcn
				ON tc.TraceNameID = tcn.TraceNameID
				AND tcn.TraceName = 'AutomationCalculator.ProcessHistory.AutoApproval.InitialAssignment'
			INNER JOIN DecisionTrail t
				ON tc.TrailID = t.TrailID
				AND t.TrailTagID = @TrailTagID
		WHERE
			tc.DecisionStatusID != 1
	)
	SELECT
		'Customer ID' = m.CustomerId,
		'Is broker' = CONVERT(BIT, (CASE WHEN c.BrokerId IS NULL THEN 0 ELSE 1 END)),
		'Cash request id' = br.FirstCashRequestID,
		'Manual decision' = d.DecisionName,
		'Underwriter' = u.UserName,
		br.HasEnoughData,
		TotalScore, TotalScoreNormalized,
		Medal, m.MedalType,
		NumOfHmrcMps,
		NumOfCompanyFiles = (
			SELECT COUNT(*)
			FROM MP_CompanyFilesMetaData f
			WHERE f.CustomerId = m.CustomerId
			AND f.Created < r.UnderwriterDecisionDate
		),
		AnnualTurnover, AnnualTurnoverWeight, AnnualTurnoverGrade, AnnualTurnoverScore,
		FreeCashFlow, FreeCashFlowWeight, FreeCashFlowGrade, FreeCashFlowScore,
		ValueAdded,
		BusinessScore, BusinessScoreWeight, BusinessScoreGrade, BusinessScoreScore,
		TangibleEquity, TangibleEquityWeight, TangibleEquityGrade, TangibleEquityScore,
		BusinessSeniority, BusinessSeniorityWeight, BusinessSeniorityGrade, BusinessSeniorityScore,
		ConsumerScore, ConsumerScoreWeight, ConsumerScoreGrade, ConsumerScoreScore,
		NetWorth, NetWorthWeight, NetWorthGrade, NetWorthScore,
		m.MaritalStatus, MaritalStatusWeight, MaritalStatusGrade, MaritalStatusScore,
		NumberOfStores, NumberOfStoresWeight, NumberOfStoresGrade, NumberOfStoresScore,
		PositiveFeedbacks, PositiveFeedbacksWeight, PositiveFeedbacksGrade, PositiveFeedbacksScore,
		EzbobSeniority, EzbobSeniorityWeight, EzbobSeniorityGrade, EzbobSeniorityScore,
		NumOfLoans, NumOfLoansWeight, NumOfLoansGrade, NumOfLoansScore,
		NumOfLateRepayments, NumOfLateRepaymentsWeight, NumOfLateRepaymentsGrade, NumOfLateRepaymentsScore,
		NumOfEarlyRepayments, NumOfEarlyRepaymentsWeight, NumOfEarlyRepaymentsGrade, NumOfEarlyRepaymentsScore,
		FirstRepaymentDatePassed,
		AmazonPositiveFeedbacks, EbayPositiveFeedbacks, NumberOfPaypalPositiveTransactions
	FROM
		medal_calc m
		INNER JOIN Customer c ON m.CustomerId = c.Id
		INNER JOIN no_medal_aa t ON m.CustomerId = t.CustomerID
		INNER JOIN BAR_Results br
			ON m.CustomerId = br.CustomerID
			AND br.TrailTagID = @TrailTagID
		INNER JOIN Decisions d ON br.ManualDecisionID = d.DecisionID
		INNER JOIN CashRequests r ON br.FirstCashRequestID = r.Id
		LEFT JOIN Security_User u ON r.IdUnderwriter = u.UserId
	WHERE
		m.RowNum = 1
	ORDER BY
		m.CustomerId
END
GO
