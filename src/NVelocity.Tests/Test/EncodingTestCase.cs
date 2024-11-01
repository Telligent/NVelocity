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
	using NUnit.Framework;
	using Runtime;
	using System;
	using System.IO;
	using System.Text;

	/// <summary>
	/// Tests input encoding handling.  The input target is UTF-8, having
	/// chinese and and a spanish enyay (n-twiddle)
	/// 
	/// Thanks to Kent Johnson for the example input file.
	/// </summary>
	/// <author><a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a></author>
	/// <version> $Id: EncodingTestCase.cs,v 1.4 2005/01/01 17:57:56 corts Exp $</version>
	public class EncodingTestCase : BaseTestCase
	{
		//, TemplateTestBase {

		public EncodingTestCase()
		{
			try
			{
				Velocity.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, TemplateTest.FILE_RESOURCE_LOADER_PATH);
				Velocity.SetProperty(RuntimeConstants.INPUT_ENCODING, "UTF-8");
				Velocity.Init();
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Cannot setup EncodingTestCase!");
				SupportClass.WriteStackTrace(e, Console.Error);
				Environment.Exit(1);
			}
		}

		/// <summary>
		/// Runs the test.
		/// </summary>
		public virtual void RunTest()
		{
			VelocityContext context = new();

			AssureResultsDirectoryExists(TemplateTest.RESULT_DIR);

			/*
			*  get the template and the output
			*/

			/*
			*  Chinese and spanish
			*/

			Template template = Velocity.GetTemplate(GetFileName(null, "encodingtest", TemplateTest.TMPL_FILE_EXT), "UTF-8");

			FileStream fos =
				new(GetFileName(TemplateTest.RESULT_DIR, "encodingtest", TemplateTest.RESULT_FILE_EXT), FileMode.Create);

			StreamWriter writer = new(fos);

			template.Merge(context, writer);
			writer.Flush();
			writer.Close();

			if (
				!IsMatch(TemplateTest.RESULT_DIR, TemplateTest.COMPARE_DIR, "encodingtest", TemplateTest.RESULT_FILE_EXT,
									TemplateTest.CMP_FILE_EXT))
			{
				Assert.Fail("Output 1 incorrect.");
			}

			/*
			*  a 'high-byte' chinese example from Michael Zhou
			*/

			template = Velocity.GetTemplate(GetFileName(null, "encodingtest2", TemplateTest.TMPL_FILE_EXT), "UTF-8");

			fos =
				new FileStream(GetFileName(TemplateTest.RESULT_DIR, "encodingtest2", TemplateTest.RESULT_FILE_EXT), FileMode.Create);

			writer = new StreamWriter(fos);

			template.Merge(context, writer);
			writer.Flush();
			writer.Close();

			if (
				!IsMatch(TemplateTest.RESULT_DIR, TemplateTest.COMPARE_DIR, "encodingtest2", TemplateTest.RESULT_FILE_EXT,
									TemplateTest.CMP_FILE_EXT))
			{
				Assert.Fail("Output 2 incorrect.");
			}

			/*
			*  a 'high-byte' chinese from Ilkka
			*/

			template = Velocity.GetTemplate(GetFileName(null, "encodingtest3", TemplateTest.TMPL_FILE_EXT), "GB18030");
			//GBK=936?

			fos =
				new FileStream(GetFileName(TemplateTest.RESULT_DIR, "encodingtest3", TemplateTest.RESULT_FILE_EXT), FileMode.Create);

			writer = new StreamWriter(fos, Encoding.GetEncoding("GB18030"));

			template.Merge(context, writer);
			writer.Flush();
			writer.Close();

			if (
				!IsMatch(TemplateTest.RESULT_DIR, TemplateTest.COMPARE_DIR, "encodingtest3", TemplateTest.RESULT_FILE_EXT,
									TemplateTest.CMP_FILE_EXT))
			{
				Assert.Fail("Output 3 incorrect.");
			}

			/*
			*  Russian example from Vitaly Repetenko
			*/

			template = Velocity.GetTemplate(GetFileName(null, "encodingtest_KOI8-R", TemplateTest.TMPL_FILE_EXT), "KOI8-R");
			fos =
				new FileStream(GetFileName(TemplateTest.RESULT_DIR, "encodingtest_KOI8-R", TemplateTest.RESULT_FILE_EXT),
												FileMode.Create);
			writer = new StreamWriter(fos, Encoding.GetEncoding("KOI8-R"));

			template.Merge(context, writer);
			writer.Flush();
			writer.Close();

			if (
				!IsMatch(TemplateTest.RESULT_DIR, TemplateTest.COMPARE_DIR, "encodingtest_KOI8-R", TemplateTest.RESULT_FILE_EXT,
									TemplateTest.CMP_FILE_EXT))
			{
				Assert.Fail("Output 4 incorrect.");
			}


			/*
			*  ISO-8859-1 example from Mike Bridge
			*/

			template =
				Velocity.GetTemplate(GetFileName(null, "encodingtest_ISO-8859-1", TemplateTest.TMPL_FILE_EXT), "ISO-8859-1");
			fos =
				new FileStream(GetFileName(TemplateTest.RESULT_DIR, "encodingtest_ISO-8859-1", TemplateTest.RESULT_FILE_EXT),
												FileMode.Create);
			writer = new StreamWriter(fos, Encoding.GetEncoding("ISO-8859-1"));

			template.Merge(context, writer);
			writer.Flush();
			writer.Close();

			if (
				!IsMatch(TemplateTest.RESULT_DIR, TemplateTest.COMPARE_DIR, "encodingtest_ISO-8859-1", TemplateTest.RESULT_FILE_EXT,
									TemplateTest.CMP_FILE_EXT))
			{
				Assert.Fail("Output 5 incorrect.");
			}
		}
	}
}