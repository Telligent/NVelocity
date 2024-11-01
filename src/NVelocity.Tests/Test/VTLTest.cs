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
	using System;
	using System.IO;

	/// <summary>
	/// Test Velocity processing
	/// </summary>
	[TestFixture]
	public class VTLTest
	{
		public class A
		{
			public string T1
			{
				get { return "0"; }
			}
		}

		[Test]
		[Ignore("Known issues from version 1.3x and will be resolved with next parser port")]
		public void VTLTest1()
		{
			//	    VelocityCharStream vcs = new VelocityCharStream(new StringReader(":=t#${A.T1}ms"), 1, 1);
			//	    Parser p = new Parser(vcs);
			//	    SimpleNode root = p.process();
			//
			//	    String nodes = String.Empty;
			//	    if (root != null) {
			//		Token t = root.FirstToken;
			//		nodes += t.kind.ToString();
			//		while (t != root.LastToken) {
			//		    t = t.next;
			//		    nodes += "," + t.kind.ToString();
			//		}
			//	    }
			//
			//	    throw new System.Exception(nodes);

			VelocityEngine velocityEngine = new();
			velocityEngine.Init();

			VelocityContext c = new();
			c.Put("A", new A());

			// modified version so Bernhard could continue
			StringWriter sw = new();
												bool ok = velocityEngine.Evaluate(c, sw, "VTLTest1", "#set($hash = \"#\"):=t${hash}${A.T1}ms");
			Assert.IsTrue(ok, "Evaluation returned failure");
			Assert.AreEqual(":=t#0ms", sw.ToString());

			// the actual problem reported
			sw = new StringWriter();
			ok = velocityEngine.Evaluate(c, sw, "VTLTest1", ":=t#${A.T1}ms");
			Assert.IsTrue(ok, "Evaluation returned failure");
			Assert.AreEqual(":=t#0ms", sw.ToString());
		}

		[Test]
		public void HashBeforeExplicitVariableDuplication()
		{
			VelocityEngine velocityEngine = new();
			velocityEngine.Init();

			VelocityContext c = new();
			c.Put("A", new A());

			StringWriter sw = new();
												bool ok = velocityEngine.Evaluate(c, sw, "HashBeforeExplicitVariableDuplication", "#${A.T1}_2");
			Assert.IsTrue(ok, "Evaluation returned failure");
			Assert.AreEqual("#0_2", sw.ToString());
		}
	}
}