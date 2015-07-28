IF OBJECT_ID('NL_PaymentsGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_PaymentsGet AS SELECT 1')
GO

ALTER PROCEDURE NL_PaymentsGet
@LoanID INT
AS
BEGIN
	SET NOCOUNT ON;
SELECT 
-- history
--h.LoanHistoryID,
--h.EventTime,
--h.Amount,
--h.RepaymentCount,
--h.InterestRate,

-- schedules
--lsch.LoanScheduleStatusID,
--lsch.PlannedDate,
--lsch.ClosedTime,
--lsch.Position,
--lsch.Principal,
--lsch.InterestRate,
--lsch.LoanHistoryID,

-- scheduled payments
--sp.LoanSchedulePaymentID,
--sp.LoanScheduleID,
--sp.PaymentID,
sp.PrincipalPaid AS [Principal],
sp.InterestPaid AS [Interest],

-- payments
--p.PaymentID,
--p.PaymentMethodID,
--p.Amount,
p.PaymentTime AS [Time],
--p.CreationTime,

-- fee payments
--fp.LoanFeePaymentID,
--fp.LoanFeeID,
--fp.PaymentID,
fp.Amount AS [Fees]

FROM [dbo].[NL_LoanHistory] h 
INNER JOIN [dbo].[NL_LoanSchedules] lsch ON lsch.LoanHistoryID=h.LoanHistoryID
INNER JOIN [dbo].[NL_LoanSchedulePayments] sp ON lsch.LoanScheduleID = sp.LoanScheduleID
INNER JOIN [dbo].[NL_Payments] p ON sp.PaymentID = p.PaymentID AND p.IsActive = 1 AND p.DeletionTime IS NULL
LEFT JOIN [dbo].[NL_PaypointTransactions] ppt ON p.PaymentID = ppt.PaymentID 
INNER JOIN [dbo].[NL_PaypointTransactionStatuses] ppts ON ppts.PaypointTransactionStatusID = ppt.PaypointTransactionStatusID AND ppts.TransactionStatus = 'Done'
INNER JOIN [dbo].[NL_LoanFeePayments] fp ON fp.PaymentID= p.PaymentID
INNER JOIN [dbo].[NL_LoanFees] f ON f.LoanFeeID = fp.LoanFeeID
WHERE
h.LoanID = @LoanID;

END
GO
