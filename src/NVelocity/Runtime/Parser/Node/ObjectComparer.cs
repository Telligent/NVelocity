// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System.Collections.Generic;

namespace NVelocity.Runtime.Parser.Node
{
	using System;
	using System.Collections;

	/// <summary>
	/// ObjectComparer allows you to compare primitive types and some others
	/// using IComparable interface whenever possible, and performing type
	/// conversions to get the best possible result.
	/// </summary>
	public class ObjectComparer : IComparer
	{
		#region Static members & constants

		public const int Smaller = -1;
		public const int Equal = 0;
		public const int Greater = 1;

		private static readonly Dictionary<string, IObjectComparer> comparers = new();
		private static readonly ObjectComparer instance = new();

		private static readonly Dictionary<Type, bool> _reTypeToLong = new Dictionary<Type, bool> {
			{ typeof(char), true },
			{ typeof(byte), true },
			{ typeof(sbyte), true },
			{ typeof(short), true },
			{ typeof(ushort), true },
			{ typeof(int), true },
			{ typeof(uint), true },
			{typeof (long), true }
		};

		static ObjectComparer()
		{
			new DoubleComparer().Register(comparers);
			new FloatComparer().Register(comparers);
			new ULongComparer().Register(comparers);
		}

		/// <summary>
		/// Tries to compare two random objects.  -1 is returned 
		/// if x is smaller than y, 1 the other way around, or 0 
		/// if they are equal.
		/// </summary>
		public static int CompareObjects(object x, object y)
		{
			return instance.Compare(x, y);
		}

		#endregion

		#region Compare method & related

		public int Compare(object x, object y)
		{
			Type xType, yType;

			if (x != null)
			{
				xType = x.GetType();
			}
			else
			{
				return Smaller;
			}

			if (y != null)
			{
				yType = y.GetType();
			}
			else
			{
				return Greater;
			}

			if (x is string || y is string)
			{
				return string.Compare(x.ToString(), y.ToString());
			}

			if (xType.IsPrimitive && yType.IsPrimitive)
			{
				return ComparePrimitive(x, y, xType, yType);
			}

			if (xType == typeof(decimal) || yType == typeof(decimal))
			{
				return decimal.Compare(Convert.ToDecimal(x), Convert.ToDecimal(y));
			}

			// Finally try a IComparable comparison
			if (x is IComparable)
			{
				return (x as IComparable).CompareTo(y);
			}
			else if (y is IComparable)
			{
				return -(y as IComparable).CompareTo(x);
			}

			throw new ArgumentException(string.Format("Unable to compare {0} and {1}", x, y));
		}

		private int ComparePrimitive(object x, object y, Type typeX, Type typeY)
		{
			x = ReType(x, ref typeX);
			y = ReType(y, ref typeY);

			if (typeX == typeY)
			{
				return (x as IComparable).CompareTo(y);
			}

			if (comparers.TryGetValue(string.Format("{0}:{1}", typeX, typeY), out IObjectComparer cmp))
			{
				return cmp.Compare(x, y);
			}

			throw new ArgumentException(string.Format("Unable to compare {0} and {1}", typeX, typeY));
		}

		private object ReType(object value, ref Type t)
		{
			if (_reTypeToLong.ContainsKey(t))
			{
				value = Convert.ToInt64(value);
				t = typeof(long);
			}

			return value;
		}

		#endregion

		#region IObjectComparer

		private interface IObjectComparer
		{
			void Register(Dictionary<string, IObjectComparer> map);
			int Compare(object x, object y);
		}

		#endregion

		#region Compare double + type

		private class DoubleComparer : IObjectComparer
		{
			public void Register(Dictionary<string, IObjectComparer> map)
			{
				map[string.Format("{0}:{1}", typeof(double), typeof(long))] = this;
				map[string.Format("{0}:{1}", typeof(long), typeof(double))] = this;
				map[string.Format("{0}:{1}", typeof(double), typeof(ulong))] = this;
				map[string.Format("{0}:{1}", typeof(ulong), typeof(double))] = this;
				map[string.Format("{0}:{1}", typeof(float), typeof(double))] = this;
				map[string.Format("{0}:{1}", typeof(double), typeof(float))] = this;
			}

			public int Compare(object x, object y)
			{
				if (x is double)
					return Compare((double)x, y);

				return -Compare((double)y, x);
			}

			public int Compare(double d, object y)
			{
				if (y is long)
				{
					long l = (long)y;
					return d == l ? Equal : (d < l ? Smaller : Greater);
				}

				if (y is ulong)
				{
					ulong l = (ulong)y;
					return d == l ? Equal : (d < l ? Smaller : Greater);
				}

				if (y is float f)
				{
					return d == f ? Equal : (d < f ? Smaller : Greater);
				}

				throw new ArgumentException(string.Format("Unable to compare double and {0}", y.GetType()));
			}
		}

		#endregion

		#region Compare float + type

		private class FloatComparer : IObjectComparer
		{
			public void Register(Dictionary<string, IObjectComparer> map)
			{
				map[string.Format("{0}:{1}", typeof(float), typeof(ulong))] = this;
				map[string.Format("{0}:{1}", typeof(ulong), typeof(float))] = this;
				map[string.Format("{0}:{1}", typeof(float), typeof(long))] = this;
				map[string.Format("{0}:{1}", typeof(long), typeof(float))] = this;
			}

			public int Compare(object x, object y)
			{
				if (x is float)
					return Compare((float)x, y);

				return -Compare((float)y, x);
			}

			public int Compare(float f, object y)
			{
				if (y is long)
				{
					long l = (long)y;
					return f == l ? Equal : (f < l ? Smaller : Greater);
				}

				if (y is ulong)
				{
					ulong l = (ulong)y;
					return f == l ? Equal : (f < l ? Smaller : Greater);
				}

				throw new ArgumentException(string.Format("Unable to compare float and {0}", y.GetType()));
			}
		}

		#endregion

		#region Compare ulong + type

		private class ULongComparer : IObjectComparer
		{
			public void Register(Dictionary<string, IObjectComparer> map)
			{
				map[string.Format("{0}:{1}", typeof(long), typeof(ulong))] = this;
				map[string.Format("{0}:{1}", typeof(ulong), typeof(long))] = this;
			}

			public int Compare(object x, object y)
			{
				if (x is ulong xAsLong)
				{
					return Compare(xAsLong, y);
				}

				return -Compare((ulong)y, x);
			}

			public int Compare(ulong ul, object y)
			{
				if (y is long l)
				{
					if (l < 0)
					{
						return Smaller;
					}

					ulong ull = (ulong)l;

					return ul == ull ? Equal : (ul < ull ? Smaller : Greater);
				}

				throw new ArgumentException(string.Format("Unable to compare long and {0}", y.GetType()));
			}
		}

		#endregion
	}
}