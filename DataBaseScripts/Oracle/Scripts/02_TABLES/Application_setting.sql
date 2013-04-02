CREATE TABLE Application_Setting (
       Param1DetailNameId   NUMBER NULL,
       Param2DetailNameId   NUMBER NULL,
       Param3DetailNameId   NUMBER NULL,
       Param4DetailNameId   NUMBER NULL,
       Param5DetailNameId   NUMBER NULL,
       Param6DetailNameId   NUMBER NULL,
       Param7DetailNameId   NUMBER NULL,
       Param8DetailNameId   NUMBER NULL,
       Param9DetailNameId   NUMBER NULL
);

COMMENT ON TABLE Application_Setting IS 'Allows configure what application detail item put in the Param1..Param9 parameters in the Runtime_App table';
COMMENT ON COLUMN Application_Setting.Param1DetailNameId IS 'Maps param field to Application Detail Name';
COMMENT ON COLUMN Application_Setting.Param2DetailNameId IS 'Maps param field to Application Detail Name';
COMMENT ON COLUMN Application_Setting.Param3DetailNameId IS 'Maps param field to Application Detail Name';
COMMENT ON COLUMN Application_Setting.Param4DetailNameId IS 'Maps param field to Application Detail Name';
COMMENT ON COLUMN Application_Setting.Param5DetailNameId IS 'Maps param field to Application Detail Name';
COMMENT ON COLUMN Application_Setting.Param6DetailNameId IS 'Maps param field to Application Detail Name';
COMMENT ON COLUMN Application_Setting.Param7DetailNameId IS 'Maps param field to Application Detail Name';
COMMENT ON COLUMN Application_Setting.Param8DetailNameId IS 'Maps param field to Application Detail Name';
COMMENT ON COLUMN Application_Setting.Param9DetailNameId IS 'Maps param field to Application Detail Name';

