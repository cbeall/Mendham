﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mendham.Equality;

namespace Mendham.Domain
{
	public abstract class ValueObject : IHasEqualityComponents
	{
		protected abstract IEnumerable<object> EqualityComponents { get; }

		public override bool Equals(object obj)
		{
			return this.EqualsFromComponents(obj);
		}

		public override int GetHashCode()
		{
			return this.GetHashCodeFromComponents();
		}

		public static bool operator ==(ValueObject a, ValueObject b)
		{
			return object.Equals(a, b);
		}

		public static bool operator !=(ValueObject a, ValueObject b)
		{
			return !(a == b);
		}

		IEnumerable<object> IHasEqualityComponents.EqualityComponents
		{
			get
			{
				return this.EqualityComponents;
			}
		}
	}

	public abstract class SingleFieldValueObject<T> : ValueObject
	{
		public T Value { get; protected set; }

        public SingleFieldValueObject(T value)
        {
            value.VerifyArgumentNotDefaultValue("Value is required");
            this.Value = value;
        }

        public static implicit operator T(SingleFieldValueObject<T> singleFieldValueObject)
		{
			return singleFieldValueObject.Value;
		}

		protected override IEnumerable<object> EqualityComponents
		{
			get
			{
				yield return this.Value;
			}
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}
}