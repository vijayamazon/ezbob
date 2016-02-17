packages: (if installing from NuGet)
---------------------------

MicrosofMicrosoft.Owin.SelfHost
Microsoft.Owin.Security.OAuth
Nancy.MSOwinSecurity
Nancy.Owin
Nancy.Bootstrappers.StructureMap
Nancy.Serialization.JsonNet
Nancy.Metadata.Modules

-------------------------
Nuget Command example:
-------------------------
Update-Package -ProjectName 'EzBobRest' -Reinstall 'log4net'


---------------------------------------- UPLOAD SCRIPT EXAMPLE --------------------------
cd C:\Program Files (x86)\Git\bin\
curl -X POST -F "file1=@cat.exe" -F "file2=@cmp.exe" http://localhost:12345/api/v1/marketplace/hmrc/upload/vat/11111
-----------------------------------------------------------------------------------------

--------------------------- NETSSH -----------------------------------------------------------------------------------
netsh http show sslcert :lists SSL server certificate bindings and corresponding client certificate policies for an IP address and port. 