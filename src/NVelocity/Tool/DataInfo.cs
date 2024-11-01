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

namespace NVelocity.Tool
{
	using System;

	/// <summary> ToolInfo implementation to handle "primitive" data types.
	/// It currently supports string, Number, and Boolean data.
	/// *
	/// </summary>
	/// <author> <a href="mailto:nathan@esha.com">Nathan Bubna</a>
	/// *
	/// </author>
	/// <version> $Id: DataInfo.cs,v 1.2 2003/10/27 13:54:12 corts Exp $
	///
	/// </version>
	public class DataInfo : IToolInfo
	{
		public static readonly string TYPE_STRING = "string";
		public static readonly string TYPE_NUMBER = "number";
		public static readonly string TYPE_BOOLEAN = "boolean";

		private readonly string key;
		private readonly object data;

		/// <summary> Parses the value string into a recognized type. If
		/// the type specified is not supported, the data will
		/// be held and returned as a string.
		/// *
		/// </summary>
		/// <param name="key">the context key for the data
		/// </param>
		/// <param name="type">the data type
		/// </param>
		/// <param name="value">the data
		///
		/// </param>
		public DataInfo(string key, string type, string value)
		{
			this.key = key;

			if (type.ToUpper().Equals(TYPE_BOOLEAN.ToUpper()))
			{
				data = bool.Parse(value);
			}
			else if (type.ToUpper().Equals(TYPE_NUMBER.ToUpper()))
			{
				if (value.IndexOf('.') >= 0)
				{
					//UPGRADE_TODO: Format of parameters of constructor 'java.lang.Double.Double' are different in the equivalent in .NET. 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="jlca1092"'
					data = double.Parse(value);
				}
				else
				{
					data = int.Parse(value);
				}
			}
			else
			{
				data = value;
			}
		}

		public string Key
		{
			get { return key; }
		}

		public string Classname
		{
			get { return data.GetType().FullName; }
		}

		/// <summary> Returns the data. Always returns the same
		/// object since the data is a constant. Initialization
		/// data is ignored.
		/// </summary>
		public object getInstance(object initData)
		{
			return data;
		}
	}
}