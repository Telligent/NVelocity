namespace NVelocity.Runtime.Parser.Node
{
	using Context;
	using System;

	public class ASTExpression : SimpleNode
	{
		public ASTExpression(int id) : base(id)
		{
		}

		public ASTExpression(Parser p, int id) : base(p, id)
		{
		}

		/// <summary>
		/// Accept the visitor.
		/// </summary>
		public override object Accept(IParserVisitor visitor, object data)
		{
			return visitor.Visit(this, data);
		}

		public override bool Evaluate(IInternalContextAdapter context)
		{
			return GetChild(0).Evaluate(context);
		}

		public override object Value(IInternalContextAdapter context)
		{
			return GetChild(0).Value(context);
		}
	}
}