﻿using System.Collections.Concurrent;
using System.Diagnostics;

namespace LinqToDB.Reflection
{
	[DebuggerDisplay("Type = {" + nameof(Type) + "}")]
	public abstract class TypeAccessor
	{
		#region Protected Emit Helpers

		protected void AddMember(MemberAccessor member)
		{
			if (member == null) ThrowHelper.ThrowArgumentNullException(nameof(member));

			Members.Add(member);
			_membersByName[member.MemberInfo.Name] = member;
		}

		#endregion

		#region CreateInstance

		[DebuggerStepThrough]
		public virtual object CreateInstance()
		{
			return ThrowHelper.ThrowLinqToDBException<object>($"The '{Type.Name}' type must have public default or init constructor.");
		}

		[DebuggerStepThrough]
		public object CreateInstanceEx()
		{
			return ObjectFactory != null ? ObjectFactory.CreateInstance(this) : CreateInstance();
		}

		#endregion

		#region Public Members

		public IObjectFactory?         ObjectFactory { get; set; }
		public abstract Type           Type          { get; }

		#endregion

		#region Items

		public List<MemberAccessor>    Members       { get; } = new();

		readonly ConcurrentDictionary<string,MemberAccessor> _membersByName = new();

		public MemberAccessor this[string memberName] =>
			_membersByName.GetOrAdd(memberName, name =>
			{
				var ma = new MemberAccessor(this, name, null);
				Members.Add(ma);
				return ma;
			});

		public MemberAccessor this[int index] => Members[index];

		#endregion

		#region Static Members

		static readonly ConcurrentDictionary<Type,TypeAccessor> _accessors = new();

		public static TypeAccessor GetAccessor(Type type)
		{
			if (type == null) ThrowHelper.ThrowArgumentNullException(nameof(type));

			if (_accessors.TryGetValue(type, out var accessor))
				return accessor;

			var accessorType = typeof(TypeAccessor<>).MakeGenericType(type);

			accessor = (TypeAccessor)Activator.CreateInstance(accessorType, true)!;

			_accessors[type] = accessor;

			return accessor;
		}

		public static TypeAccessor<T> GetAccessor<T>()
		{
			if (_accessors.TryGetValue(typeof(T), out var accessor))
				return (TypeAccessor<T>)accessor;
			return (TypeAccessor<T>)(_accessors[typeof(T)] = new TypeAccessor<T>());
		}

		#endregion
	}
}
