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

namespace NVelocity.Runtime.Parser.Node
{
	using System;

	public class MathUtil
	{
		public static object Add(Type maxType, object left, object right)
		{
			if (maxType == typeof(double))
			{
				return Convert.ToDouble(left) + Convert.ToDouble(right);
			}
			else if (maxType == typeof(float))
			{
				return Convert.ToSingle(left) + Convert.ToSingle(right);
			}
			else if (maxType == typeof(decimal))
			{
				return Convert.ToDecimal(left) + Convert.ToDecimal(right);
			}
			else if (maxType == typeof(long))
			{
				return Convert.ToInt64(left) + Convert.ToInt64(right);
			}
			else if (maxType == typeof(int))
			{
				return Convert.ToInt32(left) + Convert.ToInt32(right);
			}
			else if (maxType == typeof(short))
			{
				return Convert.ToInt16(left) + Convert.ToInt16(right);
			}
			else if (maxType == typeof(sbyte))
			{
				return Convert.ToSByte(left) + Convert.ToSByte(right);
			}
			else if (maxType == typeof(byte))
			{
				return Convert.ToByte(left) + Convert.ToByte(right);
			}

			return 0;
		}

		public static object Mult(Type maxType, object left, object right)
		{
			if (maxType == typeof(double))
			{
				return Convert.ToDouble(left) * Convert.ToDouble(right);
			}
			else if (maxType == typeof(float))
			{
				return Convert.ToSingle(left) * Convert.ToSingle(right);
			}
			else if (maxType == typeof(decimal))
			{
				return Convert.ToDecimal(left) * Convert.ToDecimal(right);
			}
			else if (maxType == typeof(long))
			{
				return Convert.ToInt64(left) * Convert.ToInt64(right);
			}
			else if (maxType == typeof(int))
			{
				return Convert.ToInt32(left) * Convert.ToInt32(right);
			}
			else if (maxType == typeof(short))
			{
				return Convert.ToInt16(left) * Convert.ToInt16(right);
			}
			else if (maxType == typeof(sbyte))
			{
				return Convert.ToSByte(left) * Convert.ToSByte(right);
			}
			else if (maxType == typeof(byte))
			{
				return Convert.ToByte(left) * Convert.ToByte(right);
			}

			return 0;
		}

		public static object Div(Type maxType, object left, object right)
		{
			if (maxType == typeof(double))
			{
				return Convert.ToDouble(left) / Convert.ToDouble(right);
			}
			else if (maxType == typeof(float))
			{
				return Convert.ToSingle(left) / Convert.ToSingle(right);
			}
			else if (maxType == typeof(decimal))
			{
				return Convert.ToDecimal(left) / Convert.ToDecimal(right);
			}
			else if (maxType == typeof(long))
			{
				return Convert.ToInt64(left) / Convert.ToInt64(right);
			}
			else if (maxType == typeof(int))
			{
				return Convert.ToInt32(left) / Convert.ToInt32(right);
			}
			else if (maxType == typeof(short))
			{
				return Convert.ToInt16(left) / Convert.ToInt16(right);
			}
			else if (maxType == typeof(sbyte))
			{
				return Convert.ToSByte(left) / Convert.ToSByte(right);
			}
			else if (maxType == typeof(byte))
			{
				return Convert.ToByte(left) / Convert.ToByte(right);
			}

			return 0;
		}

		public static object Mod(Type maxType, object left, object right)
		{
			if (maxType == typeof(double))
			{
				return Convert.ToDouble(left) % Convert.ToDouble(right);
			}
			else if (maxType == typeof(float))
			{
				return Convert.ToSingle(left) % Convert.ToSingle(right);
			}
			else if (maxType == typeof(decimal))
			{
				return Convert.ToDecimal(left) % Convert.ToDecimal(right);
			}
			else if (maxType == typeof(long))
			{
				return Convert.ToInt64(left) % Convert.ToInt64(right);
			}
			else if (maxType == typeof(int))
			{
				return Convert.ToInt32(left) % Convert.ToInt32(right);
			}
			else if (maxType == typeof(short))
			{
				return Convert.ToInt16(left) % Convert.ToInt16(right);
			}
			else if (maxType == typeof(sbyte))
			{
				return Convert.ToSByte(left) % Convert.ToSByte(right);
			}
			else if (maxType == typeof(byte))
			{
				return Convert.ToByte(left) % Convert.ToByte(right);
			}

			return 0;
		}

		public static object Sub(Type maxType, object left, object right)
		{
			if (maxType == typeof(double))
			{
				return Convert.ToDouble(left) - Convert.ToDouble(right);
			}
			else if (maxType == typeof(float))
			{
				return Convert.ToSingle(left) - Convert.ToSingle(right);
			}
			else if (maxType == typeof(decimal))
			{
				return Convert.ToDecimal(left) - Convert.ToDecimal(right);
			}
			else if (maxType == typeof(long))
			{
				return Convert.ToInt64(left) - Convert.ToInt64(right);
			}
			else if (maxType == typeof(int))
			{
				return Convert.ToInt32(left) - Convert.ToInt32(right);
			}
			else if (maxType == typeof(short))
			{
				return Convert.ToInt16(left) - Convert.ToInt16(right);
			}
			else if (maxType == typeof(sbyte))
			{
				return Convert.ToSByte(left) - Convert.ToSByte(right);
			}
			else if (maxType == typeof(byte))
			{
				return Convert.ToByte(left) - Convert.ToByte(right);
			}

			return 0;
		}

		public static Type ToMaxType(Type leftType, Type rightType)
		{
			if (leftType == rightType) return leftType;

			if (leftType == typeof(double) || rightType == typeof(double))
			{
				return typeof(double);
			}
			else if (leftType == typeof(float) || rightType == typeof(float))
			{
				return typeof(float);
			}
			else if (leftType == typeof(decimal) || rightType == typeof(decimal))
			{
				return typeof(decimal);
			}
			else if (leftType == typeof(long) || rightType == typeof(long))
			{
				return typeof(long);
			}
			else if (leftType == typeof(int) || rightType == typeof(int))
			{
				return typeof(int);
			}
			else if (leftType == typeof(short) || rightType == typeof(short))
			{
				return typeof(short);
			}
			else if (leftType == typeof(sbyte) || rightType == typeof(sbyte))
			{
				return typeof(sbyte);
			}
			else if (leftType == typeof(byte) || rightType == typeof(byte))
			{
				return typeof(byte);
			}

			return null;
		}
	}
}