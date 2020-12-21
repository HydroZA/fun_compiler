using System;
namespace CodeGen
{
    public class CodeGenerator
    {
        private const string PRELUDE = @"
declare i32 @printf(i8*, ...)

@.str_nl = private constant [2 x i8] c""\0A\00""
@.str_star = private constant[2 x i8] c""*\00""
@.str_space = private constant[2 x i8] c"" \00""

define void @new_line() #0 {
  %t0 = getelementptr[2 x i8], [2 x i8]* @.str_nl, i32 0, i32 0
  %1 = call i32(i8*, ...) @printf(i8* %t0)
  ret void
    }

    define void @print_star() #0 {
  %t0 = getelementptr[2 x i8], [2 x i8]* @.str_star, i32 0, i32 0
  %1 = call i32(i8*, ...) @printf(i8* %t0)
  ret void
}

define void @print_space() #0 {
  % t0 = getelementptr[2 x i8], [2 x i8]* @.str_space, i32 0, i32 0
  % 1 = call i32(i8 *, ...) @printf(i8 * % t0)
  ret void
}

define void @skip() #0 {
  ret void
}

@.str = private constant[4 x i8] c""%d\0A\00""

define void @print_int(i32 %x)
{
   % t0 = getelementptr[4 x i8], [4 x i8]* @.str, i32 0, i32 0
   call i32(i8*, ...) @printf(i8 * % t0, i32 % x)
   ret void
}
; END OF PRELUDE";

        public CodeGenerator()
        {
        }
    }
}
