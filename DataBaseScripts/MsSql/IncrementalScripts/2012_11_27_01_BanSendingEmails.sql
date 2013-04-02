ALTER TABLE dbo.CashRequests ADD
    EmailSendingBanned bit NULL
GO

ALTER TABLE dbo.Customer
    DROP COLUMN EmailSendingBanned
GO