IF EXISTS (SELECT 1 FROM sys.objects WHERE [type] = 'TR' AND [name] = 'TR_LoanTransactionMethod')
	DROP TRIGGER TR_LoanTransactionMethod
GO

CREATE TRIGGER TR_LoanTransactionMethod
ON LoanTransaction
FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON

	------------------------------------------------------------------------------

	UPDATE LoanTransaction SET
		LoanTransactionMethodId = m.Id
	FROM
		inserted i,
		LoanTransaction t,
		LoanTransactionMethod m
	WHERE
		i.Id = t.Id
		AND
		t.Type = 'PacnetTransaction'
		AND
		m.Name = 'Pacnet'

	------------------------------------------------------------------------------

	UPDATE LoanTransaction SET
		LoanTransactionMethodId = m.Id
	FROM
		inserted i,
		LoanTransaction t,
		LoanTransactionMethod m
	WHERE
		i.Id = t.Id
		AND
		t.Type = 'PaypointTransaction'
		AND
		t.PaypointId IS NOT NULL
		AND
		t.PaypointId NOT LIKE '--- manual ---'
		AND
		m.Name = 'Auto'

	------------------------------------------------------------------------------

	UPDATE LoanTransaction SET
		LoanTransactionMethodId = ISNULL(m.Id, 0)
	FROM
		inserted i,
		LoanTransaction t,
		LoanTransactionMethod m
	WHERE
		i.Id = t.Id
		AND
		t.Type = 'PaypointTransaction'
		AND
		t.PaypointId IS NOT NULL
		AND
		t.PaypointId LIKE '--- manual ---'
		AND
		m.Name = dbo.udfPaymentMethod(t.Description)

	------------------------------------------------------------------------------

	UPDATE LoanTransaction SET
		LoanTransactionMethodId = m.Id
	FROM
		inserted i,
		LoanTransaction t,
		LoanTransactionMethod m
	WHERE
		i.Id = t.Id
		AND
		t.Type = 'PaypointTransaction'
		AND
		t.PaypointId IS NOT NULL
		AND
		t.PaypointId LIKE '--- manual ---'
		AND
		t.LoanTransactionMethodId = 0
		AND
		m.Name = 'Manual'

	------------------------------------------------------------------------------

	SET NOCOUNT OFF
END

GO

