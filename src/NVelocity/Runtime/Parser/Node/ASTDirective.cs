namespace NVelocity.Runtime.Parser.Node
{
	using Context;
	using Directive;
	using System;
	using System.IO;

	/// <summary>
	/// This class is responsible for handling the pluggable
	/// directives in VTL. ex.  #foreach()
	///
	/// Please look at the Parser.jjt file which is
	/// what controls the generation of this class.
	/// </summary>
	public class ASTDirective : SimpleNode
	{
		private Directive directive;
		private string directiveName = string.Empty;

		public ASTDirective(int id) : base(id)
		{
		}

		public ASTDirective(Parser p, int id) : base(p, id)
		{
		}

		/// <summary>
		/// Accept the visitor.
		/// </summary>
		public override object Accept(IParserVisitor visitor, object data)
		{
			return visitor.Visit(this, data);
		}

		public override object Init(IInternalContextAdapter context, object data)
		{
			base.Init(context, data);

			if (directive == null && runtimeServices.IsVelocimacro(directiveName, context.CurrentTemplateName))
			{
				directive = runtimeServices.GetVelocimacro(directiveName, context.CurrentTemplateName);
			}

			if (directive != null)
			{
				directive.Init(runtimeServices, context, this);
				directive.SetLocation(Line, Column);
			}

			return data;
		}

		public override bool Render(IInternalContextAdapter context, TextWriter writer)
		{
			// normal processing
			if (directive == null)
			{
				writer.Write(directiveName);
			}
			else
			{
				directive.Render(context, writer, this);
			}

			return true;
		}

		/// <summary>
		/// Gets or sets the directive name.
		/// Used by the parser.  
		/// This keeps us from having to
		/// dig it out of the token stream and gives the parse 
		/// the change to override.
		/// </summary>
		public string DirectiveName
		{
			get { return directiveName; }
			set { directiveName = value; }
		}

		public Directive Directive
		{
			get { return directive; }
			set { directive = value; }
		}
	}
}