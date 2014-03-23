using NHibernate;
using StructureMap;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
namespace ApplicationMng.Model.Commands
{
	[System.Serializable]
	public class CommandsListSerializationWrapper
	{
		public static CommandsList Deserialize(string xml, ISession session)
		{
			System.IO.Stream ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
			return CommandsListSerializationWrapper.Deserialize(ms);
		}
		public static CommandsList Deserialize(byte[] buf)
		{
			System.IO.Stream ms = new System.IO.MemoryStream(buf);
			return CommandsListSerializationWrapper.Deserialize(ms);
		}
		public static CommandsList Deserialize(System.IO.Stream ms)
		{
			NHCommandsSerializationHelper instance = ObjectFactory.GetInstance<NHCommandsSerializationHelper>();
			CommandsList commandsList = new SoapFormatter
			{
				Context = new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.File, instance)
			}.Deserialize(ms) as CommandsList;
			commandsList.ConvertArrayToList();
			return commandsList;
		}
	}
}
