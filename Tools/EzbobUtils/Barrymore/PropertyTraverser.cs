namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	#region class TraversableAttribute

	[System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
	public class TraversableAttribute : Attribute { } // TraversableAttribute

	#endregion class TraversableAttribute

	#region interface ITraversable

	public interface ITraversable {
		// nothing here
	} // interface ITraversable

	#endregion interface ITraversable

	#region class PropertyTraverser

	public static class PropertyTraverser {
		#region public

		public static void Traverse(Type oType, Action<ITraversable, PropertyInfo> oCallback) {
			if (oType == null)
				throw new ArgumentNullException("oType", "Type to traverse not specified.");

			if (oCallback == null)
				throw new ArgumentNullException("oCallback", "Property callback not specified.");

			if (null == oType.GetInterface(typeof (ITraversable).ToString()))
				throw new NotImplementedException("Type " + oType + " does not implement " + typeof (ITraversable));

			Traverse(null, oType, oCallback);
		} // Traverse

		public static void Traverse<T>(Action<ITraversable, PropertyInfo> oCallback) where T: ITraversable {
			if (oCallback == null)
				throw new ArgumentNullException("oCallback", "Property callback not specified.");

			Traverse(null, typeof(T), oCallback);
		} // Traverse

		public static void Traverse(this ITraversable oInstance, Action<ITraversable, PropertyInfo> oCallback) {
			if (oInstance == null)
				throw new ArgumentNullException("oInstance", "Object to traverse not specified.");

			if (oCallback == null)
				throw new ArgumentNullException("oCallback", "Property callback not specified.");

			Traverse(oInstance, oInstance.GetType(), oCallback);
		} // Traverse

		#endregion public

		#region private

		private static void Traverse(ITraversable oInstance, Type oRealType, Action<ITraversable, PropertyInfo> oCallback) {
			PropertyInfo[] oPropertyList = oRealType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

			var oSelected = new List<PropertyInfo>();

			foreach (PropertyInfo pi in oPropertyList) {
				object[] oAttrList = pi.GetCustomAttributes(typeof(TraversableAttribute), false);

				if (oAttrList.Length > 0)
					oSelected.Add(pi);
			} // foreach

			if (oSelected.Count == 0)
				oSelected.AddRange(oPropertyList);

			foreach (PropertyInfo pi in oSelected)
				oCallback(oInstance, pi);
		} // Traverse

		#endregion private
	} // class PropertyTraverser

	#endregion class PropertyTraverser
} // namespace Ezbob.Utils
