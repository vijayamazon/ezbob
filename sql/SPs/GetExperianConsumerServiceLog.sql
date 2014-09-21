SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetExperianConsumerServiceLog') IS NULL
	EXECUTE('CREATE PROCEDURE GetExperianConsumerServiceLog AS SELECT 1')
GO

ALTER PROCEDURE GetExperianConsumerServiceLog
@CustomerId INT,
@ServiceLogId BIGINT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1
		@ServiceLogID = Id
	FROM
		MP_ServiceLog l
	WHERE
		l.CustomerId = @CustomerId
		AND
		l.DirectorId IS NULL
		AND
		l.ServiceType = 'Consumer Request'
	ORDER BY
		l.InsertDate DESC,
		l.Id DESC
		
	IF @ServiceLogId IS NULL
	BEGIN
		SELECT TOP 1
		   @ServiceLogID = l.Id
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
		   AND
			l.DirectorId IS NULL
		  ORDER BY
		   l.InsertDate DESC,
		   l.Id DESC
	END
END
GO
