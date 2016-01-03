using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("EzBob3dParties")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("EzBob3dParties")]
[assembly: AssemblyCopyright("Copyright © Microsoft 2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("32b0cdfc-c49d-4011-86a2-593e8fb17db5")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: InternalsVisibleTo("EzBob3dPartiesTests")]
[assembly: InternalsVisibleTo("EzBobShared")]

// Why "DynamicProxyGenAssembly2" and not "MOQ"? It's the name of dynamic assembly created to contain dynamically 
// generated proxy types (all of this is handled by yet another library, Castle's DynamicProxy) which is used by Moq. 
// So you expose types to dynamic proxy assembly, not to Moq itself.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]