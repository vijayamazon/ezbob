﻿namespace Ezbob.Utils.Extensions {
	using System.IO;

	public static class StrExt {
		public static Stream ToStream(this string str) {
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(str);
			writer.Flush();
			stream.Position = 0;
			return stream;
		} 
	}
}