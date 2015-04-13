using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Ezbob.Integration.CallCreditLib {
	class XSerializer {
		private static object _locker = new object();
		private static Dictionary<Type, XmlSerializer> _serializeCache = new Dictionary<Type, XmlSerializer>();

		public static T Deserialize<T>(XElement element) {
			var t = typeof(T);
			var ret = (T)Activator.CreateInstance(t);
			var cache = t.GetProperties().ToDictionary(pi => pi.Name.ToLowerInvariant());

			foreach (var el in element.Elements()) {
				if (!cache.ContainsKey(el.Name.LocalName.ToLowerInvariant()))
					continue;

				var val = el.Value;
				var pi = cache[el.Name.LocalName.ToLowerInvariant()];
				pi.SetValue(ret, Convert.ChangeType(val, pi.PropertyType), null);
			}
			return ret;
		}

		//-----------------------------------------------------------------------------------
		public static string Serialize<T>(T input) where T : class {
			if (input == null)
				return null;
			var t = typeof(T);
			lock (_locker) {
				if (!_serializeCache.ContainsKey(t)) {
					_serializeCache.Add(t, new XmlSerializer(t));
				}
			}
			using (var wr = new StringWriter()) {
				_serializeCache[t].Serialize(wr, input);
				return wr.ToString();
			}
		}
	}
}
