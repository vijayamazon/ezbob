
IF TYPE_ID('BigintList') IS  NULL
CREATE TYPE [dbo].[BigintList] AS TABLE(
    [Item] BIGINT NOT NULL
)
GO
 
IF OBJECT_ID('NL_ResetPaidAmountsAndStatuses') IS NULL
    EXECUTE('CREATE PROCEDURE NL_ResetPaidAmountsAndStatuses AS SELECT 1')
GO
 
ALTER PROCEDURE [dbo].[NL_ResetPaidAmountsAndStatuses]                                
                @PaymentDate DATETIME, 
                @LoanID bigint,
                @PaymentDateInclude bit = 0
AS
BEGIN
                SET NOCOUNT ON;
                                
                DECLARE @Paymentids [dbo].[BigintList];
 
                if  @PaymentDateInclude = 0 
                    insert into @Paymentids select PaymentID from [dbo].[NL_Payments] where LoanID = @LoanID and [PaymentTime] > @PaymentDate;
                ELSE IF @PaymentDateInclude = 1 
                    insert into @Paymentids select PaymentID from [dbo].[NL_Payments] where LoanID = @LoanID and [PaymentTime] >= @PaymentDate;
                
                IF (select COUNT(Item) from @Paymentids) = 0 
                                RETURN 0;
                
                -- RESET PAID PRINCIPAL, INTEREST (SCHEDULE), FEES PAID AFTER PaymentDate of deleted/retroactive payment              
                UPDATE [NL_LoanSchedulePayments] 
                SET 
					[ResetPrincipalPaid] = [PrincipalPaid], 
					[ResetInterestPaid] = [InterestPaid],
					[PrincipalPaid] = 0, 
					[InterestPaid] = 0 
                WHERE [PaymentID] in (select Item from @Paymentids);
                
                UPDATE [NL_LoanFeePayments] SET [ResetAmount] = [Amount], [Amount] = 0 WHERE [PaymentID] in (select Item from @Paymentids);            
 
                -- reset schedules statuses and closed time
                UPDATE  s
                    SET s.ClosedTime = null, s.LoanScheduleStatusID = (select st.LoanScheduleStatusID from [dbo].[NL_LoanScheduleStatuses] st where st.LoanScheduleStatus = 'StillToPay')
                FROM
                    [dbo].[NL_LoanSchedules] s
                INNER JOIN
                    [dbo].[NL_LoanSchedulePayments] sp ON s.[LoanScheduleID] = sp.[LoanScheduleID]
                WHERE sp.[PaymentID] in (select Item from @Paymentids);
                                                                                
 
                -- loan status
                UPDATE [dbo].[NL_Loans] SET [DateClosed] = null, [LoanStatusID] = (select ls.LoanStatusID from [dbo].[NL_LoanStatuses] ls where [LoanStatus] ='Live') WHERE [LoanID] = @LoanID
 
END
