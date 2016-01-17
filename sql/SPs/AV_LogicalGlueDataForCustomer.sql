IF OBJECT_ID('AV_LogicalGlueDataForCustomer') IS NULL
	EXECUTE('CREATE PROCEDURE AV_LogicalGlueDataForCustomer AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

LTER PROCEDURE [dbo].[AV_LogicalGlueDataForCustomer]
	@CustomerID int,
	@CompanyID int,
	@PlannedPayment decimal (18,6),
	@ProcessingDate datetime
AS
BEGIN
	
	SET NOCOUNT ON;
	
   
	select 
		l.Id as ServiceLogID,
		r.RequestID, 
		rs.ResponseID, --rs.ReceivedTime, rs.HttpStatus, rs.ResponseStatus, 
		rs.ErrorMessage, --rs.HasEquifaxData, rs.ParsingExceptionType, rs.ParsingExceptionMessage, 
		rs.GradeID,
		--t.TimeoutSource, 		
		etl.EtlDataID, etl.[Message],etlcod.EtlCode,
		mo.ModelID, mo.ErrorCode, mo.ModelOutputID, mo.Score--,
	--	g.GradeID, g.Name--,
		--gr.GradeRangeID, gr.IsActive, gr.IsFirstLoan, gr.LoanSourceID, gr.MaxInterestRate, gr.MaxLoanAmount, gr.MaxSetupFee, gr.MaxTerm, gr.MinInterestRate,gr.MinLoanAmount,gr.MinSetupFee, gr.MinTerm--,
		--gro.GradeOriginID
	  from 
	   [dbo].[MP_ServiceLog] l join [dbo].[LogicalGlueRequests] r on 	 
	 l.Id=r.ServiceLogID	 
	 and r.ServiceLogID=(select MAX(r1.ServiceLogID) as LatestRequest from [dbo].[LogicalGlueRequests] r1 join MP_ServiceLog l1 on l1.Id=r1.ServiceLogID and l1.CustomerId=@CustomerID and l1.CompanyID=@CompanyID 
and l1.ServiceType='LogicalGlue'
and r1.MonthlyRepayment=@PlannedPayment 
and r1.IsTryOut=0 and l1.InsertDate <= @ProcessingDate)

	  left join  [dbo].[LogicalGlueResponses] rs on rs.ServiceLogID=l.Id 
	  left join LogicalGlueEtlData etl on etl.ResponseID=rs.ResponseID join LogicalGlueEtlCodes etlcod on etlcod.EtlCodeID=etl.EtlCodeID

	  left join [dbo].[LogicalGlueModelOutputs] mo on rs.ResponseID=mo.ResponseID left join [dbo].[LogicalGlueModels] m on m.ModelID=mo.ModelID and m.ModelName='Neural network'	   
		--left join [dbo].[LogicalGlueTimeoutSources] t on t.TimeoutSourceID=rs.TimeoutSourceID
		
		--left join [dbo].[I_Grade] g on g.GradeID=rs.GradeID --left join [dbo].[I_GradeRange] gr on gr.GradeID=g.GradeID and gr.IsActive=1 --and 
		--left join [dbo].[I_GradeOriginMap] gro on gro.GradeID=g.GradeID and gro.OriginID = (select cu.OriginID from Customer cu where cu.Id=@CustomerID)

END