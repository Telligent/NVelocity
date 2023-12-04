namespace NVelocity.Runtime.Parser.Node
{
	using System;

	public class ASTprocess : SimpleNode
	{
		public ASTprocess(int id) : base(id)
		{
		}

		public ASTprocess(Parser p, int id) : base(p, id)
		{
		}

		/// <summary>Accept the visitor. *
		/// </summary>
		public override object Accept(IParserVisitor visitor, object data)
		{
			return visitor.Visit(this, data);
		}
	}
}