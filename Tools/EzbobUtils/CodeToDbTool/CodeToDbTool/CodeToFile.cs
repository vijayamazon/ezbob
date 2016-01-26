
namespace CodeToDbTool {
    using System.IO;
    using Ezbob.Utils.dbutils;

    public static class CodeToFile {
        public static void SaveSp<T>() where T : class
        {
            using (StreamWriter sw = new StreamWriter(Folder + typeof(T).Name + "Save.sql", true)) {
                sw.WriteLine(CodeToSql.GetCreateSp<T>());
            }
        }

		public static void SaveTable<T>() where T : class {
			using (StreamWriter sw = new StreamWriter(Folder + typeof(T).Name + "Table.sql", true)) {
				sw.WriteLine(CodeToSql.GetCreateTable<T>());
			}
		}

        public static string Folder { get; set; }
    }
}
