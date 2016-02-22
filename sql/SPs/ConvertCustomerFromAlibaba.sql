SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ConvertCustomerFromAlibaba') IS NULL
	EXECUTE('CREATE PROCEDURE ConvertCustomerFromAlibaba AS SELECT 1')
GO

ALTER PROCEDURE ConvertCustomerFromAlibaba
@CustomerID INT,
@UserName NVARCHAR(100),
@Origin NVARCHAR(20)
AS
BEGIN
	IF EXISTS (SELECT * FROM Customer WHERE Id = @CustomerID AND AlibabaID IS NOT NULL AND IsAlibaba = 1)
	BEGIN
		BEGIN TRANSACTION

		INSERT INTO CustomerRelations (CustomerId, UserName, Type, ActionId, StatusId, Comment, Timestamp, RankId, IsBroker, PhoneNumber)
		SELECT
			c.Id,
			@UserName,
			'Internal',
			a.Id,
			s.Id,
			'The customer was converted from Alibaba (Alibaba ID: ' + c.AlibabaId + ') to ' + @Origin + '.',
			GETUTCDATE(),
			NULL,
			0,
			NULL
		FROM
			Customer c
			INNER JOIN CRMStatuses s ON s.Name = 'Note for underwriting'
			INNER JOIN CRMActions a ON a.Name = 'Note'
		WHERE
			c.Id = @CustomerID

		UPDATE Customer SET
			IsAlibaba = 0,
			AlibabaID = NULL,
			OriginID = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name = @Origin)
		WHERE
			Id = @CustomerID

		UPDATE Security_User SET
			OriginID = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name = @Origin)
		WHERE
			UserId = @CustomerID

		COMMIT TRANSACTION
	END

	SELECT
		'Current customer state',
		c.Id,
		c.AlibabaId,
		c.IsAlibaba,
		c.OriginID
	FROM
		Customer c
	WHERE
		c.Id = @CustomerID
END
GO
