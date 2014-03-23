using System;
using System.Runtime.Serialization;
namespace ApplicationMng.Model.Commands
{
	public static class SerializtionHelper
	{
		public static string TryGetString(this System.Runtime.Serialization.SerializationInfo info, string name)
		{
			string result;
			try
			{
				result = info.GetString(name);
			}
			catch (System.Exception var_0_0C)
			{
				result = null;
			}
			return result;
		}
	}
}
