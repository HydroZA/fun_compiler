using System;
using System.Collections.Generic;
using Parser.AbstractSyntaxTrees;
using LLVMSharp;

namespace CodeGen
{

    /* TODO
     * - Find out how to prepend built-in functions - DONE
     * - Compiling If statements crashes when compiling the else branch - KINDA FIXED
     * - Finish Operation definitions for CompileFloatOperation()
     * - Implement remaining Exps in CompileExps()
     */

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

        private readonly string[] builtInFunctions;

        public LLVMCodeGen(string filename)
        {
            module = LLVM.ModuleCreateWithName(filename);
            builder = LLVM.CreateBuilder();
            namedValues = new Dictionary<string, LLVMValueRef>();
            typeEnv = new Dictionary<string, VarType>();
            builtInFunctions = new string[]
            {
                "new_line",
                "print_star",
                "print_space",
                "skip",
                "print_int"
            };
        }

        private void CreatePrologue()
        {
            // Create @printf(i8*, ...)
            var arg = new LLVMTypeRef[] { LLVM.PointerType(LLVM.Int8Type(), 0) };
            var printf = LLVM.FunctionType(LLVM.Int32Type(), arg, true);
            var printfFunc = LLVM.AddFunction(module, "printf", printf);

            // Creat @.str
            var str = LLVM.ConstString("%d\n", 4, true);
            var str_glob = LLVM.AddGlobal(module, LLVM.TypeOf(str), "str");
            LLVM.SetInitializer(str_glob, str);
            //namedValues["str"] = str_glob;
            

            // Create @.str_nl
            var str_nl = LLVM.ConstString("\n", 2, true);
            var str_nl_glob = LLVM.AddGlobal(module, LLVM.TypeOf(str_nl), "str_nl");
            LLVM.SetInitializer(str_nl_glob, str_nl);

            // Create @.str_star
            var str_star = LLVM.ConstString("*", 2, true);
            var str_star_glob = LLVM.AddGlobal(module, LLVM.TypeOf(str_star), "str_star");
            LLVM.SetInitializer(str_star_glob, str_star);

            // Create @.str_space
            var str_space = LLVM.ConstString(" ", 2, true);
            var str_space_glob = LLVM.AddGlobal(module, LLVM.TypeOf(str_space), "str_space");
            LLVM.SetInitializer(str_space_glob, str_space);

            // Create @new_line()
            var new_line = LLVM.GetNamedFunction(module, "new_line");
            new_line = LLVM.AddFunction(module, "new_line", LLVM.FunctionType(LLVM.VoidType(), Array.Empty<LLVMTypeRef>(), false));

            LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(new_line, "entry"));

            var callee = LLVM.GetNamedFunction(module, "printf");
            var indicies = new LLVMValueRef[] { LLVM.ConstInt(LLVM.Int32Type(), 0, false), LLVM.ConstInt(LLVM.Int32Type(), 0, false) };
            var gep = LLVM.BuildGEP(builder, str_nl_glob, indicies, "t");
            var call = LLVM.BuildCall(builder, callee, new LLVMValueRef[] { gep }, "1");

            LLVM.BuildRetVoid(builder);
            LLVM.VerifyFunction(new_line, LLVMVerifierFailureAction.LLVMPrintMessageAction);
            
            // Create @print_star()
            var print_star = LLVM.GetNamedFunction(module, "print_star");
            print_star = LLVM.AddFunction(module, "print_star", LLVM.FunctionType(LLVM.VoidType(), Array.Empty<LLVMTypeRef>(), false));

            LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(print_star, "entry"));

            callee = LLVM.GetNamedFunction(module, "printf");
            indicies = new LLVMValueRef[] { LLVM.ConstInt(LLVM.Int32Type(), 0, false), LLVM.ConstInt(LLVM.Int32Type(), 0, false) };
            gep = LLVM.BuildGEP(builder, str_star_glob, indicies, "t");
            LLVM.BuildCall(builder, callee, new LLVMValueRef[] { gep }, "1");

            LLVM.BuildRetVoid(builder);

            // Create @print_space()
            var print_space = LLVM.GetNamedFunction(module, "print_space");
            print_space = LLVM.AddFunction(module, "print_space", LLVM.FunctionType(LLVM.VoidType(), Array.Empty<LLVMTypeRef>(), false));

            LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(print_space, "entry"));

            callee = LLVM.GetNamedFunction(module, "printf");
            indicies = new LLVMValueRef[] { LLVM.ConstInt(LLVM.Int32Type(), 0, false), LLVM.ConstInt(LLVM.Int32Type(), 0, false) };
            gep = LLVM.BuildGEP(builder, str_space_glob, indicies, "t");
            LLVM.BuildCall(builder, callee, new LLVMValueRef[] { gep }, "1");

            LLVM.BuildRetVoid(builder);

            // Create @skip()
            var skip = LLVM.GetNamedFunction(module, "skip");
            skip = LLVM.AddFunction(module, "skip", LLVM.FunctionType(LLVM.VoidType(), Array.Empty<LLVMTypeRef>(), false));

            LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(skip, "entry"));
            LLVM.BuildRetVoid(builder);

            // Create @print_int()
            var print_int = LLVM.GetNamedFunction(module, "print_int");
            print_int = LLVM.AddFunction(module, "print_int", LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[] { LLVM.Int32Type() }, false));

            LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(print_int, "entry"));

            callee = LLVM.GetNamedFunction(module, "printf");
            indicies = new LLVMValueRef[] { LLVM.ConstInt(LLVM.Int32Type(), 0, false), LLVM.ConstInt(LLVM.Int32Type(), 0, false) };
            gep = LLVM.BuildGEP(builder, str_glob, indicies, "t");
            LLVMValueRef param = LLVM.GetParam(print_int, 0);
            LLVM.SetValueName(param, "x");

            LLVM.BuildCall(builder, callee, new LLVMValueRef[] { gep, param}, "1");

            LLVM.BuildRetVoid(builder);

        }

        private bool IsBuiltInFunction(string f) => Array.Exists(builtInFunctions, x => x == f);

        private VarType ConvertType(string s) => s.Trim().ToLower() switch
        {
            "int" => VarType.INT,
            "double" => VarType.DOUBLE,
            "void" => VarType.VOID,
            _ => throw new UnexpectedArgumentException("Unable to define type: " + s)
        };

        private LLVMValueRef CompileExp(Exp e) => e switch
        {
            Num n => LLVM.ConstInt(LLVM.Int32Type(), (ulong)n.I, false),
            FNum fn => LLVM.ConstReal(LLVM.DoubleType(), fn.I),
            Var v => namedValues[v.S],
            Call c => CompileCall(c),
            If f => CompileIf(f),
            Operation o => GetType(o) == VarType.INT ? CompileIntOperation(o) : CompileFloatOperation(o),
            Sequence s => CompileSequence(s),
            _ => throw new NotImplementedException()
        };

        private LLVMValueRef CompileSequence (Sequence s)
        {
            CompileExp(s.E1);
            return CompileExp(s.E2);
        }
        private LLVMValueRef CompileIf(If f)
        {
            LLVMValueRef _if;
            if (GetType(f) == VarType.DOUBLE)
                _if = CompileFloatOperation(f.Cond);
            else
                _if = CompileIntOperation(f.Cond);

            //return LLVM.BuildSelect(builder, _if, CompileExp(f.E1), CompileExp(f.E2), "");

            var thenBB = parent.AppendBasicBlock("then");
            var elseBB = parent.AppendBasicBlock("else");
          //  var mergeBB = parent.AppendBasicBlock("merge");

            var condbr = LLVM.BuildCondBr(
                builder,
                _if,
                thenBB,
                elseBB
            );

            LLVM.PositionBuilderAtEnd(builder, thenBB);
            var thenVal = CompileExp(f.E1);
            //LLVM.BuildBr(builder, mergeBB);
            LLVM.BuildRetVoid(builder);

            LLVM.PositionBuilderAtEnd(builder, elseBB);
            var elseVal = CompileExp(f.E2);
            // LLVM.BuildBr(builder, mergeBB);
           // LLVM.BuildRetVoid(builder);

            return condbr;
         /*   LLVM.PositionBuilderAtEnd(builder, mergeBB);
            var phi = LLVM.BuildPhi(builder, LLVM.VoidType(), "");
            phi.AddIncoming(new LLVMValueRef[] { thenVal, elseVal }, new LLVMBasicBlockRef[] { thenBB, elseBB }, 2);
            
            return phi;
           */ 

        }

        private VarType GetType(Exp e) => e switch
        {
            Num => VarType.INT,
            FNum => VarType.DOUBLE,
            Var v => typeEnv[v.S],
            Call c => typeEnv[c.S],
            If f => GetType(f.Cond),//GetType(f.E1) == GetType(f.E2) ? GetType(f.E1) : throw new Exception("Mismatched types in If expression"),
            Operation o => GetType(o.A1),
            _ => throw new Exception("Unable to define type of: " + e)
        };

        private LLVMValueRef CompileCall(Call c)
        {
            var calleeF = LLVM.GetNamedFunction(module, c.S);
            if (calleeF.Pointer == IntPtr.Zero)
                throw new Exception($"Unknown function: {c.S}");

            if (LLVM.CountParams(calleeF) != c.Arguments.Count)
                throw new Exception($"Incorrect no. arguments passed in function call: {c.S}");
          
            LLVMValueRef[] argsV = new LLVMValueRef[c.Arguments.Count];

            for (int i = 0; i < c.Arguments.Count; i++)
            {
                argsV[i] = CompileExp(c.Arguments[i]);
            }

            if (IsBuiltInFunction(c.S))
            {
                return LLVM.BuildCall(this.builder, calleeF, argsV, "");
            }
            else
            {
                return LLVM.BuildCall(this.builder, calleeF, argsV, "");
            }


            
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
                OperationType.DIVIDE => LLVM.BuildFDiv(this.builder, l, r, "divtmp"),
                OperationType.MODULO => LLVM.BuildFRem(this.builder, l, r, "remtmp"),
                OperationType.LESS_THAN => LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealOLT, l, r, "lttmp"),
                OperationType.GREATER_THAN => LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealOGT, l, r, "gttmp"),
                OperationType.LESS_THAN_OR_EQUAL => LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealOLE, l, r, "letmp"),
                OperationType.GREATER_THAN_OR_EQUAL => LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealOGE, l, r, "getmp"),
                OperationType.NOT_EQUAL => LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealONE, l, r, "netmp"),
                OperationType.EQUAL => LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealOEQ, l, r, "eqtmp"),
                _ => throw new Exception("invalid binary operator"),
            };
        }

        private LLVMValueRef CompileIntOperation(Operation node)
        {
            LLVMValueRef l = CompileExp(node.A1);
            LLVMValueRef r = CompileExp(node.A2);

            return node.O switch
            {
                OperationType.PLUS => LLVM.BuildAdd(this.builder, l, r, "addtmp"),
                OperationType.MINUS => LLVM.BuildSub(this.builder, l, r, "subtmp"),
                OperationType.TIMES => LLVM.BuildMul(this.builder, l, r, "multmp"),
                OperationType.LESS_THAN => LLVM.BuildICmp(this.builder, LLVMIntPredicate.LLVMIntSLT, l, r, "lttmp"),
                OperationType.DIVIDE => LLVM.BuildSDiv(this.builder, l, r, "divtmp"),
                OperationType.MODULO => LLVM.BuildSRem(this.builder, l, r, "remtmp"),
                OperationType.GREATER_THAN => LLVM.BuildICmp(this.builder, LLVMIntPredicate.LLVMIntSGT, l, r, "gttmp"),
                OperationType.LESS_THAN_OR_EQUAL => LLVM.BuildICmp(this.builder, LLVMIntPredicate.LLVMIntSLE, l, r, "letmp"),
                OperationType.GREATER_THAN_OR_EQUAL => LLVM.BuildICmp(this.builder, LLVMIntPredicate.LLVMIntSGE, l, r, "getmp"),
                OperationType.NOT_EQUAL => LLVM.BuildICmp(this.builder, LLVMIntPredicate.LLVMIntNE, l, r, "netmp"),
                OperationType.EQUAL => LLVM.BuildICmp(this.builder, LLVMIntPredicate.LLVMIntEQ, l, r, "eqtmp"),
                _ => throw new Exception("invalid binary operator"),
            };
        }

        private LLVMValueRef CompileFunctionDef(Def d)
        {
            var arguments = new LLVMTypeRef[Math.Max(d.Args.Count, 1)];
            var function = LLVM.GetNamedFunction(module, d.Name);

            LLVMTypeRef ConvertToLLVMType (VarType v) => v switch
            {
                VarType.DOUBLE => LLVM.DoubleType(),
                VarType.INT => LLVM.Int32Type(),
                _ => throw new Exception("Invalid argument type in function: " + d.Name)
            };

            if (d.Type == VarType.DOUBLE)
            {
                for (int i = 0; i < d.Args.Count; ++i)
                    // TODO convert Def.Args[] to be a VarType[] not (string,string)[]
                    arguments[i] = ConvertToLLVMType(ConvertType(d.Args[i].Item2));
                
                function = LLVM.AddFunction(module, d.Name, LLVM.FunctionType(LLVM.DoubleType(), arguments, false));

                // Add the function to the typing environment
                typeEnv.Add(d.Name, VarType.DOUBLE);
            }

            else if (d.Type == VarType.INT)
            {
                for (int i = 0; i < d.Args.Count; ++i)
                    arguments[i] = ConvertToLLVMType(ConvertType(d.Args[i].Item2));
                function = LLVM.AddFunction(module, d.Name, LLVM.FunctionType(LLVM.Int32Type(), arguments, false));

                typeEnv.Add(d.Name, VarType.INT);
            }
            else if (d.Type == VarType.VOID)
            {
                for (int i = 0; i < d.Args.Count; ++i)
                    arguments[i] = ConvertToLLVMType(ConvertType(d.Args[i].Item2));
                function = LLVM.AddFunction(module, d.Name, LLVM.FunctionType(LLVM.VoidType(), arguments, false));

                typeEnv.Add(d.Name, VarType.VOID);
            }
            else
                throw new Exception("Return type unknown in function: " + d.Name);

            LLVM.SetLinkage(function, LLVMLinkage.LLVMExternalLinkage);

            for (int i = 0; i < d.Args.Count; ++i)
            {
                string argName = d.Args[i].Item1;

                // add the variable to the typeEnv
                typeEnv.AddOrUpdate(d.Args[i].Item1, ConvertType(d.Args[i].Item2));

                LLVMValueRef param = LLVM.GetParam(function, (uint)i);
                LLVM.SetValueName(param, argName);

                this.namedValues[argName] = param;
            }
            
            return function;
        }

        private LLVMValueRef CompileMainDef()
        {
            var func = LLVM.GetNamedFunction(module, "main");
            func = LLVM.AddFunction(module, "main", LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[0], false));
            LLVM.SetLinkage(func, LLVMLinkage.LLVMExternalLinkage);

            return func;
        }

        private LLVMValueRef CompileConstantDef(Const c)
        {
            LLVM.GetNamedGlobal(module, c.Name);
            LLVMValueRef con = LLVM.AddGlobal(module, LLVM.Int32Type(), c.Name);
            LLVM.SetLinkage(con, LLVMLinkage.LLVMExternalLinkage);

            typeEnv.Add(c.Name, VarType.INT);

            return con;

        }
        private LLVMValueRef CompileConstantDef(FConst c)
        {
            LLVM.GetNamedGlobal(module, c.Name);
            LLVMValueRef con = LLVM.AddGlobal(module, LLVM.DoubleType(), c.Name);
         //   LLVM.SetLinkage(con, LLVMLinkage.LLVMExternalLinkage);

            typeEnv.Add(c.Name, VarType.DOUBLE);

            return con;
        }

        private LLVMValueRef parent;

        public LLVMValueRef CompileDecl(Decl d)
        {
            switch (d)
            {
                case Def de:
                    LLVMValueRef func = CompileFunctionDef(de);
                    LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(func, "entry"));

                    parent = func;
                    LLVMValueRef body = CompileExp(de.body);

                    if (de.Type == VarType.VOID)
                        LLVM.BuildRetVoid(builder);                   
                    else                  
                        LLVM.BuildRet(builder, body);

                    // validate code
                    LLVM.VerifyFunction(func, LLVMVerifierFailureAction.LLVMPrintMessageAction);
                    return func;
                case Main m:
                    LLVMValueRef main = CompileMainDef();
                    LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(main, "main"));

                    LLVMValueRef _body = CompileExp(m.Body);

                    LLVM.BuildRet(builder, LLVM.ConstInt(LLVM.Int32Type(), 0, false));

                    LLVM.VerifyFunction(main, LLVMVerifierFailureAction.LLVMPrintMessageAction);

                    return main;

                case Const c:
                    var con = CompileConstantDef(c);
                    var val = LLVM.ConstInt(LLVM.Int32Type(), (ulong)c.V, false);
                    con.SetInitializer(val);

                    this.namedValues[c.Name] = val;

                    return con;
                case FConst fc:
                    var fcon = CompileConstantDef(fc);
                    var fval = LLVM.ConstReal(LLVM.DoubleType(), fc.X);
                    fcon.SetInitializer(fval);

                    this.namedValues[fc.Name] = fval;

                    return fcon;
                default:
                    throw new UnexpectedArgumentException("Unknown declaration received: " + d);
            }
        }

        public LLVMModuleRef GenerateCode(List<Decl> ast)
        {
            CreatePrologue();
            foreach (Decl d in ast)
            {
                CompileDecl(d);
            }
            return module;
        }
    }
}
