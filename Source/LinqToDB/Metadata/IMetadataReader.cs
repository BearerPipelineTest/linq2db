﻿using System.Reflection;

namespace LinqToDB.Metadata
{
	public interface IMetadataReader
	{
		T[] GetAttributes<T>(Type type,                        bool inherit = true) where T : Attribute;
		T[] GetAttributes<T>(Type type, MemberInfo memberInfo, bool inherit = true) where T : Attribute;

		/// <summary>
		/// Gets the dynamic columns defined on given type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>All dynamic columns defined on given type.</returns>
		MemberInfo[] GetDynamicColumns(Type type);
	}
}
