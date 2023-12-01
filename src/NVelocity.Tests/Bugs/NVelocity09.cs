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
namespace NVelocity.Tests.Bugs
{
	using App;
	using Commons.Collections;
	using NUnit.Framework;
	using Runtime;
	using System.IO;
	using Test;

	[SetUpFixture, Ignore("Won't fix.")]
	public class NVelocity09 : BaseTestCase
	{
		[Test]
		public void Test()
		{
			var velocityEngine = new VelocityEngine();

			ExtendedProperties extendedProperties = new();
			extendedProperties.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, TemplateTest.FILE_RESOURCE_LOADER_PATH);

			velocityEngine.Init(extendedProperties);

			VelocityContext context = new();

			Template template = velocityEngine.GetTemplate(
				GetFileName(null, "nv09", TemplateTest.TMPL_FILE_EXT));

			StringWriter writer = new();

			template.Merge(context, writer);
		}
	}
}
