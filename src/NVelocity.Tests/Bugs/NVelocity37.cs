﻿// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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
namespace NVelocity.Tests.Bugs
{
	using App;
	using Commons.Collections;
	using NUnit.Framework;
	using Runtime;
	using System;
	using System.IO;
	using Test;

	[TestFixture]
	public class NVelocity37 : BaseTestCase
	{
		[Test]
		public void Test()
		{
			var velocityEngine = new VelocityEngine();

			ExtendedProperties extendedProperties = new();
			extendedProperties.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, TemplateTest.FILE_RESOURCE_LOADER_PATH);

			velocityEngine.Init(extendedProperties);

			VelocityContext context = new();

			context.Put("yada", "line");

			Template template = velocityEngine.GetTemplate(
				GetFileName(null, "nv37", TemplateTest.TMPL_FILE_EXT));

			StringWriter writer = new();

#pragma warning disable 612, 618
			velocityEngine.MergeTemplate("nv37.vm", context, writer);
#pragma warning restore 612, 618

			//template.Merge(context, writer);

			Console.WriteLine(writer);
		}
	}
}
