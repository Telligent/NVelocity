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

namespace Commons.Collections
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Linq;

		/// <summary>
		/// This class extends normal Java properties by adding the possibility
		/// to use the same key many times concatenating the value strings
		/// instead of overwriting them.
		///
		/// <para>The Extended Properties syntax is explained here:
		///
		/// <ul>
		/// <li>
		/// Each property has the syntax <code>key = value</code>
		/// </li>
		/// <li>
		/// The <i>key</i> may use any character but the equal sign '='.
		/// </li>
		/// <li>
		/// <i>value</i> may be separated on different lines if a backslash
		/// is placed at the end of the line that continues below.
		/// </li>
		/// <li>
		/// If <i>value</i> is a list of strings, each token is separated
		/// by a comma ','.
		/// </li>
		/// <li>
		/// Commas in each token are escaped placing a backslash right before
		/// the comma.
		/// </li>
		/// <li>
		/// If a <i>key</i> is used more than once, the values are appended
		/// like if they were on the same line separated with commas.
		/// </li>
		/// <li>
		/// Blank lines and lines starting with character '#' are skipped.
		/// </li>
		/// <li>
		/// If a property is named "include" (or whatever is defined by
		/// setInclude() and getInclude() and the value of that property is
		/// the full path to a file on disk, that file will be included into
		/// the ConfigurationsRepository. You can also pull in files relative
		/// to the parent configuration file. So if you have something
		/// like the following:
		///
		/// include = additional.properties
		///
		/// Then "additional.properties" is expected to be in the same
		/// directory as the parent configuration file.
		///
		/// Duplicate name values will be replaced, so be careful.
		///
		/// </li>
		/// </ul>
		/// </para>
		/// <para>Here is an example of a valid extended properties file:
		/// </para>
		/// <para><pre>
		/// # lines starting with # are comments
		///
		/// # This is the simplest property
		/// key = value
		///
		/// # A long property may be separated on multiple lines
		/// longvalue = aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa \
		/// aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
		///
		/// # This is a property with many tokens
		/// tokens_on_a_line = first token, second token
		///
		/// # This sequence generates exactly the same result
		/// tokens_on_multiple_lines = first token
		/// tokens_on_multiple_lines = second token
		///
		/// # commas may be escaped in tokens
		/// commas.excaped = Hi\, what'up?
		/// </pre>
		/// </para>
		/// <para><b>NOTE</b>: this class has <b>not</b> been written for
		/// performance nor low memory usage.  In fact, it's way slower than it
		/// could be and generates too much memory garbage.  But since
		/// performance is not an issue during intialization (and there is not
		/// much time to improve it), I wrote it this way.  If you don't like
		/// it, go ahead and tune it up!</para>
		/// </summary>
		public class ExtendedProperties : Dictionary<string, object>
	{
		private static readonly byte DEFAULT_BYTE = 0;
		private static readonly bool DEFAULT_BOOLEAN = false;
		private static readonly int DEFAULT_INT32 = 0;
		private static readonly float DEFAULT_SINGLE = 0;
		private static readonly long DEFAULT_INT64 = 0;
		private static readonly double DEFAULT_DOUBLE = 0;

		/// <summary> Default configurations repository.
		/// </summary>
		private readonly ExtendedProperties defaults;

		/// <summary>
		/// The file connected to this repository (holding comments and such).
		/// </summary>
		protected internal string file;

		/// <summary>
		/// Base path of the configuration file used to create
		/// this ExtendedProperties object.
		/// </summary>
		protected internal string basePath;

		/// <summary>
		/// File separator.
		/// </summary>
		protected internal string fileSeparator = Path.AltDirectorySeparatorChar.ToString();

		/// <summary>
		/// Has this configuration been initialized.
		/// </summary>
		protected internal bool isInitialized = false;

		/// <summary>
		/// This is the name of the property that can point to other
		/// properties file for including other properties files.
		/// </summary>
		protected internal static string include = "include";

		/// <summary>
		/// These are the keys in the order they listed
		/// in the configuration file. This is useful when
		/// you wish to perform operations with configuration
		/// information in a particular order.
		/// </summary>
		protected internal List<string> keysAsListed = new();

		/// <summary>
		/// Creates an empty extended properties object.
		/// </summary>
		public ExtendedProperties()
		{
		}

		/// <summary>
		/// Creates and loads the extended properties from the specified
		/// file.
		/// </summary>
		/// <param name="file">A string.</param>
		/// <exception cref="IOException" />
		public ExtendedProperties(string file) : this(file, null)
		{
		}

		/// <summary>
		/// Creates and loads the extended properties from the specified
		/// file.
		/// </summary>
		/// <param name="file">A string.</param>
		/// <param name="defaultFile">File to load defaults from.</param>
		/// <exception cref="IOException" />
		public ExtendedProperties(string file, string defaultFile)
		{
			this.file = file;

			basePath = new FileInfo(file).FullName;
			basePath = basePath[..(basePath.LastIndexOf(fileSeparator) + 1)];

			Load(new FileStream(file, FileMode.Open, FileAccess.Read));

			if (defaultFile != null)
			{
				defaults = new ExtendedProperties(defaultFile);
			}
		}

		//Not used
		///// <summary>
		///// Private initializer method that sets up the generic
		///// resources.
		///// </summary>
		///// <exception cref="IOException">if there was an I/O problem.</exception>
		//private void Init(ExtendedProperties exp)
		//{
		//    isInitialized = true;
		//}

		/// <summary>
		/// Indicate to client code whether property
		/// resources have been initialized or not.
		/// </summary>
		public bool IsInitialized()
		{
			return isInitialized;
		}

		public string Include
		{
			get { return include; }
			set { include = value; }
		}

		public new IEnumerable<string> Keys
		{
			get { return keysAsListed; }
		}

		public void Load(Stream input)
		{
			Load(input, null);
		}

		/// <summary>
		/// Load the properties from the given input stream
		/// and using the specified encoding.
		/// </summary>
		/// <param name="input">An InputStream.
		/// </param>
		/// <param name="encoding">An encoding.
		/// </param>
		/// <exception cref="IOException" />
		public void Load(Stream input, string encoding)
		{
			lock (this)
			{
				PropertiesReader reader = null;
				if (encoding != null)
				{
					try
					{
						reader = new PropertiesReader(new StreamReader(input, Encoding.GetEncoding(encoding)));
					}
					catch (IOException)
					{
						// Get one with the default encoding...
					}
				}

				reader ??= new PropertiesReader(new StreamReader(input));

				try
				{
					while (true)
					{
						string line = reader.ReadProperty();

						if (line == null)
						{
							break;
						}

						int equalSignIndex = line.IndexOf('=');

						if (equalSignIndex > 0)
						{
							string key = line[..equalSignIndex].Trim();
							string value = line[(equalSignIndex + 1)..].Trim();

							/*
								* Configure produces lines like this ... just
								* ignore them.
								*/
							if (string.Empty.Equals(value))
							{
								continue;
							}

							if (Include != null && key.ToUpper().Equals(Include.ToUpper()))
							{
								/*
									* Recursively load properties files.
									*/
								FileInfo file;

								if (value.StartsWith(fileSeparator))
								{
									/*
										* We have an absolute path so we'll
										* use this.
										*/
									file = new FileInfo(value);
								}
								else
								{
									/*
										* We have a relative path, and we have
										* two possible forms here. If we have the
										* "./" form then just strip that off first
										* before continuing.
										*/
									if (value.StartsWith(string.Format(".{0}", fileSeparator)))
									{
										value = value[2..];
									}
									file = new FileInfo(basePath + value);
								}

								bool tmpBool;
								if (File.Exists(file.FullName))
								{
									tmpBool = true;
								}
								else
								{
									tmpBool = Directory.Exists(file.FullName);
								}
								// TODO: make sure file is readable or handle exception appropriately
								//if (file != null && tmpBool && file.canRead()) {
								if (tmpBool)
								{
									Load(new FileStream(file.FullName, FileMode.Open, FileAccess.Read));
								}
							}
							else
							{
								AddProperty(key, value);
							}
						}
					}
				}
				catch (NullReferenceException)
				{
					/*
						* Should happen only when EOF is reached.
						*/
					return;
				}
				reader.Close();
			}
		}

		/// <summary>  Gets a property from the configuration.
		/// *
		/// </summary>
		/// <param name="key">property to retrieve
		/// </param>
		/// <returns>value as object. Will return user value if exists,
		/// if not then default value if exists, otherwise null
		///
		/// </returns>
		public object GetProperty(string key)
		{
			/*
			*  first, try to get from the 'user value' store
			*/
			object o;
			if (!TryGetValue(key, out o))
				o = null;

			if (o == null)
			{
				// if there isn't a value there, get it from the
				// defaults if we have them
				if (defaults != null)
				{
					o = defaults[key];
				}
			}

			return o;
		}

		/// <summary> Add a property to the configuration. If it already
		/// exists then the value stated here will be added
		/// to the configuration entry. For example, if
		/// *
		/// resource.loader = file
		/// *
		/// is already present in the configuration and you
		/// *
		/// addProperty("resource.loader", "classpath")
		/// *
		/// Then you will end up with a Vector like the
		/// following:
		/// *
		/// ["file", "classpath"]
		/// *
		/// </summary>
		/// <param name="key"></param>
		/// <param name="token"></param>
		public void AddProperty(string key, object token)
		{
			object o;
			if (!TryGetValue(key, out o))
				o = null;

			/*
			*  $$$ GMJ
			*  FIXME : post 1.0 release, we need to not assume
			*  that a scalar is a string - it can be an object
			*  so we should make a little vector-like class
			*  say, Foo that wraps (not extends Vector),
			*  so we can do things like
			*  if ( !( o instanceof Foo) )
			*  so we know it's our 'vector' container
			*
			*  This applies throughout
			*/

			if (o is string oldSingleValue)
			{
				List<string> v = new()
				{
						oldSingleValue,
						token.ToString()
				};
				this[key] = v;
			}
			else if (o is List<string> list)
			{
				list.Add(token.ToString());
			}
			else
			{
				/*
		* This is the first time that we have seen
		* request to place an object in the 
		* configuration with the key 'key'. So
		* we just want to place it directly into
		* the configuration ... but we are going to
		* make a special exception for string objects
		* that contain "," characters. We will take
		* CSV lists and turn the list into a vector of
		* Strings before placing it in the configuration.
		* This is a concession for Properties and the
		* like that cannot parse multiple same key
		* values.
		*/
				if (token is string stringToken && stringToken.IndexOf(PropertiesTokenizer.DELIMITER) > 0)
				{
					PropertiesTokenizer tokenizer = new(stringToken);

					while (tokenizer.HasMoreTokens())
					{
						string s = tokenizer.NextToken();

						/*
			* we know this is a string, so make sure it
			* just goes in rather than risking vectorization
			* if it contains an escaped comma
			*/
						AddStringProperty(key, s);
					}
				}
				else
				{
					/*
				* We want to keep track of the order the keys
				* are parsed, or dynamically entered into
				* the configuration. So when we see a key
				* for the first time we will place it in
				* a list so that if a client class needs
				* to perform operations with configuration
				* in a definite order it will be possible.
				*/
					AddPropertyDirect(key, token);
				}
			}
		}

		/// <summary>   Adds a key/value pair to the map.  This routine does
		/// no magic morphing.  It ensures the keyList is maintained
		/// *
		/// </summary>
		/// <param name="key">key to use for mapping
		/// </param>
		/// <param name="obj">object to store
		///
		/// </param>
		private void AddPropertyDirect(string key, object obj)
		{
			/*
			* safety check
			*/

			if (!ContainsKey(key))
			{
				keysAsListed.Add(key);
			}

			/*
			* and the value
			*/
			this[key] = obj;
		}

		/// <summary>  Sets a string property w/o checking for commas - used
		/// internally when a property has been broken up into
		/// strings that could contain escaped commas to prevent
		/// the inadvertent vectorization.
		///
		/// Thanks to Leon Messerschmidt for this one.
		///
		/// </summary>
		private void AddStringProperty(string key, string token)
		{
			object o;
			if (!TryGetValue(key, out o))
				o = null;

			/*
			*  $$$ GMJ
			*  FIXME : post 1.0 release, we need to not assume
			*  that a scalar is a string - it can be an object
			*  so we should make a little vector-like class
			*  say, Foo that wraps (not extends Vector),
			*  so we can do things like
			*  if ( !( o instanceof Foo) )
			*  so we know it's our 'vector' container
			*
			*  This applies throughout
			*/

			/*
			*  do the usual thing - if we have a value and 
			*  it's scalar, make a vector, otherwise add
			*  to the vector
			*/

			if (o is string oldSingleValue)
			{
				List<string> v = new()
				{
						oldSingleValue,
						token
				};
				this[key] = v;
			}
			else if (o is List<string> list)
			{
				list.Add(token);
			}
			else
			{
				AddPropertyDirect(key, token);
			}
		}

		/// <summary> Set a property, this will replace any previously
		/// set values. Set values is implicitly a call
		/// to clearProperty(key), addProperty(key,value).
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void SetProperty(string key, object value)
		{
			ClearProperty(key);
			AddProperty(key, value);
		}

		/// <summary> Save the properties to the given outputStream.
		/// </summary>
		/// <param name="output">An OutputStream.
		/// </param>
		/// <param name="Header">A string.
		/// </param>
		/// <exception cref="IOException">
		/// </exception>
		public void Save(TextWriter output, string Header)
		{
			lock (this)
			{
				if (output != null)
				{
					TextWriter textWriter = output;
					if (Header != null)
					{
						textWriter.WriteLine(Header);
					}

					foreach (string key in Keys)
					{
						object value = this[key];
						if (value == null)
						{
							continue;
						}

						if (value is string)
						{
							WriteKeyOutput(textWriter, key, (string)value);
						}
						else if (value is IEnumerable)
						{
							foreach (string currentElement in (IEnumerable)value)
								WriteKeyOutput(textWriter, key, currentElement);
						}

						textWriter.WriteLine();
						textWriter.Flush();
					}
				}
			}
		}

		private void WriteKeyOutput(TextWriter textWriter, string key, string value)
		{
			StringBuilder currentOutput = new();
			currentOutput.Append(key).Append('=').Append(value);
			textWriter.WriteLine(currentOutput.ToString());
		}

		/// <summary> Combines an existing ExtendedProperties with this ExtendedProperties.
		/// *
		/// Warning: It will overwrite previous entries without warning.
		/// *
		/// </summary>
		/// <param name="c">ExtendedProperties
		///
		/// </param>
		public void Combine(ExtendedProperties c)
		{
			foreach (string key in c.Keys)
			{
				object o = c[key];
				// if the value is a string, escape it so that if there are delimiters that the value is not converted to a list
				if (o is string v)
					o = v.Replace(",", @"\,");
				
				SetProperty(key, o);
			}
		}

		/// <summary> Clear a property in the configuration.
		/// *
		/// </summary>
		/// <param name="key">key to remove along with corresponding value.
		///
		/// </param>
		public void ClearProperty(string key)
		{
			if (ContainsKey(key))
			{
				/*
				* we also need to rebuild the keysAsListed or else
				* things get *very* confusing
				*/
				keysAsListed.Remove(key);
				Remove(key);
			}
		}

		/// <summary> Get the list of the keys contained in the configuration
		/// repository.
		/// *
		/// </summary>
		/// <returns>An Iterator.
		///
		/// </returns>
		/// <summary> Get the list of the keys contained in the configuration
		/// repository that match the specified prefix.
		/// *
		/// </summary>
		/// <param name="prefix">The prefix to test against.
		/// </param>
		/// <returns>An Iterator of keys that match the prefix.
		///
		/// </returns>
		public IEnumerable<string> GetKeys(string prefix)
		{
			return Keys.Where(k => k.StartsWith(prefix));
		}

		/// <summary> Create an ExtendedProperties object that is a subset
		/// of this one. Take into account duplicate keys
		/// by using the setProperty() in ExtendedProperties.
		/// *
		/// </summary>
		/// <param name="prefix">prefix
		///
		/// </param>
		public ExtendedProperties Subset(string prefix)
		{
			ExtendedProperties c = new();
			bool validSubset = false;

			foreach (string key in Keys)
			{
				if (key.StartsWith(prefix))
				{
					if (!validSubset)
						validSubset = true;

					string newKey;

					/*
					* Check to make sure that c.subset(prefix) doesn't
					* blow up when there is only a single property
					* with the key prefix. This is not a useful
					* subset but it is a valid subset.
					*/
					if (key.Length == prefix.Length)
					{
						newKey = prefix;
					}
					else
					{
						newKey = key[(prefix.Length + 1)..];
					}

					/*
						*  use addPropertyDirect() - this will plug the data as 
						*  is into the Map, but will also do the right thing
						*  re key accounting
						*/

					c.AddPropertyDirect(newKey, this[key]);
				}
			}

			if (validSubset)
			{
				return c;
			}
			else
			{
				return null;
			}
		}

		/// <summary> Display the configuration for debugging
		/// purposes.
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new();
			foreach (string key in Keys)
			{
				object value = this[key];
				sb.AppendFormat("{0} => {1}", key, ValueToString(value)).Append(Environment.NewLine);
			}
			return sb.ToString();
		}

		private string ValueToString(object value)
		{
			if (value is List<string> list)
			{
				var s = new StringBuilder();
				foreach (string o in list)
				{
					if (s.Length > 0)
					{
						s.Append(", ");
					}
					s.Append("[").Append(o).Append("]");
				}

				s.Insert(0, "List<string> :: ");
				return s.ToString();
			}
			else
			{
				return value.ToString();
			}
		}

		/// <summary> Get a string associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <returns>The associated string.
		/// </returns>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a string.
		///
		/// </exception>
		public string GetString(string key)
		{
			return GetString(key, null);
		}

		/// <summary> Get a string associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <param name="defaultValue">The default value.
		/// </param>
		/// <returns>The associated string if key is found,
		/// default value otherwise.
		/// </returns>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a string.
		///
		/// </exception>
		public string GetString(string key, string defaultValue)
		{
			object value;
			if (!TryGetValue(key, out value))
				value = null;

			if (value is string)
			{
				return (string)value;
			}
			else if (value == null)
			{
				if (defaults == null)
				{
					return defaultValue;
				}
				else
				{
					return defaults.GetString(key, defaultValue);
				}
			}
			else if (value is List<string> list)
			{
				return list.FirstOrDefault();
			}
			else
			{
				throw new InvalidCastException(string.Format("{0}{1}' doesn't map to a string object", '\'', key));
			}
		}

		/// <summary> Get a list of properties associated with the given
		/// configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <returns>The associated properties if key is found.
		/// </returns>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a string/Vector.
		/// </exception>
		/// <exception cref="ArgumentException"> if one of the tokens is
		/// malformed (does not contain an equals sign).
		///
		/// </exception>
		public Dictionary<string, string> GetProperties(string key)
		{
			//UPGRADE_TODO: Format of property file may need to be changed. 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="jlca1089"'
			return GetProperties(key, new Dictionary<string, string>());
		}

		/// <summary> Get a list of properties associated with the given
		/// configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <param name="defaultProps">Default property values.
		/// </param>
		/// <returns>The associated properties if key is found.
		/// </returns>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a string/Vector.
		/// </exception>
		/// <exception cref="ArgumentException"> if one of the tokens is
		/// malformed (does not contain an equals sign).
		///
		/// </exception>
		public Dictionary<string, string> GetProperties(string key, Dictionary<string, string> defaultProps)
		{
			/*
			* Grab an array of the tokens for this key.
			*/
			var tokens = GetStringList(key);

			/*
			* Each token is of the form 'key=value'.
			*/
			Dictionary<string, string> props = new(defaultProps);
			foreach (var token in tokens)
			{
				int equalSign = token.IndexOf('=');
				if (equalSign > 0)
				{
					string pkey = token[..equalSign].Trim();
					string pvalue = token[(equalSign + 1)..].Trim();
					props[pkey] = pvalue;
				}
				else
				{
					throw new ArgumentException(string.Format("{0}{1}' does not contain an equals sign", '\'', token));
				}
			}
			return props;
		}

		/// <summary>
		/// Gets the string list.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public List<string> GetStringList(string key)
		{
			return GetStringList(key, null);
		}

		/// <summary> Get a Vector of strings associated with the given configuration
		/// key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <param name="defaultValue">The default value.
		/// </param>
		/// <returns>The associated Vector.
		/// </returns>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Vector.
		///
		/// </exception>
		public List<string> GetStringList(string key, List<string> defaultValue)
		{
			object value;
			if (!TryGetValue(key, out value))
				value = null;

			if (value is List<string> l)
			{
				return l;
			}
			else if (value is string s)
			{
				List<string> v = new(1)
				{
						s
				};
				this[key] = v;
				return v;
			}
			else if (value == null)
			{
				if (defaults == null)
				{
					return (defaultValue ?? new List<string>());
				}
				else
				{
					return defaults.GetStringList(key, defaultValue);
				}
			}
			else
			{
				throw new InvalidCastException(string.Format("{0}{1}' doesn't map to a Vector object", '\'', key));
			}
		}

		/// <summary> Get a boolean associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <returns>The associated boolean.
		/// </returns>
		/// <exception cref="Exception"> is thrown if the key doesn't
		/// map to an existing object.
		/// </exception>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Boolean.
		///
		/// </exception>
		public bool GetBoolean(string key)
		{
			return GetBoolean(key, DEFAULT_BOOLEAN);
		}

		/// <summary> Get a boolean associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <param name="defaultValue">The default value.
		/// </param>
		/// <returns>The associated boolean if key is found and has valid
		/// format, default value otherwise.
		/// </returns>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Boolean.
		///
		/// </exception>
		public bool GetBoolean(string key, bool defaultValue)
		{
			object value;
			if (!TryGetValue(key, out value))
				value = null;

			if (value is bool)
			{
				return (bool)value;
			}
			else if (value is string)
			{
				string s = TestBoolean((string)value);
				var b = s.ToUpper().Equals("TRUE");
				this[key] = b;
				return b;
			}
			else if (value == null)
			{
				if (defaults == null)
				{
					return defaultValue;
				}
				else
				{
					return defaults.GetBoolean(key, defaultValue);
				}
			}
			else
			{
				throw new InvalidCastException(string.Format("{0}{1}' doesn't map to a Boolean object", '\'', key));
			}
		}

		/// <summary> Test whether the string represent by value maps to a boolean
		/// value or not. We will allow <code>true</code>, <code>on</code>,
		/// and <code>yes</code> for a <code>true</code> boolean value, and
		/// <code>false</code>, <code>off</code>, and <code>no</code> for
		/// <code>false</code> boolean values.  Case of value to test for
		/// boolean status is ignored.
		/// *
		/// </summary>
		/// <param name="value">The value to test for boolean state.
		/// </param>
		/// <returns><code>true</code> or <code>false</code> if the supplied
		/// text maps to a boolean value, or <code>null</code> otherwise.
		///
		/// </returns>
		public static string TestBoolean(string value)
		{
			string s = value.ToLower();

			if (s.Equals("true") || s.Equals("on") || s.Equals("yes"))
			{
				return "true";
			}
			else if (s.Equals("false") || s.Equals("off") || s.Equals("no"))
			{
				return "false";
			}
			else
			{
				return null;
			}
		}

		/// <summary> Get a byte associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <returns>The associated byte if key is found and has valid
		/// format, <see cref="DEFAULT_BYTE"/> otherwise.
		/// </returns>
		/// <exception cref="Exception"> is thrown if the key doesn't
		/// map to an existing object.
		/// </exception>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Byte.
		/// </exception>
		public sbyte GetByte(string key)
		{
			if (ContainsKey(key))
			{
				byte b = GetByte(key, DEFAULT_BYTE);
				return (sbyte)b;
			}
			else
			{
				throw new Exception(string.Format("{0}{1} doesn't map to an existing object", '\'', key));
			}
		}

		/// <summary> Get a byte associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <param name="defaultValue">The default value.
		/// </param>
		/// <returns>The associated byte if key is found and has valid
		/// format, default value otherwise.
		/// </returns>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Byte.
		/// </exception>
		public static sbyte GetByte(string key, sbyte defaultValue)
		{
			return GetByte(key, defaultValue);
		}

		/// <summary> Get a byte associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <param name="defaultValue">The default value.
		/// </param>
		/// <returns>The associated byte if key is found and has valid
		/// format, default value otherwise.
		/// </returns>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Byte.
		/// </exception>
		public byte GetByte(string key, byte defaultValue)
		{
			object value;
			if (!TryGetValue(key, out value))
				value = null;

			if (value is byte)
			{
				return (byte)value;
			}
			else if (value is string)
			{
				byte b = byte.Parse((string)value);
				this[key] = b;
				return b;
			}
			else if (value == null)
			{
				if (defaults == null)
				{
					return defaultValue;
				}
				else
				{
					return defaults.GetByte(key, defaultValue);
				}
			}
			else
			{
				throw new InvalidCastException(string.Format("{0}{1}' doesn't map to a Byte object", '\'', key));
			}
		}

		/// <summary> The purpose of this method is to get the configuration resource
		/// with the given name as an integer.
		/// *
		/// </summary>
		/// <param name="name">The resource name.
		/// </param>
		/// <returns>The value of the resource as an integer.
		///
		/// </returns>
		public int GetInt(string name)
		{
			return GetInteger(name);
		}

		/// <summary> The purpose of this method is to get the configuration resource
		/// with the given name as an integer, or a default value.
		/// *
		/// </summary>
		/// <param name="name">The resource name
		/// </param>
		/// <param name="def">The default value of the resource.
		/// </param>
		/// <returns>The value of the resource as an integer.
		///
		/// </returns>
		public int GetInt(string name, int def)
		{
			return GetInteger(name, def);
		}

		/// <summary> Get a int associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <returns>The associated int if key is found and has valid
		/// format, <see cref="DEFAULT_INT32"/> otherwise.
		/// </returns>
		/// <exception cref="Exception"> is thrown if the key doesn't
		/// map to an existing object.
		/// </exception>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Integer.
		/// </exception>
		public int GetInteger(string key)
		{
			return GetInteger(key, DEFAULT_INT32);
		}

		/// <summary> Get a int associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <param name="defaultValue">The default value.
		/// </param>
		/// <returns>The associated int if key is found and has valid
		/// format, <see cref="DEFAULT_INT32"/> otherwise.
		/// </returns>
		/// <returns>The associated int if key is found and has valid
		/// format, default value otherwise.
		/// </returns>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Integer.
		/// </exception>
		public int GetInteger(string key, int defaultValue)
		{
			object value;
			if (!TryGetValue(key, out value))
				value = null;

			if (value is int)
			{
				return (int)value;
			}
			else if (value is string s && int.TryParse(s, out int i))
			{
				this[key] = i;
				return i;
			}
			else if (value == null)
			{
				if (defaults == null)
				{
					return defaultValue;
				}
				else
				{
					return defaults.GetInteger(key, defaultValue);
				}
			}
			else
			{
				throw new InvalidCastException(string.Format("{0}{1}' doesn't map to a Integer object", '\'', key));
			}
		}

		/// <summary> Get a long associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <returns>The associated long if key is found and has valid
		/// format, <see cref="DEFAULT_INT64"/> otherwise.
		/// </returns>
		/// <exception cref="Exception"> is thrown if the key doesn't
		/// map to an existing object.
		/// </exception>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Long.
		/// </exception>
		public long GetLong(string key)
		{
			return GetLong(key, DEFAULT_INT64);
		}

		/// <summary> Get a long associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <param name="defaultValue">The default value.
		/// </param>
		/// <returns>The associated long if key is found and has valid
		/// format, <see cref="DEFAULT_INT64"/> otherwise.
		/// </returns>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Long.
		/// </exception>
		public long GetLong(string key, long defaultValue)
		{
			object value;
			if (!TryGetValue(key, out value))
				value = null;

			if (value is long)
			{
				return (long)value;
			}
			else if (value is string s && long.TryParse(s, out long l))
			{
				this[key] = l;
				return l;
			}
			else if (value == null)
			{
				if (defaults == null)
				{
					return defaultValue;
				}
				else
				{
					return defaults.GetLong(key, defaultValue);
				}
			}
			else
			{
				throw new InvalidCastException(string.Format("{0}{1}' doesn't map to a Long object", '\'', key));
			}
		}

		/// <summary> Get a float associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <returns>The associated float if key is found and has valid
		/// format, <see cref="DEFAULT_SINGLE"/> otherwise.
		/// </returns>
		/// <exception cref="Exception"> is thrown if the key doesn't
		/// map to an existing object.
		/// </exception>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Float.
		/// </exception>
		public float GetFloat(string key)
		{
			return GetFloat(key, DEFAULT_SINGLE);
		}

		/// <summary> Get a float associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <param name="defaultValue">The default value.
		/// </param>
		/// <returns>The associated float if key is found and has valid
		/// format, <see cref="DEFAULT_SINGLE"/> otherwise.
		/// </returns>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Float.
		/// </exception>
		public float GetFloat(string key, float defaultValue)
		{
			object value;
			if (!TryGetValue(key, out value))
				value = null;

			if (value is float)
			{
				return (float)value;
			}
			else if (value is string s && float.TryParse(s, out float f))
			{
				this[key] = f;
				return f;
			}
			else if (value == null)
			{
				if (defaults == null)
				{
					return defaultValue;
				}
				else
				{
					return defaults.GetFloat(key, defaultValue);
				}
			}
			else
			{
				throw new InvalidCastException(string.Format("{0}{1}' doesn't map to a Float object", '\'', key));
			}
		}

		/// <summary> Get a double associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <returns>The associated double if key is found and has valid
		/// format, <see cref="DEFAULT_DOUBLE"/> otherwise.
		/// </returns>
		/// <exception cref="Exception"> is thrown if the key doesn't
		/// map to an existing object.
		/// </exception>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Double.
		/// </exception>
		public double GetDouble(string key)
		{
			return GetDouble(key, DEFAULT_DOUBLE);
		}

		/// <summary> Get a double associated with the given configuration key.
		/// *
		/// </summary>
		/// <param name="key">The configuration key.
		/// </param>
		/// <param name="defaultValue">The default value.
		/// </param>
		/// <returns>The associated double if key is found and has valid
		/// format, <see cref="DEFAULT_DOUBLE"/> otherwise.
		/// </returns>
		/// <exception cref="InvalidCastException"> is thrown if the key maps to an
		/// object that is not a Double.
		/// </exception>
		public double GetDouble(string key, double defaultValue)
		{
			object value;
			if (!TryGetValue(key, out value))
				value = null; ;

			if (value is double)
			{
				return (double)value;
			}
			else if (value is string s && double.TryParse(s, out double d))
			{
				this[key] = d;
				return d;
			}
			else if (value == null)
			{
				if (defaults == null)
				{
					return defaultValue;
				}
				else
				{
					return defaults.GetDouble(key, defaultValue);
				}
			}
			else
			{
				throw new InvalidCastException(string.Format("{0}{1}' doesn't map to a Double object", '\'', key));
			}
		}

		/// <summary>
		/// Convert a standard properties class into a configuration class.
		/// </summary>
		/// <param name="p">properties object to convert into a ExtendedProperties object.</param>
		/// <returns>ExtendedProperties configuration created from the properties object.</returns>
		public static ExtendedProperties ConvertProperties(ExtendedProperties p)
		{
			ExtendedProperties c = new();

			foreach (string key in p.Keys)
			{
				object value = p.GetProperty(key);

				// if the value is a string, escape it so that if there are delimiters that the value is not converted to a list
				if (value is string)
					value = value.ToString().Replace(",", @"\,");
				c.SetProperty(key, value);
			}

			return c;
		}
	}
}