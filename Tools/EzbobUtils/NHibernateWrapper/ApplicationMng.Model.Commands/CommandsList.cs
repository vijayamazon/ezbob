using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
namespace ApplicationMng.Model.Commands
{
	[System.Serializable]
	public class CommandsList : System.Runtime.Serialization.ISerializable
	{
		private System.Collections.Generic.IList<CommandBase> _commands = new System.Collections.Generic.List<CommandBase>();
		private CommandBase[] _array;
		public virtual System.Collections.Generic.IList<CommandBase> Commands
		{
			get
			{
				return this._commands;
			}
			set
			{
				this._commands = value;
			}
		}
		public virtual string Id
		{
			get;
			set;
		}
		public virtual User User
		{
			get;
			set;
		}
		public virtual SecurityApplication SecApp
		{
			get;
			set;
		}
		public virtual Application App
		{
			get;
			set;
		}
		public virtual string Description
		{
			get;
			set;
		}
		public virtual System.DateTime? CreatedOn
		{
			get;
			set;
		}
		public virtual void AddCmd(CommandBase cmd)
		{
			this._commands.Add(cmd);
		}
		public CommandsList()
		{
		}
		public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("Id", this.Id);
			info.AddValue("Description", this.Description);
			info.AddValue("User", (this.User == null) ? -1 : this.User.Id);
			info.AddValue("SecApp", (this.SecApp == null) ? -1 : this.SecApp.Id);
			CommandBase[] array = new CommandBase[this._commands.Count];
			this.Commands.CopyTo(array, 0);
			info.AddValue("Commands", array);
		}
		protected CommandsList(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			ISerializationHelper serializationHelper = (ISerializationHelper)context.Context;
			this.SecApp = serializationHelper.GetSecApp(info.GetInt32("SecApp"));
			this.User = serializationHelper.GetUser(info.GetInt32("User"));
			this.Description = info.GetString("Description");
			this._array = (info.GetValue("Commands", typeof(CommandBase[])) as CommandBase[]);
		}
		public virtual void ConvertArrayToList()
		{
			if (this._array != null)
			{
				CommandBase[] array = this._array;
				for (int i = 0; i < array.Length; i++)
				{
					CommandBase cmd = array[i];
					this.AddCmd(cmd);
				}
			}
		}
		public virtual string Serialize()
		{
			string result;
			using (System.IO.Stream stream = new System.IO.MemoryStream())
			{
				SoapFormatter soapFormatter = new SoapFormatter();
				soapFormatter.Serialize(stream, this);
				stream.Flush();
				stream.Seek(0L, System.IO.SeekOrigin.Begin);
				using (System.IO.StreamReader streamReader = new System.IO.StreamReader(stream))
				{
					result = streamReader.ReadToEnd();
				}
			}
			return result;
		}
		public virtual void Serialize(System.IO.Stream stream)
		{
			SoapFormatter soapFormatter = new SoapFormatter();
			soapFormatter.Serialize(stream, this);
		}
	}
}
