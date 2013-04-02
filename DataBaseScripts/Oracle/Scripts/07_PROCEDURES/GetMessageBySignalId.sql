create or replace procedure GetMessageBySignalId
(
    pApplicationId in Number,
    pMessage OUT Blob
)
as
 l_message Blob;
begin
  begin
    select Message
    into l_message
    from Signal
    where Label like '%_' || pApplicationId
          and Applicationid = pApplicationId;

  exception
    when no_data_found
    then l_message := null;
  end;
  pMessage := l_message;
end GetMessageBySignalId;
/
