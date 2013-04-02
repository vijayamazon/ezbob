CREATE TABLE AppAdditionalData (
       AppId					NUMBER NOT NULL,
       Name						VARCHAR2(256),
       PassportSeries			VARCHAR2(256),
       Patronymic				VARCHAR2(1024),
       StatusId 				NUMBER NULL,
       Surname					VARCHAR2(1024),
       CreditProduct 			VARCHAR2(1024),
	   DesiredCreditSum			NUMBER NULL,
	   ActualCreditSum			NUMBER NULL,
	   ReadOnlyNodeName			VARCHAR2(1024) NULL,
	   AutoCreditTerm 			VARCHAR2(1024) NULL,
	   AutoCreditFirstPayment	VARCHAR2(1024) NULL,
       DecisionStatus 			VARCHAR2(1024) NULL,
	   CommandsList				CHAR(32) NULL
);