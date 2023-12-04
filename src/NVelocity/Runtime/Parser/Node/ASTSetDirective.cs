namespace NVelocity.Runtime.Parser.Node
{
	using Context;
	using NVelocity.App.Events;
	using System;
	using System.IO;

	/// <summary> 
	/// Node for the #set directive
	/// </summary>
	public class ASTSetDirective : SimpleNode
	{
		private string leftReference = string.Empty;
		private INode right;
		private ASTReference left;
		internal bool blather = false;

		public ASTSetDirective(int id) : base(id)
		{
		}

		public ASTSetDirective(Parser p, int id) : base(p, id)
		{
		}

		private ASTReference LeftHandSide
		{
			get { return (ASTReference)GetChild(0).GetChild(0).GetChild(0); }
		}

		private INode RightHandSide
		{
			get { return GetChild(0).GetChild(0).GetChild(1).GetChild(0); }
		}

		/// <summary>Accept the visitor. *
		/// </summary>
		public override object Accept(IParserVisitor visitor, object data)
		{
			return visitor.Visit(this, data);
		}

		/// <summary>  simple init.  We can get the RHS and LHS as the the tree structure is static
		/// </summary>
		public override object Init(IInternalContextAdapter context, object data)
		{
			/*
			*  init the tree correctly
			*/

			base.Init(context, data);

			right = RightHandSide;
			left = LeftHandSide;

			blather = runtimeServices.GetBoolean(RuntimeConstants.RUNTIME_LOG_REFERENCE_LOG_INVALID, true);

			/*
			*  grab this now.  No need to redo each time
			*/
			leftReference = left.FirstToken.Image[1..];

			return data;
		}

		/// <summary>   puts the value of the RHS into the context under the key of the LHS
		/// </summary>
		public override bool Render(IInternalContextAdapter context, TextWriter writer)
		{
			/*
			*  get the RHS node, and it's value
			*/

			object value = right.Value(context);

			/*
			* it's an error if we don't have a value of some sort
			*/

			if (value == null)
			{
				/*
				*  first, are we supposed to say anything anyway?
				*/
				if (blather)
				{
					EventCartridge eventCartridge = context.EventCartridge;

					bool doIt = true;

					/*
				*  if we have an EventCartridge...
				*/
					if (eventCartridge != null)
					{
						doIt = eventCartridge.ShouldLogOnNullSet(left.Literal, right.Literal);
					}

					if (doIt)
					{
						runtimeServices.Error(
							string.Format("RHS of #set statement is null. Context will not be modified. {0} [line {1}, column {2}]",
														context.CurrentTemplateName, Line, Column));
					}
				}

				return false;
			}

			/*
			*  if the LHS is simple, just punch the value into the context
			*  otherwise, use the setValue() method do to it.
			*  Maybe we should always use setValue()
			*/

			if (left.ChildrenCount == 0)
			{
				context.Put(leftReference, value);
			}
			else
			{
				left.SetValue(context, value);
			}

			return true;
		}
	}
}