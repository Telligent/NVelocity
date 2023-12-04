namespace NVelocity.Runtime.Parser.Node
{
	using Context;
	using System;

	public class ASTNumberLiteral : SimpleNode
	{
		private int valueField;

		public ASTNumberLiteral(int id) : base(id)
		{
		}

		public ASTNumberLiteral(Parser p, int id) : base(p, id)
		{
		}

		/// <summary>Accept the visitor. *
		/// </summary>
		public override object Accept(IParserVisitor visitor, object data)
		{
			return visitor.Visit(this, data);
		}

		/// <summary>  Initialization method - doesn't do much but do the object
		/// creation.  We only need to do it once.
		/// </summary>
		public override object Init(IInternalContextAdapter context, object data)
		{
			/*
			*  init the tree correctly
			*/

			base.Init(context, data);

			valueField = int.Parse(FirstToken.Image);

			return data;
		}

		public override object Value(IInternalContextAdapter context)
		{
			return valueField;
		}
	}
}