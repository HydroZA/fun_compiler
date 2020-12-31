using System;
using System.Collections.Generic;
using Parser.AbstractSyntaxTrees;
using LLVMSharp;

namespace CodeGen
{
    public class LLVMCodeGen
    {
        // the top-level structure that the LLVM IR uses to contain code
        private readonly LLVMModuleRef module;

        // helper object that makes it easy to generate LLVM instructions.
        // Instances of the LLVMBuilderRef class template keep track of the
        // current place to insert instructions and has methods to create new instructions
        private readonly LLVMBuilderRef builder;

        // keeps track of which values are defined in the current scope and what their LLVM representation is.
        private readonly Dictionary<string, LLVMValueRef> namedValues;

        // maintains a map of what type each variable, function, operation, etc takes
        private readonly Dictionary<string, VarType> typeEnv;

        public LLVMCodeGen()
        {
            module = LLVM.ModuleCreateWithName("codegen");
            builder = LLVM.CreateBuilder();
            namedValues = new Dictionary<string, LLVMValueRef>();
            typeEnv = new Dictionary<string, VarType>();
        }

        private VarType ConvertType(string s) => s.Trim().ToLower() switch
        {
            "int" => VarType.INT,
            "double" => VarType.DOUBLE,
            _ => throw new UnexpectedArgumentException("Unable to define type: " + s)
        };

        private LLVMValueRef CompileExp(Exp e) => e switch
        {
            Num n => LLVM.ConstReal(LLVM.Int32Type(), n.I),
            FNum fn => LLVM.ConstReal(LLVM.DoubleType(), fn.I),
            Var v => namedValues[v.S],
            Call c => CompileCall(c),
            If f => CompileIf(f),
            Operation o => CompileFloatOperation(o),
            Sequence => throw new NotImplementedException()
        };


        private LLVMValueRef CompileIf(If f)
        {
            LLVMValueRef cond;
            if (GetType(f) == VarType.DOUBLE)
                cond = CompileFloatOperation(f.Cond);
            else
                cond = CompileIntOperation(f.Cond);
            
            return LLVM.BuildCondBr(builder,
                cond,
                CompileExp(f.E1).AppendBasicBlock("iftmp"),
                CompileExp(f.E2).AppendBasicBlock("iftmp")
            );
        }

        private VarType GetType(Exp e) => e switch
        {
            Num => VarType.INT,
            FNum => VarType.DOUBLE,
            Var v => typeEnv[v.S],
            Call c => typeEnv[c.S],
            If f => GetType(f.E1) == GetType(f.E2) ? GetType(f.E1) : throw new Exception("Mismatched types in If expression"),
            Operation o => GetType(o.A1),
            _ => throw new Exception("Unable to define type of: " + e)
        };

        private LLVMValueRef CompileCall(Call c)
        {
            var calleeF = LLVM.GetNamedFunction(module, c.S);
            if (calleeF.Pointer == IntPtr.Zero)
                throw new Exception($"Unknown function called: {c.S}");

            if (LLVM.CountParams(calleeF) != c.Arguments.Count)
                throw new Exception($"Incorrect no. arguments passed in function call: {c.S}");

            LLVMValueRef[] argsV = new LLVMValueRef[c.Arguments.Count];

            for (int i = 0; i < c.Arguments.Count; i++)
            {
                argsV[i] = CompileExp(c.Arguments[i]);
            }

            return LLVM.BuildCall(this.builder, calleeF, argsV, "calltmp");
        }

        private LLVMValueRef CompileFloatOperation(Operation node)
        {
            LLVMValueRef l = CompileExp(node.A1);
            LLVMValueRef r = CompileExp(node.A2);
            
            return node.O switch
            {
                OperationType.PLUS => LLVM.BuildFAdd(this.builder, l, r, "addtmp"),
                OperationType.MINUS => LLVM.BuildFSub(this.builder, l, r, "subtmp"),
                OperationType.TIMES => LLVM.BuildFMul(this.builder, l, r, "multmp"),
                OperationType.LESS_THAN => LLVM.BuildUIToFP(this.builder, LLVM.BuildFCmp(this.builder, LLVMRealPredicate.LLVMRealULT, l, r, "cmptmp"), LLVM.DoubleType(), "booltmp"),// Convert bool 0/1 to double 0.0 or 1.0
                OperationType.DIVIDE => LV
                OperationType.MODULO => throw new NotImplementedException(),
                OperationType.GREATER_THAN => throw new NotImplementedException(),
                OperationType.LESS_THAN_OR_EQUAL => throw new NotImplementedException(),
                OperationType.GREATER_THAN_OR_EQUAL => throw new NotImplementedException(),
                OperationType.NOT_EQUAL => throw new NotImplementedException(),
                OperationType.EQUAL => throw new NotImplementedException(),
                _ => throw new Exception("invalid binary operator"),
            };
        }

        private LLVMValueRef CompileIntOperation(Operation node)
        {
            throw new NotImplementedException();
        }

        private LLVMValueRef CompileFunctionDef(Def d)
        {
            var arguments = new LLVMTypeRef[Math.Max(d.Args.Count, 1)];
            var function = LLVM.GetNamedFunction(module, d.Name);

            if (d.Type == VarType.DOUBLE)
            {
                for (int i = 0; i < d.Args.Count; ++i)
                    arguments[i] = LLVM.DoubleType();
                function = LLVM.AddFunction(module, d.Name, LLVM.FunctionType(LLVM.DoubleType(), arguments, false));

                // Add the function to the typing environment
                typeEnv.Add(d.Name, VarType.DOUBLE);
            }

            else if (d.Type == VarType.INT)
            {
                for (int i = 0; i < d.Args.Count; ++i)
                    arguments[i] = LLVM.Int32Type();
                function = LLVM.AddFunction(module, d.Name, LLVM.FunctionType(LLVM.Int32Type(), arguments, false));

                typeEnv.Add(d.Name, VarType.INT);
            }
            else
                throw new Exception("Return type unknown in function: " + d.Name);

            LLVM.SetLinkage(function, LLVMLinkage.LLVMExternalLinkage);

            for (int i = 0; i < d.Args.Count; ++i)
            {
                string argName = d.Args[i].Item1;

                // add the variable to the typeEnv
                typeEnv.Add(d.Args[i].Item1, ConvertType(d.Args[i].Item2));

                LLVMValueRef param = LLVM.GetParam(function, (uint)i);
                LLVM.SetValueName(param, argName);

                this.namedValues[argName] = param;
            }
            
            return function;
        }

        private LLVMValueRef CompileMainDef(Main m)
        {
            var func = LLVM.GetNamedFunction(module, "main");
            func = LLVM.AddFunction(module, "main", LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[0], false));
            LLVM.SetLinkage(func, LLVMLinkage.LLVMExternalLinkage);

            return func;
        }

        public LLVMValueRef CompileDecl(Decl d)
        {
            switch (d)
            {
                case Def de:
                    LLVMValueRef func = CompileFunctionDef(de);
                    LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(func, "entry"));

                    LLVMValueRef body = CompileExp(de.body);

                    LLVM.BuildRet(builder, body);

                    // validate code
                    LLVM.VerifyFunction(func, LLVMVerifierFailureAction.LLVMPrintMessageAction);

                    return func;
                case Main m:
                    LLVMValueRef main = CompileMainDef(m);
                    LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(main, "main"));

                    LLVMValueRef _body = CompileExp(m.Body);

                    LLVM.BuildRet(builder, _body);

                    LLVM.VerifyFunction(main, LLVMVerifierFailureAction.LLVMPrintMessageAction);

                    return main;

                case Const c:
                    //  LLVM.AddGlobal(module, LLVM.ConstInt(LLVM.Int32Type(), (ulong)c.V, false), c.Name);
                    throw new NotImplementedException();
                case FConst fc:
                    throw new NotImplementedException();
                default:
                    throw new UnexpectedArgumentException("Unknown declaration received: " + d);
            }
        }

        public LLVMModuleRef GenerateCode(List<Decl> ast)
        {
            foreach (Decl d in ast)
            {
                CompileDecl(d);
            }
            return module;
        }
    }
}
