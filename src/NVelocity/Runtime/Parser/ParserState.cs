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

/* Generated By:JJTree: Do not edit this line. JJTParserState.java */


namespace NVelocity.Runtime.Parser
{
	using Node;
	using System.Collections.Generic;

	internal class ParserState
	{
		private readonly Stack<INode> nodes;
		private readonly Stack<int> marks;

		private int mark; // current mark
		private bool nodeCreated;

		internal ParserState()
		{
			nodes = new Stack<INode>();
			marks = new Stack<int>();
		}

		/// <summary>
		/// Determines whether the current node was actually closed and
		/// pushed.  This should only be called in the final user action of a
		/// node scope.
		/// </summary>
		/// <returns></returns>
		internal bool NodeCreated()
		{
			return nodeCreated;
		}

		/// <summary>
		/// Call this to reinitialize the node stack.  It is called automatically by the parser's ReInit() method.
		/// </summary>
		internal void Reset()
		{
			nodes.Clear();
			marks.Clear();
			mark = 0;
		}

		/// <summary>
		/// Returns the root node of the AST.  It only makes sense to call this after a successful parse.  
		/// </summary>
		internal INode RootNode
		{
			get { return (nodes.ToArray())[nodes.Count - (0 + 1)]; }
		}

		/// <summary>
		/// Pushes a node on to the stack. 
		/// </summary>
		internal void PushNode(INode n)
		{
			nodes.Push(n);
		}

		/// <summary>
		/// Returns the node on the top of the stack, and remove it from the	stack. 
		/// </summary>
		internal INode PopNode()
		{
			if (nodes.Count < mark)
			{
				mark = marks.Pop();
			}
			return nodes.Pop();
		}

		/// <summary>
		/// Returns the node currently on the top of the stack.
		/// </summary>
		internal INode PeekNode()
		{
			return nodes.Peek();
		}

		/// <summary>
		/// Returns the number of children on the stack in the current node scope.
		/// </summary>
		internal int NodeArity()
		{
			return nodes.Count - mark;
		}


		internal void ClearNodeScope(INode n)
		{
			while (nodes.Count > mark)
			{
				PopNode();
			}
			mark = marks.Pop();
		}


		internal void OpenNodeScope(INode node)
		{
			marks.Push(mark);
			mark = nodes.Count;
			node.Open();
		}


		/// <summary>
		/// A definite node is constructed from a specified number of
		/// children.  That number of nodes are popped from the stack and
		/// made the children of the definite node.  Then the definite node
		/// is pushed on to the stack.
		/// </summary>
		internal void CloseNodeScope(INode parentNode, int num)
		{
			mark = marks.Pop();
			while (num-- > 0)
			{
				INode node = PopNode();
				node.Parent = parentNode;
				parentNode.AddChild(node, num);
			}
			parentNode.Close();
			PushNode(parentNode);
			nodeCreated = true;
		}


		/// <summary>
		/// A conditional node is constructed if its condition is true.  All
		/// the nodes that have been pushed since the node was opened are
		/// made children of the the conditional node, which is then pushed
		/// on to the stack.  If the condition is false the node is not
		/// constructed and they are left on the stack.
		/// </summary>
		internal void CloseNodeScope(INode n, bool condition)
		{
			if (condition)
			{
				int arity = NodeArity();
				mark = marks.Pop();
				while (arity-- > 0)
				{
					INode node = PopNode();
					node.Parent = n;
					n.AddChild(node, arity);
				}
				n.Close();
				PushNode(n);
				nodeCreated = true;
			}
			else
			{
				mark = marks.Pop();
				nodeCreated = false;
			}
		}
	}
}
