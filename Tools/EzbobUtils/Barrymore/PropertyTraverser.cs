namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	#region class TraversableAttribute

	[System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
	public class TraversableAttribute : Attribute { } // TraversableAttribute

	#endregion class TraversableAttribute

	#region class NonTraversableAttribute

	[System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
	public class NonTraversableAttribute : Attribute { } // NonTraversableAttribute

	#endregion class NonTraversableAttribute

	#region class PropertyTraverser

	public static class PropertyTraverser {
		#region public

		public static void Traverse(Type oType, Action<object, PropertyInfo> oCallback) {
			if (oType == null)
				throw new ArgumentNullException("oType", "Type to traverse not specified.");

			if (oCallback == null)
				throw new ArgumentNullException("oCallback", "Property callback not specified.");

			Traverse(null, oType, oCallback);
		} // Traverse

		public static void Traverse<T>(Action<object, PropertyInfo> oCallback) {
			if (oCallback == null)
				throw new ArgumentNullException("oCallback", "Property callback not specified.");

			Traverse(null, typeof(T), oCallback);
		} // Traverse

		public static void Traverse(this object oInstance, Action<object, PropertyInfo> oCallback) {
			if (oInstance == null)
				throw new ArgumentNullException("oInstance", "Object to traverse not specified.");

			if (oCallback == null)
				throw new ArgumentNullException("oCallback", "Property callback not specified.");

			Traverse(oInstance, oInstance.GetType(), oCallback);
		} // Traverse

		#endregion public

		#region private

		private static void Traverse(object oInstance, Type oRealType, Action<object, PropertyInfo> oCallback) {
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

				if (oAttrList.Length == 0)
					oCallback(oInstance, pi);
			} // foreach
		} // Traverse

		#endregion private
	} // class PropertyTraverser

	#endregion class PropertyTraverser

	#region class TraversableExt

	public static class TraversableExt {
		public static object[] ToObjectArray(this object oSrc) {
			if (oSrc == null)
				return new object[0];

			var oResult = new List<object>();

			oSrc.Traverse((oInstance, oPropertyInfo) => oResult.Add(oPropertyInfo.GetValue(oInstance)));

			return oResult.ToArray();
		} // ToObjectArray
	} // class TraversableExt

	#endregion class TraversableExt
} // namespace
