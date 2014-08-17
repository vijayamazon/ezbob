IF OBJECT_ID('LoadExperianConsumerForCustomer') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianConsumerForCustomer AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadExperianConsumerForCustomer
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @ServiceLogID BIGINT
	DECLARE @InsertDate DATETIME
	------------------------------------------------------------------------------

	SELECT TOP 1
		@ServiceLogID = Id, @InsertDate = InsertDate
	FROM
		MP_ServiceLog l
	WHERE
		l.CustomerId = @CustomerId
		AND
		l.ServiceType = 'Consumer Request'
		AND
		l.DirectorId IS NULL
	ORDER BY
		l.InsertDate DESC,
		l.Id DESC
	
	IF @ServiceLogId IS NULL
	BEGIN
		PRINT 'select by name'
		SELECT TOP 1
		   l.Id, l.InsertDate
		FROM
		 Customer c 
		 INNER JOIN CustomerAddress a ON a.CustomerId = c.Id AND a.addressType=1
		 INNER JOIN MP_ServiceLog l on
		  l.Firstname = c.FirstName AND
		  l.Surname = c.Surname AND 
		  l.DateOfBirth = c.DateOfBirth AND
		  l.Postcode = a.Postcode AND
		  l.ServiceType = 'Consumer Request'
		  
		  WHERE
		   c.Id=@CustomerId
		  ORDER BY
		   l.InsertDate DESC,
		   l.Id DESC
	END
	------------------------------------------------------------------------------

	EXECUTE LoadFullExperianConsumer @ServiceLogID, @InsertDate
END
GO
