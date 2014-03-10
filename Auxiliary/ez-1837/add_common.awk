{
	printf "%s\n", $0;

	path = "";

	for (i = 0; i < nLevel; i++)
		path = (path "..\\");

	if (match($0, /^\s+<Compile Include="Properties\\AssemblyInfo.cs"/))
		printf "<Compile Include=\"%sCommon\\CommonAssemblyInfo.cs\"><Link>Properties\\CommonAssemblyInfo.cs</Link></Compile>\n", path;
}

