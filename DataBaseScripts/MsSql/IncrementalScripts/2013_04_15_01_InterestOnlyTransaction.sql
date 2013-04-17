ALTER TABLE dbo.LoanTransaction ADD
    InterestOnly bit NULL
GO
ALTER TABLE dbo.LoanTransaction SET (LOCK_ESCALATION = TABLE)
GO