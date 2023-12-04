namespace NVelocity.Runtime.Parser.Node
{
	using System;

	public class ASTParameters : SimpleNode
	{
		public ASTParameters(int id) : base(id)
		{
		}

		public ASTParameters(Parser p, int id) : base(p, id)
		{
		}

		/// <summary>
		/// Accept the visitor.
		/// </summary>
		public override object Accept(IParserVisitor visitor, object data)
		{
			return visitor.Visit(this, data);
		}
	}
}