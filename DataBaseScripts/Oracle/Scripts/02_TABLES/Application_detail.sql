CREATE TABLE Application_Detail (
       DetailId             NUMBER NOT NULL,
       ApplicationId        NUMBER NOT NULL,
       DetailNameId         NUMBER NULL,
       ParentDetailId       NUMBER NULL,
       ValueStr             CLOB NULL,
       ValueNum             NUMBER NULL,
       ValueDateTime        DATE NULL,
       IsBinary             SMALLINT NULL
);

COMMENT ON TABLE Application_Detail IS 'Application details. Additional field for applications represented in Runtime_App table.';
COMMENT ON COLUMN Application_Detail.DetailId IS 'Primary key';
COMMENT ON COLUMN Application_Detail.ApplicationId IS 'Owner application id';
COMMENT ON COLUMN Application_Detail.DetailNameId IS 'Application detail item name identifier';
COMMENT ON COLUMN Application_Detail.ParentDetailId IS 'Parent application detail row. It is used for storing data in tree manner';
COMMENT ON COLUMN Application_Detail.ValueStr IS 'String value';
COMMENT ON COLUMN Application_Detail.ValueNum IS 'Numeric value';
COMMENT ON COLUMN Application_Detail.ValueDateTime IS 'DateTime value';
COMMENT ON COLUMN Application_Detail.IsBinary IS '0 (or Null) - this record does not have a corresponded record in Application_Attathment table.
1 - have a corresponded record.';
CREATE UNIQUE INDEX PK_Application_Detail ON Application_Detail
(
       DetailId                       ASC
);


ALTER TABLE Application_Detail
       ADD  ( CONSTRAINT PK_Application_Detail PRIMARY KEY (DetailId) ) ;


create index I_APP_DETAIL_1 on APPLICATION_DETAIL (APPLICATIONID) ;
create index I_APP_DETAIL_2 on APPLICATION_DETAIL (DETAILNAMEID) ;
create index I_APP_DETAIL_3 on APPLICATION_DETAIL (PARENTDETAILID) ;