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

namespace NVelocity.Test
{
	using App;
	using global::Commons.Collections;
	using NUnit.Framework;
	using Provider;
	using Runtime;
	using System;
	using System.Collections;
	using System.IO;


	/// <summary>
	/// Easily add test cases which evaluate templates and check their output.
	///
	/// NOTE:
	/// This class DOES NOT extend RuntimeTestCase because the TemplateTestSuite
	/// already initializes the Velocity runtime and adds the template
	/// test cases. Having this class extend RuntimeTestCase causes the
	/// Runtime to be initialized twice which is not good. I only discovered
	/// this after a couple hours of wondering why all the properties
	/// being setup were ending up as Vectors. At first I thought it
	/// was a problem with the Configuration class, but the Runtime
	/// was being initialized twice: so the first time the property
	/// is seen it's stored as a String, the second time it's seen
	/// the Configuration class makes a Vector with both Strings.
	/// As a result all the getBoolean(property) calls were failing because
	/// the Configurations class was trying to create a Boolean from
	/// a Vector which doesn't really work that well. I have learned
	/// my lesson and now have to add some code to make sure the
	/// Runtime isn't initialized more then once :-)
	/// </summary>
	[TestFixture]
	public class TemplateTestCase : BaseTestCase
	{
		private readonly ExtendedProperties testProperties;

		private TestProvider provider;
		private ArrayList al;
		private Hashtable h;
		private VelocityContext context;
		private VelocityContext context1;
		private VelocityContext context2;
		private ArrayList vec;

		private readonly VelocityEngine velocityEngine;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public TemplateTestCase()
		{
			try
			{
				velocityEngine = new VelocityEngine();

				ExtendedProperties extendedProperties = new();
				extendedProperties.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, TemplateTest.FILE_RESOURCE_LOADER_PATH);

				extendedProperties.SetProperty(RuntimeConstants.RUNTIME_LOG_ERROR_STACKTRACE, "true");
				extendedProperties.SetProperty(RuntimeConstants.RUNTIME_LOG_WARN_STACKTRACE, "true");
				extendedProperties.SetProperty(RuntimeConstants.RUNTIME_LOG_INFO_STACKTRACE, "true");

				velocityEngine.Init(extendedProperties);

				testProperties = new ExtendedProperties();
				testProperties.Load(new FileStream(TemplateTest.TEST_CASE_PROPERTIES, FileMode.Open, FileAccess.Read));
			}
			catch (Exception ex)
			{
				throw new Exception("Cannot setup TemplateTestSuite!", ex);
			}
		}

		/// <summary>
		/// Sets up the test.
		/// </summary>
		[SetUp]
		protected void SetUp()
		{
			provider = new TestProvider();
			al = provider.Customers;
			h = new Hashtable();

			h["Bar"] = "this is from a hashtable!";
			h["Foo"] = "this is from a hashtable too!";

			/*
			*  lets set up a vector of objects to test late introspection. See ASTMethod.java
			*/

			vec = new ArrayList
			{
					new string("string1".ToCharArray()),
					new string("string2".ToCharArray())
			};

			/*
			*  set up 3 chained contexts, and add our data 
			*  through the 3 of them.
			*/

			context2 = new VelocityContext();
			context1 = new VelocityContext(context2);
			context = new VelocityContext(context1);

			context.Put("provider", provider);
			context1.Put("name", "jason");
			context2.Put("providers", provider.Customers2);
			context.Put("list", al);
			context1.Put("hashtable", h);
			context2.Put("hashmap", new Hashtable());
			context2.Put("search", provider.Search);
			context.Put("relatedSearches", provider.RelSearches);
			context1.Put("searchResults", provider.RelSearches);
			context2.Put("stringarray", provider.Array);
			context.Put("vector", vec);
			context.Put("mystring", new string(string.Empty.ToCharArray()));
			context.Put("runtime", new FieldMethodizer("NVelocity.Runtime.RuntimeSingleton"));
			context.Put("fmprov", new FieldMethodizer(provider));
			context.Put("Floog", "floogie woogie");
			context.Put("boolobj", new BoolObj());

												/*
												*  we want to make sure we test all types of iterative objects
												*  in #foreach()
												*/

												object[] oarr = new object[] { "a", "b", "c", "d" };
			int[] intarr = new int[] { 10, 20, 30, 40, 50 };

			context.Put("collection", vec);
			context2.Put("iterator", vec.GetEnumerator());
			context1.Put("map", h);
			context.Put("obarr", oarr);
			context.Put("enumerator", vec.GetEnumerator());
			context.Put("intarr", intarr);
		}

		[Test]
		public void CacheProblems()
		{
			VelocityContext context = new();

			context.Put("AjaxHelper2", new AjaxHelper2());
			context.Put("DictHelper", new DictHelper());

			Template template = velocityEngine.GetTemplate(
				GetFileName(null, "dicthelper", TemplateTest.TMPL_FILE_EXT));

			StringWriter writer = new();

			template.Merge(context, writer);

			System.Console.WriteLine(writer.GetStringBuilder().ToString());

			writer = new StringWriter();

			template.Merge(context, writer);

			System.Console.WriteLine(writer.GetStringBuilder().ToString());

			writer = new StringWriter();

			template.Merge(context, writer);

			System.Console.WriteLine(writer.GetStringBuilder().ToString());
		}

		/// <summary>
		/// Adds the template test cases to run to this test suite.  Template test
		/// cases are listed in the <code>TEST_CASE_PROPERTIES</code> file.
		/// </summary>
		[Test]
		public void Test_Run()
		{
												string template;
												bool allpass = true;
												int failures = 0;
			for (int i = 1; ; i++)
			{
				template = testProperties.GetString(getTemplateTestKey(i));

				if (template != null)
				{
					bool pass = RunTest(template);
					if (!pass)
					{
						Console.Out.Write("Adding TemplateTestCase : " + template + "...");
						Console.Out.WriteLine("FAIL!");
						allpass = false;
						failures++;
					}
				}
				else
				{
					// Assume we're done adding template test cases.
					break;
				}
			}

			if (!allpass)
			{
				Assert.Fail(failures.ToString() + " templates failed");
			}
		}

		/// <summary>
		/// Macro which returns the properties file key for the specified template
		/// test number.
		/// </summary>
		/// <param name="nbr">The template test number to return a property key for.</param>
		/// <returns>The property key.</returns>
		private static string getTemplateTestKey(int nbr)
		{
			return ("test.template." + nbr);
		}

		/// <summary>
		/// Runs the test.
		/// </summary>
		private bool RunTest(string baseFileName)
		{
			// run setup before each test so that the context is clean
			SetUp();

			try
			{
				Template template = velocityEngine.GetTemplate(GetFileName(null, baseFileName, TemplateTest.TMPL_FILE_EXT));

				AssureResultsDirectoryExists(TemplateTest.RESULT_DIR);

				/* get the file to write to */
				FileStream fos =
					new(GetFileName(TemplateTest.RESULT_DIR, baseFileName, TemplateTest.RESULT_FILE_EXT), FileMode.Create);

				StreamWriter writer = new(fos);

				/* process the template */
				template.Merge(context, writer);

				/* close the file */
				writer.Flush();
				writer.Close();

				if (!IsMatch(TemplateTest.RESULT_DIR, TemplateTest.COMPARE_DIR, baseFileName,
											TemplateTest.RESULT_FILE_EXT, TemplateTest.CMP_FILE_EXT))
				{
					//Fail("Processed template did not match expected output");
					return false;
				}
			}
			catch (Exception)
			{
				Console.WriteLine("Test {0} failed", baseFileName);

				throw;
			}

			return true;
		}
	}
}