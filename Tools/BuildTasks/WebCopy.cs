namespace BuildTasks
{
	public class WebCopy : MultiCopy
	{
		public override bool Execute()
		{
			ExcludeFiles = new []
			{
				".csproj",
				".psproj",
				".cs"
			};

			ExcludeDirs = new []
			{
				".svn",
				"obj",
				"Web References",
				"bin",
				"Properties"
			};

			base.Execute();

			ExcludeFiles = new []
			{
				".pdb",
				".xml"
			};

			ExcludeDirs = null;
			CopyFrom += "bin\\";
			CopyTo += "bin\\";
			base.Execute();

			return true;
		}
	}
}