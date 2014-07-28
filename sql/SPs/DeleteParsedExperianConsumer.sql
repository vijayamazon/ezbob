IF OBJECT_ID('DeleteParsedExperianConsumer') IS NULL
	EXECUTE('CREATE PROCEDURE DeleteParsedExperianConsumer AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE DeleteParsedExperianConsumer
@ServiceLogID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DELETE
		ExperianConsumerDataCaisCardHistory
	FROM
		ExperianConsumerDataCaisCardHistory ch
		INNER JOIN ExperianConsumerDataCais cc ON ch.ExperianConsumerDataCaisId = cc.Id
		INNER JOIN ExperianConsumerData c ON cc.ExperianConsumerDataId = c.Id
	WHERE
		c.ServiceLogId = @ServiceLogID
		
	DELETE
		ExperianConsumerDataCaisBalance
	FROM
		ExperianConsumerDataCaisBalance cb
		INNER JOIN ExperianConsumerDataCais cc ON cb.ExperianConsumerDataCaisId = cc.Id
		INNER JOIN ExperianConsumerData c ON cc.ExperianConsumerDataId = c.Id
	WHERE
		c.ServiceLogId = @ServiceLogID	

	------------------------------------------------------------------------------

	DELETE ExperianConsumerDataCais      FROM ExperianConsumerDataCais      d INNER JOIN ExperianConsumerData c ON d.ExperianConsumerDataId = c.Id  WHERE c.ServiceLogId = @ServiceLogID
	DELETE ExperianConsumerDataResidency FROM ExperianConsumerDataResidency r INNER JOIN ExperianConsumerData c ON r.ExperianConsumerDataId = c.Id  WHERE c.ServiceLogId = @ServiceLogID
	DELETE ExperianConsumerDataLocation  FROM ExperianConsumerDataLocation  l INNER JOIN ExperianConsumerData c ON l.ExperianConsumerDataId = c.Id  WHERE c.ServiceLogId = @ServiceLogID
	DELETE ExperianConsumerDataNoc       FROM ExperianConsumerDataNoc       n INNER JOIN ExperianConsumerData c ON n.ExperianConsumerDataId = c.Id  WHERE c.ServiceLogId = @ServiceLogID
	DELETE ExperianConsumerDataApplicant FROM ExperianConsumerDataApplicant a INNER JOIN ExperianConsumerData c ON a.ExperianConsumerDataId = c.Id  WHERE c.ServiceLogId = @ServiceLogID
	
	------------------------------------------------------------------------------

	DELETE FROM ExperianConsumerData WHERE ServiceLogID = @ServiceLogID
END

GO