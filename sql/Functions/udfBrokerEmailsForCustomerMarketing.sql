IF OBJECT_ID('dbo.udfBrokerEmailsForCustomerMarketing') IS NOT NULL
	DROP FUNCTION dbo.udfBrokerEmailsForCustomerMarketing
GO

CREATE FUNCTION dbo.udfBrokerEmailsForCustomerMarketing(@tblCustomerIDs IntList READONLY)
RETURNS @out TABLE (
	CustomerID INT NOT NULL,
	BrokerEmail NVARCHAR(255) NOT NULL,
	HasLoan INT NOT NULL,
	SendEmailOnCreate INT NULL
)
AS
BEGIN
	DECLARE @CustomersWithLoan IntList

	INSERT INTO @CustomersWithLoan (Value)
	SELECT DISTINCT
		CustomerId
	FROM
		Loan

	------------------------------------------------------------------------------

	INSERT INTO @out(CustomerID, BrokerEmail, HasLoan, SendEmailOnCreate)
	SELECT
		c.Value,
		ISNULL(b.ContactEmail, ''),
		ISNULL(l.Value, 0),
		ISNULL(m.SendEmailOnCreate, 0)
	FROM
		@tblCustomerIDs c
		INNER JOIN Customer cu ON c.Value = cu.Id
		LEFT JOIN BrokerLeads bl ON c.Value = bl.CustomerID
		LEFT JOIN BrokerLeadAddModes m ON bl.BrokerLeadAddModeID = m.BrokerLeadAddModeID
		LEFT JOIN @CustomersWithLoan l ON c.Value = l.Value
		LEFT JOIN Broker b ON cu.BrokerID = b.BrokerID

	------------------------------------------------------------------------------

	UPDATE @out SET
		BrokerEmail = ''
	WHERE
		HasLoan != 0
		OR
		SendEmailOnCreate != 1

	------------------------------------------------------------------------------

	RETURN
END
GO
