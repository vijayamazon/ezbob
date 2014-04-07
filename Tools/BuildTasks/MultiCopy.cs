namespace BuildTasks
{
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	public class MultiCopy : Task
	{
		private string[] filters;
		public string[] ExcludeDirs
		{
			get;
			set;
		}
		public string[] ExcludeFiles
		{
			get;
			set;
		}
		public string Filters
		{
			get;
			set;
		}
		[Required]
		public string CopyFrom
		{
			get;
			set;
		}
		[Required]
		public string CopyTo
		{
			get;
			set;
		}
		public MultiCopy()
		{
			this.ExcludeDirs = new string[]
			{
				".svn"
			};
			this.ExcludeFiles = new string[]
			{
				".pdb"
			};
		}
		public override bool Execute()
		{
			base.Log.LogMessage(MessageImportance.High, "Copy from: {0} to {1}", new object[]
			{
				this.CopyFrom,
				this.CopyTo
			});
			bool result;
			if (!System.IO.Directory.Exists(this.CopyFrom))
			{
				base.Log.LogWarning("CopyFrom directory {0} not exists!", new object[]
				{
					this.CopyFrom
				});
				result = true;
			}
			else
			{
				if (!string.IsNullOrEmpty(this.Filters))
				{
					this.filters = this.Filters.Split(new char[]
					{
						';'
					});
				}
				System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(this.CopyFrom);
				System.IO.FileInfo[] files = directoryInfo.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
				System.IO.FileInfo[] array = files;
				int i = 0;
				while (i < array.Length)
				{
					System.IO.FileInfo fileInfo = array[i];
					if (this.ExcludeDirs == null)
					{
						goto IL_142;
					}
					bool flag = false;
					string[] array2 = this.ExcludeDirs;
					for (int j = 0; j < array2.Length; j++)
					{
						string str = array2[j];
						if (fileInfo.DirectoryName.Contains("\\" + str))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						goto IL_142;
					}
					IL_22B:
					i++;
					continue;
					IL_142:
					if (this.ExcludeFiles != null)
					{
						flag = false;
						array2 = this.ExcludeFiles;
						for (int j = 0; j < array2.Length; j++)
						{
							string b = array2[j];
							if (fileInfo.Extension == b)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							goto IL_22B;
						}
					}
					string text = this.CopyTo + fileInfo.FullName.Replace(this.CopyFrom, string.Empty);
					if (this.filters != null)
					{
						if (!this.CheckFileForFilters(fileInfo))
						{
							goto IL_22B;
						}
						text = System.Text.RegularExpressions.Regex.Replace(text, "*", string.Empty);
					}
					string directoryName = System.IO.Path.GetDirectoryName(text);
					if (!System.IO.Directory.Exists(directoryName))
					{
						System.IO.Directory.CreateDirectory(directoryName);
					}
					fileInfo.CopyTo(text, true);
					goto IL_22B;
				}
				result = true;
			}
			return result;
		}
		private bool CheckFileForFilters(System.IO.FileInfo fileInfo)
		{
			bool result;
			if (fileInfo.Extension == ".dll" || fileInfo.Extension == ".exe")
			{
				System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(fileInfo.FullName);
				object[] customAttributes = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false);
				if (customAttributes == null)
				{
					result = true;
				}
				else
				{
					string[] filters = this.filters;
					for (int i = 0; i < filters.Length; i++)
					{
						string b = filters[i];
						bool flag = false;
						object[] array = customAttributes;
						for (int j = 0; j < array.Length; j++)
						{
							object obj = array[j];
							if (((System.Reflection.AssemblyDescriptionAttribute)obj).Description == b)
							{
								flag = true;
							}
						}
						if (!flag)
						{
							result = false;
							return result;
						}
					}
					result = true;
				}
			}
			else
			{
				if (!System.Text.RegularExpressions.Regex.IsMatch(fileInfo.Name, "*_[*]*"))
				{
					result = true;
				}
				else
				{
					string str = string.Concat(this.filters);
					result = System.Text.RegularExpressions.Regex.IsMatch(fileInfo.Name, "*_[" + str + "]*");
				}
			}
			return result;
		}
	}
}
