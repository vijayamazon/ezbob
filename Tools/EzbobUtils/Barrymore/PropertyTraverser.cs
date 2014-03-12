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

		public static T Traverse<T>(Action<ITraversable, PropertyInfo> oCallback) where T: ITraversable, new() {
			if (oCallback == null)
				throw new ArgumentNullException("oCallback", "Property callback not specified.");

			ConstructorInfo oCreator = typeof (T).GetConstructors().FirstOrDefault(ci => ci.GetParameters().Length == 0);

			if (oCreator == null)
				throw new SeldenException("Type " + typeof (T) + " has no parameterless constructor.");

			var oInstance = (T)oCreator.Invoke(null);

			Traverse(oInstance, oCallback);

			return oInstance;
		} // Traverse

		public static void Traverse(this ITraversable oInstance, Action<ITraversable, PropertyInfo> oCallback) {
			if (oInstance == null)
				throw new ArgumentNullException("oInstance", "Object to traverse not specified.");

			if (oCallback == null)
				throw new ArgumentNullException("oCallback", "Property callback not specified.");

			PropertyInfo[] oPropertyList = oInstance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

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

		#endregion public

		#region protected

		#endregion protected

		#region private

		#endregion private
	} // class PropertyTraverser

	#endregion class PropertyTraverser
} // namespace Ezbob.Utils
