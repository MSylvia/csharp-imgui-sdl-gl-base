using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CLanguage.Interpreter;
using CLanguage.Compiler;

namespace CLanguage.Syntax
{
    public class WhileStatement : Statement
    {
        public bool IsDo { get; private set; }
        public Expression Condition { get; private set; }
        public Block Loop { get; private set; }

        public WhileStatement(bool isDo, Expression condition, Block loop)
        {
            IsDo = isDo;
            Condition = condition;
            Loop = loop;
        }

        protected override void DoEmit(EmitContext parentContext)
        {
            var condLabel = parentContext.DefineLabel();
            var loopLabel = parentContext.DefineLabel();
            var endLabel = parentContext.DefineLabel();

            var ec = parentContext.PushLoop (breakLabel: endLabel, continueLabel: condLabel);

            if (IsDo) {
                ec.EmitLabel (loopLabel);
                Loop.Emit (ec);
                ec.EmitLabel (condLabel);
                Condition.Emit (ec);
                ec.EmitCastToBoolean (Condition.GetEvaluatedCType (ec));
                ec.Emit (OpCode.BranchIfFalse, endLabel);
                ec.Emit (OpCode.Jump, condLabel);
            }
            else {
                ec.EmitLabel (condLabel);
                Condition.Emit (ec);
                ec.EmitCastToBoolean (Condition.GetEvaluatedCType (ec));
                ec.Emit (OpCode.BranchIfFalse, endLabel);
                ec.EmitLabel (loopLabel);
                parentContext.BeginBlock (Loop);
                Loop.Emit (ec);
                ec.Emit (OpCode.Jump, condLabel);
            }
            ec.EmitLabel (endLabel);
        }

		public override bool AlwaysReturns {
			get {
				return false;
			}
		}

        public override string ToString()
        {
            if (IsDo)
            {
                return string.Format("do {1} while({0});", Condition, Loop);
            }
            else
            {
                return string.Format("while ({0}) {1};", Condition, Loop);
            }
        }

        public override void AddDeclarationToBlock (BlockContext context)
        {
            Loop.AddDeclarationToBlock (context);
        }
    }
}
