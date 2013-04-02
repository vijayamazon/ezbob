CREATE OR REPLACE procedure DeleteSignalById
( 
    pId in Number
)
as
begin
    delete from Signal
    where Id = pId;
end DeleteSignalById;
/
