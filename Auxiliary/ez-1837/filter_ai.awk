{
	if (match($0, /using System.Reflection;/) ||
		match($0, /using System.Runtime.InteropServices;/) ||
		match($0, /^\[assembly: AssemblyTitle/) ||
		match($0, /^\[assembly: AssemblyDescription/) ||
		match($0, /^\[assembly: Guid/))
		printf "%s\n", $0;
}

