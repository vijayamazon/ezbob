using System;
using System.Collections;
using System.Xml.Serialization;

namespace EzBob.CommonLib
{
	public static class SerializerCache
	{
		private static readonly Hashtable hash = new Hashtable();
		public static XmlSerializer GetSerializer( Type type )
		{
			XmlSerializer res = null;
			lock ( hash )
			{
				res = hash[type.FullName] as XmlSerializer;
				if ( res == null )
				{
					res = new XmlSerializer( type );
					hash[type.FullName] = res;
				}
			}
			return res;
		}
	}
}