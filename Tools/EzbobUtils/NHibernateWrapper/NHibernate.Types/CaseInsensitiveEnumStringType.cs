using NHibernate;
using NHibernate.Type;
using System;
namespace NHibernateWrapper.NHibernate.Types
{
	public class CaseInsensitiveEnumStringType<T> : EnumStringType<T>
	{
		public override object GetInstance(object value)
		{
			if (value == null)
			{
				throw new System.ArgumentNullException("value");
			}
			object result;
			try
			{
				result = (T)((object)System.Enum.Parse(typeof(T), (string)value, true));
			}
			catch (System.ArgumentException innerException)
			{
				throw new HibernateException(string.Format("Can't Parse {0} as {1}", value, this.ReturnedClass.Name), innerException);
			}
			return result;
		}
	}
}
