IF OBJECT_ID('DeleteParsedExperianLtd') IS NULL
	EXECUTE('CREATE PROCEDURE DeleteParsedExperianLtd AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE DeleteParsedExperianLtd
@ServiceLogID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DELETE
		ExperianLtdLenderDetails
	FROM
		ExperianLtdLenderDetails ld
		INNER JOIN ExperianLtdDL65 dl65 ON ld.DL65ID = dl65.ExperianLtdDL65ID
		INNER JOIN ExperianLtd e ON dl65.ExperianLtdID = e.ExperianLtdID
	WHERE
		e.ServiceLogID = @ServiceLogID

	------------------------------------------------------------------------------

	DELETE ExperianLtdPrevCompanyNames FROM ExperianLtdPrevCompanyNames d INNER JOIN ExperianLtd e ON d.ExperianLtdID = e.ExperianLtdID WHERE e.ServiceLogID = @ServiceLogID
	DELETE ExperianLtdCaisMonthly      FROM ExperianLtdCaisMonthly      d INNER JOIN ExperianLtd e ON d.ExperianLtdID = e.ExperianLtdID WHERE e.ServiceLogID = @ServiceLogID
	DELETE ExperianLtdShareholders     FROM ExperianLtdShareholders     d INNER JOIN ExperianLtd e ON d.ExperianLtdID = e.ExperianLtdID WHERE e.ServiceLogID = @ServiceLogID
	DELETE ExperianLtdDLB5             FROM ExperianLtdDLB5             d INNER JOIN ExperianLtd e ON d.ExperianLtdID = e.ExperianLtdID WHERE e.ServiceLogID = @ServiceLogID
	DELETE ExperianLtdDL72             FROM ExperianLtdDL72             d INNER JOIN ExperianLtd e ON d.ExperianLtdID = e.ExperianLtdID WHERE e.ServiceLogID = @ServiceLogID
	DELETE ExperianLtdCreditSummary    FROM ExperianLtdCreditSummary    d INNER JOIN ExperianLtd e ON d.ExperianLtdID = e.ExperianLtdID WHERE e.ServiceLogID = @ServiceLogID
	DELETE ExperianLtdDL48             FROM ExperianLtdDL48             d INNER JOIN ExperianLtd e ON d.ExperianLtdID = e.ExperianLtdID WHERE e.ServiceLogID = @ServiceLogID
	DELETE ExperianLtdDL52             FROM ExperianLtdDL52             d INNER JOIN ExperianLtd e ON d.ExperianLtdID = e.ExperianLtdID WHERE e.ServiceLogID = @ServiceLogID
	DELETE ExperianLtdDL68             FROM ExperianLtdDL68             d INNER JOIN ExperianLtd e ON d.ExperianLtdID = e.ExperianLtdID WHERE e.ServiceLogID = @ServiceLogID
	DELETE ExperianLtdDL97             FROM ExperianLtdDL97             d INNER JOIN ExperianLtd e ON d.ExperianLtdID = e.ExperianLtdID WHERE e.ServiceLogID = @ServiceLogID
	DELETE ExperianLtdDL99             FROM ExperianLtdDL99             d INNER JOIN ExperianLtd e ON d.ExperianLtdID = e.ExperianLtdID WHERE e.ServiceLogID = @ServiceLogID
	DELETE ExperianLtdDLA2             FROM ExperianLtdDLA2             d INNER JOIN ExperianLtd e ON d.ExperianLtdID = e.ExperianLtdID WHERE e.ServiceLogID = @ServiceLogID
	DELETE ExperianLtdDL65             FROM ExperianLtdDL65             d INNER JOIN ExperianLtd e ON d.ExperianLtdID = e.ExperianLtdID WHERE e.ServiceLogID = @ServiceLogID

	------------------------------------------------------------------------------

	DELETE FROM ExperianLtd WHERE ServiceLogID = @ServiceLogID
END
GO
