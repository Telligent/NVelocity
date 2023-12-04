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

namespace NVelocity.Runtime.Directive
{
	using Context;
	using NVelocity.Runtime.Parser.Node;
	using NVelocity.Util.Introspection;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
				using System.Linq;

				/// <summary>
				/// Foreach directive used for moving through arrays,
				/// or objects that provide an Iterator.
				/// </summary>
				public class Foreach : Directive
	{
		private static readonly string[] SectionNames;

		static Foreach()
		{
			SectionNames = ForeachSectionEnum.GetNames(typeof(ForeachSectionEnum));

			Array.Sort(SectionNames, CaseInsensitiveComparer.Default);

			for (int i = 0; i < SectionNames.Length; i++)
			{
				SectionNames[i] = SectionNames[i].ToLower();
			}
		}

		private List<INode>[] sections;

		/// <summary>
		/// The name of the variable to use when placing
		/// the counter value into the context. Right
		/// now the default is $velocityCount.
		/// </summary>
		private string counterName;

		/// <summary>
		/// What value to start the loop counter at.
		/// </summary>
		private int counterInitialValue;

		/// <summary>
		/// The reference name used to access each
		/// of the elements in the list object. It
		/// is the $item in the following:
		///
		/// #foreach ($item in $list)
		///
		/// This can be used class wide because
		/// it is immutable.
		/// </summary>
		private string elementKey;


		/// <summary>
		/// Return name of this directive.
		/// </summary>
		public override string Name
		{
			get { return "foreach"; }
			set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Return type of this directive.
		/// </summary>
		public override DirectiveType Type
		{
			get { return DirectiveType.BLOCK; }
		}

		public override bool SupportsNestedDirective(string name)
		{
			int index = Array.BinarySearch(SectionNames, name.ToLower());

			return index >= 0;
		}

		public override Directive CreateNestedDirective(string name)
		{
			name = name.ToLower();

			if (name == "between")
			{
				return new ForeachBetweenSection();
			}
			else if (name == "odd")
			{
				return new ForeachOddSection();
			}
			else if (name == "even")
			{
				return new ForeachEvenSection();
			}
			else if (name == "nodata")
			{
				return new ForeachNoDataSection();
			}
			else if (name == "before")
			{
				return new ForeachBeforeSection();
			}
			else if (name == "after")
			{
				return new ForeachAfterSection();
			}
			else if (name == "beforeall")
			{
				return new ForeachBeforeAllSection();
			}
			else if (name == "afterall")
			{
				return new ForeachAfterAllSection();
			}
			else if (name == "each")
			{
				return new ForeachEachSection();
			}

			throw new NotSupportedException(string.Format("Foreach directive error: Nested directive not supported: {0}", name));
		}

		/// <summary>  
		/// simple init - init the tree and get the elementKey from
		/// the AST
		/// </summary>
		public override void Init(IRuntimeServices rs, IInternalContextAdapter context, INode node)
		{
			base.Init(rs, context, node);

			counterName = runtimeServices.GetString(RuntimeConstants.COUNTER_NAME);
			counterInitialValue = runtimeServices.GetInt(RuntimeConstants.COUNTER_INITIAL_VALUE);

			// this is really the only thing we can do here as everything
			// else is context sensitive
			elementKey = node.GetChild(0).FirstToken.Image[1..];

			PrepareSections(node.GetChild(3));
		}

		/// <summary>
		/// returns an Iterator to the collection in the #foreach()
		/// </summary>
		/// <param name="context"> current context </param>
		/// <param name="node">  AST node </param>
		/// <returns>Iterator to do the dataset </returns>
		private IEnumerator GetIterator(IInternalContextAdapter context, INode node)
		{
			// get our list object, and punt if it's null.
			object listObject = node.GetChild(2).Value(context);

			// if we have an event cartridge, get a new value object
			NVelocity.App.Events.EventCartridge eventCartridge = context.EventCartridge;
			if (eventCartridge != null)
			{
				listObject = eventCartridge.ReferenceInsert(new Stack<object>(), node.GetChild(2).Literal, listObject);
			}

			if (listObject == null)
			{
				return null;
			}

			if (listObject is IEnumerable enumerable)
			{
				return enumerable.GetEnumerator();
			}
			else
			{
				runtimeServices.Warn(
								string.Format(
										"Could not determine type of enumerator ({0}) in #foreach loop for {1} at [{2},{3}] in template {4}",
										listObject.GetType().Name, node.GetChild(2).FirstToken.Image, Line, Column, context.CurrentTemplateName));

				return null;
			}
		}

		/// <summary>
		/// renders the #foreach() block
		/// </summary>
		public override bool Render(IInternalContextAdapter context, TextWriter writer, INode node)
		{
			// do our introspection to see what our collection is
			IEnumerator enumerator = GetIterator(context, node);
			INode bodyNode = node.GetChild(3);

			bool isFancyLoop = (sections != null);

			if (enumerator == null && !isFancyLoop)
			{
				return true;
			}

			int counter = counterInitialValue;

			// save the element key if there is one,
			// and the loop counter
			object o = context.Get(elementKey);
			object ctr = context.Get(counterName);

			if (enumerator != null && enumerator.MoveNext())
			{
				do
				{
					object current = enumerator.Current;

					context.Put(counterName, counter);
					//context.Put(hasNextName, enumerator.MoveNext() ? Boolean.TrueString : Boolean.FalseString);
					context.Put(elementKey, current);

					try
					{
						if (isFancyLoop)
						{
							if (counter == counterInitialValue)
							{
								ProcessSection(ForeachSectionEnum.BeforeAll, context, writer);
							}
							else
							{
								ProcessSection(ForeachSectionEnum.Between, context, writer);
							}

							ProcessSection(ForeachSectionEnum.Before, context, writer);

							// since 1st item is zero we invert odd/even
							if ((counter - counterInitialValue) % 2 == 0)
							{
								ProcessSection(ForeachSectionEnum.Odd, context, writer);
							}
							else
							{
								ProcessSection(ForeachSectionEnum.Even, context, writer);
							}

							ProcessSection(ForeachSectionEnum.Each, context, writer);

							ProcessSection(ForeachSectionEnum.After, context, writer);
						}
						else
						{
							bodyNode.Render(context, writer);
						}
					}
					catch (BreakException)
					{
						break;
					}
					counter++;
				} while (enumerator.MoveNext());
			}

			if (isFancyLoop)
			{
				if (counter > counterInitialValue)
				{
					ProcessSection(ForeachSectionEnum.AfterAll, context, writer);
				}
				else
				{
					ProcessSection(ForeachSectionEnum.NoData, context, writer);
				}
			}

			// restores the loop counter (if we were nested)
			// if we have one, else just removes
			if (ctr == null)
			{
				context.Remove(counterName);
			}
			else
			{
				context.Put(counterName, ctr);
			}

			// restores element key if exists
			// otherwise just removes
			if (o == null)
			{
				context.Remove(elementKey);
			}
			else
			{
				context.Put(elementKey, o);
			}

			return true;
		}

		private void ProcessSection(ForeachSectionEnum sectionEnumType, IInternalContextAdapter context, TextWriter writer)
		{
			int sectionIndex = (int)sectionEnumType;

			if (sections[sectionIndex] == null)
			{
				return;
			}

			foreach (INode node in sections[sectionIndex])
			{
				node.Render(context, writer);
			}
		}

		private void PrepareSections(INode node)
		{
			bool isFancyLoop = false;

			int curSection = (int)ForeachSectionEnum.Each;

			var sections = new List<INode>[SectionNames.Length];

			int nodeCount = node.ChildrenCount;

			for (int i = 0; i < nodeCount; i++)
			{
				INode childNode = node.GetChild(i);

				if (childNode is ASTDirective directive && Array.BinarySearch(SectionNames, directive.DirectiveName) > -1)
				{
					isFancyLoop = true;
					curSection = (int)ForeachSectionEnum.Parse(typeof(ForeachSectionEnum), directive.DirectiveName, true);
				}
				else
				{
					if (sections[curSection] == null)
						sections[curSection] = new();

					sections[curSection].Add(childNode);
				}
			}

			if (!isFancyLoop)
			{
				this.sections = null;
			}
			else
			{
				this.sections = sections;
			}
		}
	}

	public enum ForeachSectionEnum
	{
		Each = 0,
		Between = 1,
		Odd = 2,
		Even = 3,
		NoData = 4,
		BeforeAll = 5,
		AfterAll = 6,
		Before = 7,
		After = 8
	}

	public interface IForeachSection
	{
		ForeachSectionEnum Section { get; }
	}

	public abstract class AbstractForeachSection : Directive, IForeachSection
	{
		public override string Name
		{
			get { return Section.ToString().ToLower(); }
			set { throw new NotImplementedException(); }
		}

		public override bool AcceptParams
		{
			get { return false; }
		}

		public override DirectiveType Type
		{
			get { return DirectiveType.LINE; }
		}

		public override bool Render(IInternalContextAdapter context, TextWriter writer, INode node)
		{
			return true;
		}

		public abstract ForeachSectionEnum Section { get; }
	}

	public class ForeachEachSection : AbstractForeachSection
	{
		public override ForeachSectionEnum Section
		{
			get { return ForeachSectionEnum.Each; }
		}
	}

	public class ForeachBetweenSection : AbstractForeachSection
	{
		public override ForeachSectionEnum Section
		{
			get { return ForeachSectionEnum.Between; }
		}
	}

	public class ForeachOddSection : AbstractForeachSection
	{
		public override ForeachSectionEnum Section
		{
			get { return ForeachSectionEnum.Odd; }
		}
	}

	public class ForeachEvenSection : AbstractForeachSection
	{
		public override ForeachSectionEnum Section
		{
			get { return ForeachSectionEnum.Even; }
		}
	}

	public class ForeachNoDataSection : AbstractForeachSection
	{
		public override ForeachSectionEnum Section
		{
			get { return ForeachSectionEnum.NoData; }
		}
	}

	public class ForeachBeforeSection : AbstractForeachSection
	{
		public override ForeachSectionEnum Section
		{
			get { return ForeachSectionEnum.Before; }
		}
	}

	public class ForeachAfterSection : AbstractForeachSection
	{
		public override ForeachSectionEnum Section
		{
			get { return ForeachSectionEnum.After; }
		}
	}

	public class ForeachBeforeAllSection : AbstractForeachSection
	{
		public override ForeachSectionEnum Section
		{
			get { return ForeachSectionEnum.BeforeAll; }
		}
	}

	public class ForeachAfterAllSection : AbstractForeachSection
	{
		public override ForeachSectionEnum Section
		{
			get { return ForeachSectionEnum.AfterAll; }
		}
	}
}