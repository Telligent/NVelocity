namespace NVelocity.Runtime.Parser.Node
{
	using System;

	public class ASTVariable : SimpleNode
	{
		public ASTVariable(int id) : base(id)
		{
		}

		public ASTVariable(Parser p, int id) : base(p, id)
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