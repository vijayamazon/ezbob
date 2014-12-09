namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	[System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
	public class TraversableAttribute : Attribute { } // TraversableAttribute

	[System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
	public class NonTraversableAttribute : Attribute { } // NonTraversableAttribute

	public static class PropertyTraverser {

		public static void Traverse(Type oType, Action<object, PropertyInfo> oCallback) {
			if (oType == null)
				throw new ArgumentNullException("oType", "Type to traverse not specified.");

			if (oCallback == null)
				throw new ArgumentNullException("oCallback", "Traversing callback not specified for type " + oType + ".");

			Traverse(null, oType, oCallback);
		} // Traverse

		public static void Traverse<T>(Action<object, PropertyInfo> oCallback) {
			if (oCallback == null)
				throw new ArgumentNullException("oCallback", "Traversing callback not specified for type " + typeof(T) + ".");

			Traverse(null, typeof(T), oCallback);
		} // Traverse

		public static void Traverse(this object oInstance, Action<object, PropertyInfo> oCallback) {
			if (oInstance == null)
				throw new ArgumentNullException("oInstance", "Object to traverse not specified.");

			if (oCallback == null)
				throw new ArgumentNullException("oCallback", "Traversing callback not specified for type " + oInstance.GetType() + ".");

			Traverse(oInstance, oInstance.GetType(), oCallback);
		} // Traverse

		public static IEnumerable<PropertyInfo> EnumerateProperties(Type oType) {
			if (oType == null)
				throw new ArgumentNullException("oType", "Type to traverse not specified.");

			return Traverse(null, oType, null);
		} // EnumerateProperties

		public static IEnumerable<PropertyInfo> EnumerateProperties<T>() {
			return Traverse(null, typeof(T), null);
		} // EnumerateProperties

		public static IEnumerable<PropertyInfo> EnumerateProperties(this object oInstance) {
			if (oInstance == null)
				throw new ArgumentNullException("oInstance", "Object to traverse not specified.");

			return Traverse(oInstance, oInstance.GetType(), null);
		} // EnumerateProperties

		private static IEnumerable<PropertyInfo> Traverse(object oInstance, Type oRealType, Action<object, PropertyInfo> oCallback) {
			List<PropertyInfo> oResult = (oCallback == null) ? new List<PropertyInfo>() : null;

			PropertyInfo[] oPropertyList = oRealType
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.GetSetMethod() != null)
				.ToArray();

			var oSelected = new List<PropertyInfo>();

			foreach (PropertyInfo pi in oPropertyList) {
				object[] oAttrList = pi.GetCustomAttributes(typeof(TraversableAttribute), false);

				if (oAttrList.Length > 0)
					oSelected.Add(pi);
			} // foreach

			if (oSelected.Count == 0)
				oSelected.AddRange(oPropertyList);

			foreach (PropertyInfo pi in oSelected) {
				object[] oAttrList = pi.GetCustomAttributes(typeof(NonTraversableAttribute), false);

				if (oAttrList.Length == 0) {
					if (oCallback == null)
						oResult.Add(pi);
					else
						oCallback(oInstance, pi);
				} // if
			} // foreach

			return oResult;
		} // Traverse

	} // class PropertyTraverser

	public static class TraversableExt {
		public static object[] ToObjectArray(this object oSrc) {
			if (oSrc == null)
				return new object[0];

			var oResult = new List<object>();

			oSrc.Traverse((oInstance, oPropertyInfo) => oResult.Add(oPropertyInfo.GetValue(oInstance)));

			return oResult.ToArray();
		} // ToObjectArray
	} // class TraversableExt

} // namespace
