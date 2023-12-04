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
	using NUnit.Framework;
	using Runtime;
	using System;
	using System.Collections;
	using System.IO;
	using System.Text;
	using Util;

	/// <summary>
	/// This is a base interface that contains a bunch of static final
	/// strings that are of use when testing templates.
	/// </summary>
	/// <author> <a href="mailto:jon@latchkey.com">Jon S. Stevens</a></author>
	public struct TemplateTest
	{
		/// <summary>
		/// VTL file extension.
		/// </summary>
		public const string TMPL_FILE_EXT = "vm";

		/// <summary>
		/// Comparison file extension.
		/// </summary>
		public const string CMP_FILE_EXT = "cmp";

		/// <summary>
		/// Comparison file extension.
		/// </summary>
		public const string RESULT_FILE_EXT = "res";

		/// <summary>
		/// Path for templates. This property will override the
		/// value in the default velocity properties file.
		/// </summary>
		public static readonly string FILE_RESOURCE_LOADER_PATH;

		/// <summary>
		/// Properties file that lists which template tests to run.
		/// </summary>
		public static readonly string TEST_CASE_PROPERTIES;

		/// <summary>
		/// Results relative to the build directory.
		/// </summary>
		public static readonly string RESULT_DIR;

		/// <summary>
		/// Results relative to the build directory.
		/// </summary>
		public static readonly string COMPARE_DIR;

		static TemplateTest()
		{
			FILE_RESOURCE_LOADER_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("../../../", "templates"));
			TEST_CASE_PROPERTIES = Path.Combine(FILE_RESOURCE_LOADER_PATH, "templates.properties");
			RESULT_DIR = Path.Combine(FILE_RESOURCE_LOADER_PATH, "results");
			COMPARE_DIR = Path.Combine(FILE_RESOURCE_LOADER_PATH, "compare");
		}
	}

	/// <summary>
	/// Base test case that provides a few utility methods for
	/// the rest of the tests.
	/// </summary>
	/// <author> <a href="mailto:dlr@finemaltcoding.com">Daniel Rall</a></author>
	public class BaseTestCase
	{
		/// <summary>
		/// Concatenates the file name parts together appropriately.
		/// </summary>
		/// <returns>
		/// The full path to the file.
		/// </returns>
		protected internal static string GetFileName(string dir, string baseDir, string ext)
		{
			StringBuilder buf = new();
			if (dir != null)
				buf.Append(dir).Append('/');
			buf.Append(baseDir).Append('.').Append(ext);
			return buf.ToString();
		}

		/// <summary>
		/// Assures that the results directory exists.  If the results directory
		/// cannot be created, fails the test.
		/// </summary>
		protected internal static void AssureResultsDirectoryExists(string resultsDirectory)
		{
			FileInfo dir = new(resultsDirectory);
			bool tmpBool;
			if (File.Exists(dir.FullName))
				tmpBool = true;
			else
				tmpBool = Directory.Exists(dir.FullName);
			if (!tmpBool)
			{
				RuntimeSingleton.Info("Template results directory does not exist");
																bool ok = true;
				try
				{
					Directory.CreateDirectory(dir.FullName);
				}
				catch (Exception)
				{
					ok = false;
				}

				if (ok)
				{
					RuntimeSingleton.Info("Created template results directory");
				}
				else
				{
																				string errMsg = "Unable to create template results directory";
					RuntimeSingleton.Warn(errMsg);
					Assert.Fail(errMsg);
				}
			}
		}


		/// <summary>
		/// Normalizes lines to account for platform differences.  Macs use
		/// a single \r, DOS derived operating systems use \r\n, and Unix
		/// uses \n.  Replace each with a single \n.
		/// </summary>
		/// <author> <a href="mailto:rubys@us.ibm.com">Sam Ruby</a>
		/// </author>
		/// <returns>
		/// source with all line terminations changed to Unix style
		/// </returns>
		protected internal virtual string NormalizeNewlines(string source)
		{
			return source.Replace(Environment.NewLine, "|").Replace("\n", "|");
		}

		/// <summary>
		/// Returns whether the processed template matches the
		/// content of the provided comparison file.
		/// </summary>
		/// <returns>
		/// Whether the output matches the contents
		/// of the comparison file.
		/// </returns>
		protected internal virtual bool IsMatch(string resultsDir, string compareDir, string baseFileName, string resultExt,
																						string compareExt)
		{
												bool SHOW_RESULTS = true;

												string result = StringUtils.FileContentsToString(GetFileName(resultsDir, baseFileName, resultExt));
												string compare = StringUtils.FileContentsToString(GetFileName(compareDir, baseFileName, compareExt));

												string s1 = NormalizeNewlines(result);
												string s2 = NormalizeNewlines(compare);

												bool equals = s1.Equals(s2);
			if (!equals && SHOW_RESULTS)
			{
																string[] cmp = compare.Split(Environment.NewLine.ToCharArray());
																string[] res = result.Split(Environment.NewLine.ToCharArray());

				IEnumerator cmpi = cmp.GetEnumerator();
				IEnumerator resi = res.GetEnumerator();
																int line = 0;
				while (cmpi.MoveNext() && resi.MoveNext())
				{
					line++;
					if (!cmpi.Current.Equals(resi.Current))
					{
						Console.Out.WriteLine(line + " : " + cmpi.Current);
						Console.Out.WriteLine(line + " : " + resi.Current);
					}
				}
			}

			return equals;
		}

		/// <summary>
		/// Turns a base file name into a test case name.
		/// </summary>
		/// <param name="s">
		/// The base file name.
		/// </param>
		/// <returns>
		/// The test case name.
		/// </returns>
		protected internal static string GetTestCaseName(string s)
		{
			StringBuilder name = new();
			name.Append(char.ToUpper(s[0]));
			name.Append(s[1..].ToLower());
			return name.ToString();
		}
	}
}