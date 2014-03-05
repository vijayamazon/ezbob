IF OBJECT_ID (N'dbo.vw_CollectionFinal') IS NOT NULL
	DROP VIEW dbo.vw_CollectionFinal
GO

CREATE VIEW [dbo].[vw_CollectionFinal]
AS
SELECT DISTINCT CustomerId, MIN(StartDate) AS StartDate
FROM         (SELECT     loanID, CustomerId, StartDate, DateClose, MaxDelinquencyDays, RepaymentPeriod, CurrentBalance, Gender, FirstName, MiddleInitial, 
                                              Surname, RefNumber, Line1, Line2, Line3, Town, County, Postcode, DateOfBirth, LoanAmount,
                                              CompanyType, LimitedRefNum, NonLimitedRefNum, CustomerState, SortCode
                       FROM          dbo.vw_NotClose
                       WHERE      (loanID IN
                                                  (SELECT DISTINCT nc.loanID
                                                    FROM          dbo.vw_NotClose AS nc INNER JOIN
                                                                           dbo.vw_collection AS vc ON nc.DateClose = vc.DateClosed AND nc.MaxDelinquencyDays = vc.MaxDelinquencyDays))) 
                      AS a
GROUP BY CustomerId

GO

