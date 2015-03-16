IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.GetEscalationData') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.GetEscalationData
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.GetEscalationData 
	(@CustomerId INT)
AS
BEGIN
	SELECT 
		EscalationReason,
		UnderwriterName,
		GreetingMailSentDate,
		MedalType,
		SystemDecision
	FROM 
		Customer 
	WHERE 
		Id = @CustomerId
END
GO
