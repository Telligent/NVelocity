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
// 
namespace NVelocity.Test.Provider
{
	using System;
	using System.Collections;
	using System.Text;

	/// <summary>
	/// This class is used by the testbed. Instances of the class
	/// are fed into the context that is set before the AST
	/// is traversed and dynamic content generated.
	/// </summary>
	/// <author><a href="mailto:jvanzyl@apache.org">Jason van Zyl</a></author>
	public class TestProvider
	{
		public static string PUB_STAT_STRING = "Public Static String";

		internal string title = "lunatic";
		//internal bool state;
		internal object ob;
		internal int stateint;

		public virtual string Name
		{
			get { return "jason"; }
		}

		public virtual Stack Stack
		{
			get
			{
				Stack stack = new();
																object temp_object;
				temp_object = "stack element 1";
				stack.Push(temp_object);
																object temp_object2;
				temp_object2 = "stack element 2";
				stack.Push(temp_object2);
																object temp_object3;
				temp_object3 = "stack element 3";
				stack.Push(temp_object3);
				return stack;
			}
		}

		public virtual IList EmptyList
		{
			get
			{
				IList list = new ArrayList();
				return list;
			}
		}

		public virtual IList List
		{
			get
			{
				IList list = new ArrayList
				{
						"list element 1",
						"list element 2",
						"list element 3"
				};

				return list;
			}
		}

		public virtual Hashtable Search
		{
			get
			{
				Hashtable h = new()
				{
						{ "Text", "this is some text" },
						{ "EscText", "this is escaped text" },
						{ "Title", "this is the title" },
						{ "Index", "this is the index" },
						{ "URL", "http://periapt.com" }
				};

				ArrayList al = new()
				{
						h
				};

				h.Add("RelatedLinks", al);

				return h;
			}
		}

		public virtual Hashtable Hashtable
		{
			get
			{
				Hashtable h = new();
				h["key0"] = "value0";
				h["key1"] = "value1";
				h["key2"] = "value2";

				return h;
			}
		}

		public virtual ArrayList RelSearches
		{
			get
			{
				ArrayList al = new()
				{
						Search
				};

				return al;
			}
		}

		public virtual string Title
		{
			get { return title; }

			set { title = value; }
		}

		public virtual object[] Menu
		{
			get
			{
																//ArrayList al = new ArrayList();
																object[] menu = new object[3];
				for (int i = 0; i < 3; i++)
				{
					Hashtable item = new();
					item["id"] = "item" + Convert.ToString(i + 1);
					item["name"] = "name" + Convert.ToString(i + 1);
					item["label"] = "label" + Convert.ToString(i + 1);
					//al.Add(item);
					menu[i] = item;
				}

				//return al;
				return menu;
			}
		}

		public ArrayList getCustomers()
		{
			return Customers;
		}


		public virtual ArrayList Customers
		{
			get
			{
				ArrayList list = new()
				{
						"ArrayList element 1",
						"ArrayList element 2",
						"ArrayList element 3",
						"ArrayList element 4"
				};

				return list;
			}
		}

		public virtual ArrayList Customers2
		{
			get
			{
				ArrayList list = new()
				{
						new TestProvider(),
						new TestProvider(),
						new TestProvider(),
						new TestProvider()
				};

				return list;
			}
		}

		public virtual ArrayList Vector
		{
			get
			{
				ArrayList list = new()
				{
						"vector element 1",
						"vector element 2"
				};

				return list;
			}
		}

		public virtual string[] Array
		{
			get
			{
																string[] strings = new string[2];
				strings[0] = "first element";
				strings[1] = "second element";
				return strings;
			}
		}

		public virtual bool StateTrue
		{
			get { return true; }
		}

		public virtual bool StateFalse
		{
			get { return false; }
		}

		public virtual Person Person
		{
			get { return new Person(); }
		}

		public virtual Child Child
		{
			get { return new Child(); }
		}

		public virtual bool State
		{
			set { }
		}

		public virtual int BangStart
		{
			set
			{
				Console.Out.WriteLine("SetBangStart() : called with val = " + value);
				stateint = value;
			}
		}

		public virtual string Foo
		{
			get
			{
				Console.Out.WriteLine("Hello from getfoo");
				throw new Exception("From getFoo()");
			}
		}

		public virtual string Throw
		{
			get
			{
				Console.Out.WriteLine("Hello from geThrow");
				throw new Exception("From getThrow()");
			}
		}

		public string getTitleMethod()
		{
			return title;
		}


		public virtual object me()
		{
			return this;
		}

		public override string ToString()
		{
			return ("test provider");
		}


		public virtual bool theAPLRules()
		{
			return true;
		}


		public virtual string objectArrayMethod(object[] o)
		{
			return "result of objectArrayMethod";
		}

		public virtual string concat(object[] strings)
		{
			StringBuilder result = new();

			for (int i = 0; i < strings.Length; i++)
			{
				result.Append((string)strings[i]).Append(' ');
			}

			return result.ToString();
		}

		public virtual string concat(IList strings)
		{
			StringBuilder result = new();

			for (int i = 0; i < strings.Count; i++)
			{
				result.Append((string)strings[i]).Append(' ');
			}

			return result.ToString();
		}

		public virtual string objConcat(IList objects)
		{
			StringBuilder result = new();

			for (int i = 0; i < objects.Count; i++)
			{
				result.Append(objects[i]).Append(' ');
			}

			return result.ToString();
		}

		public virtual string parse(string a, object o, string c, string d)
		{
			//UPGRADE_TODO: The equivalent in .NET for method 'java.Object.toString' may return a different value. 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="jlca1043"'
			return a + o + c + d;
		}

		public virtual string concat(string a, string b)
		{
			return a + b;
		}

		// These two are for testing subclasses.


		public virtual string showPerson(Person person)
		{
			return person.Name;
		}

		/// <summary> Chop i characters off the end of a string.
		/// *
		/// </summary>
		/// <param name="str">String to chop.
		/// </param>
		/// <param name="i">Number of characters to chop.
		/// </param>
		/// <returns>String with processed answer.
		///
		/// </returns>
		public virtual string chop(string str, int i)
		{
			return (str[..^i]);
		}

		public virtual bool allEmpty(object[] list)
		{
			int size = list.Length;

			for (int i = 0; i < size; i++)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.Object.toString' may return a different value. 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="jlca1043"'
				if (list[i].ToString().Length > 0)
					return false;
			}

			return true;
		}

		/*
	* This can't have the signature

	public void setState(boolean state)

	or dynamically invoking the method
	doesn't work ... you would have to
	put a wrapper around a method for a
	real boolean property that takes a 
	Boolean object if you wanted this to
	work. Not really sure how useful it
	is anyway. Who cares about boolean
	values you can just set a variable.

	*/


		public virtual int bang()
		{
			Console.Out.WriteLine("Bang! : " + stateint);
												int ret = stateint;
			stateint++;
			return ret;
		}

		/// <summary> Test the ability of vel to use a get(key)
		/// method for any object type, not just one
		/// that implements the Map interface.
		/// </summary>
		public virtual string get
			(string key)
		{
			return key;
		}


		public string this[string key]
		{
			get { return key; }
		}


		/// <summary> Test the ability of vel to use a put(key)
		/// method for any object type, not just one
		/// that implements the Map interface.
		/// </summary>
		public virtual string put(string key, object o)
		{
			ob = o;
			return key;
		}
	}
}