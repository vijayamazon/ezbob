IF OBJECT_ID ('dbo.CustomerRequestedLoan') IS NOT NULL
	DROP TABLE dbo.CustomerRequestedLoan
GO

CREATE TABLE dbo.CustomerRequestedLoan
	(
	  CustomerId                                                    INT NOT NULL
	, Amount                                                        DECIMAL(18)
	, ReasonId                                                      INT
	, OtherReason                                                   NVARCHAR(300)   
	, SourceOfRepaymentId                                           INT
	, OtherSourceOfRepayment                                        NVARCHAR(300)
	, CONSTRAINT PK_CustomerRequestedLoan                           PRIMARY KEY (CustomerId)
    , CONSTRAINT FK_CustomerRequestedLoan_Customer                  FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id)	
    , CONSTRAINT FK_CustomerRequestedLoan_CustomerReason            FOREIGN KEY (ReasonId) REFERENCES dbo.CustomerReason (Id)	
    , CONSTRAINT FK_CustomerRequestedLoan_CustomerSourceOfRepayment FOREIGN KEY (SourceOfRepaymentId) REFERENCES dbo.CustomerSourceOfRepayment (Id)	
	)
GO
