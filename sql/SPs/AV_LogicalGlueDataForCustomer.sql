IF OBJECT_ID('AV_LogicalGlueDataForCustomer') IS NULL
	EXECUTE('CREATE PROCEDURE AV_LogicalGlueDataForCustomer AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[AV_LogicalGlueDataForCustomer]
	@CustomerID int,
	@CompanyID int,
	@ProcessingDate datetime
AS
BEGIN
	
	SET NOCOUNT ON;
	
   
	select 
		l.Id as ServiceLogID, --l.InsertDate,
		r.RequestID, r.MonthlyRepayment, --r.UniqueID, 
		rs.ResponseID, rs.ReceivedTime, rs.HttpStatus, rs.ResponseStatus, 
		t.TimeoutSource, 
		rs.ErrorMessage, rs.GradeID, rs.HasEquifaxData, rs.ParsingExceptionType, rs.ParsingExceptionMessage,
		etl.EtlCodeID, etl.EtlDataID, etl.[Message],
		etlcod.EtlCode,
		g.GradeID, g.Name,
		--gr.GradeRangeID, gr.IsActive, gr.IsFirstLoan, gr.LoanSourceID, gr.MaxInterestRate, gr.MaxLoanAmount, gr.MaxSetupFee, gr.MaxTerm, gr.MinInterestRate,gr.MinLoanAmount,gr.MinSetupFee, gr.MinTerm,
		gro.GradeOriginID
	  from 
	   [dbo].[MP_ServiceLog] l join [dbo].[LogicalGlueRequests] r on l.Id=r.ServiceLogID and r.IsTryOut=0 and l.CompanyID=@CompanyID
	   join  [dbo].[LogicalGlueResponses] rs on rs.ServiceLogID=l.Id and l.CustomerId=@CustomerID and l.ServiceType='LogicalGlue' 	   
	   join [dbo].[LogicalGlueModelOutputs] o on rs.ResponseID=o.ResponseID inner join [dbo].[LogicalGlueModels] m on m.ModelID=o.ModelID and m.ModelName='Neural network'
	   and rs.ServiceLogID = 
	   (select MAX(ServiceLogID) as LastResponse 
	from [dbo].[LogicalGlueResponses] rs1 join MP_ServiceLog l1 on l1.Id=rs1.ServiceLogID 
	and l1.CustomerId=l.CustomerId and l1.CompanyID=l.CompanyID
	and l1.InsertDate <= @ProcessingDate)
		left join [dbo].[LogicalGlueTimeoutSources] t on t.TimeoutSourceID=rs.TimeoutSourceID
		left join LogicalGlueEtlData etl on etl.ResponseID=rs.ResponseID join LogicalGlueEtlCodes etlcod on etlcod.EtlCodeID=etl.EtlCodeID
		left join [dbo].[I_Grade] g on g.GradeID=rs.GradeID -- left join [dbo].[I_GradeRange] gr on gr.GradeID=g.GradeID and gr.IsActive=1 --and 
		left join [dbo].[I_GradeOriginMap] gro on gro.GradeID=g.GradeID and gro.OriginID = (select cu.OriginID from Customer cu where cu.Id=@CustomerID)


END