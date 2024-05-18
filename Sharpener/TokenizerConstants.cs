﻿using Sharpener.Enums;

namespace Sharpener
{
    public class TokenizerConstants
    {
        public static readonly Dictionary<string, TokenType> KeywordSyntax = new Dictionary<string, TokenType>
        {
            { "abstract", TokenType.AbstractKeyword },
            { "add", TokenType.AddEventSubscriptionHandler },
            { "array", TokenType.ArrayDeclaration },
            { "as", TokenType.AsCast },
            { "asc", TokenType.UNKNOWN },
            { "aspect", TokenType.AspectPrefix },
            { "assembly", TokenType.AssemblyKeyword },
            { "async", TokenType.AsyncDelclaration },
            { "autoreleasepool", TokenType.UNKNOWN },
            { "await", TokenType.AwaitKeyword },
            { "begin", TokenType.CodeBlockBegin },
            { "block", TokenType.BlockDelegateType },
            { "break", TokenType.BreakLoopKeyword },
            { "by", TokenType.ByLinq },
            { "case", TokenType.CaseKeyword },
            { "class", TokenType.ClassKeyword },
            { "concat", TokenType.ConcatLinq },
            { "const", TokenType.ConstKeyword },
            { "constructor", TokenType.ConstructorKeyword },
            { "continue", TokenType.ContinueKeyword },
            { "copy", TokenType.UNKNOWN },
            { "default", TokenType.UNKNOWN },
            { "delegate", TokenType.DelegateType },
            { "deprecated", TokenType.DeprecatedKeyword },
            { "desc", TokenType.DescLinq },
            { "distint", TokenType.DistinctLinq },
            { "div", TokenType.DivideOperator },
            { "do", TokenType.DoKeyword },
            { "downto", TokenType.DownToForLoopKeyword },
            { "dynamic", TokenType.DynamicType },
            { "each", TokenType.EachKeyword },
            { "else", TokenType.ElseKeyword },
            { "empty", TokenType.EmptyKeyword },
            { "end", TokenType.CodeBlockEnd },
            { "end.", TokenType.EndOfFile },
            { "ensure", TokenType.EnsureMethodCondition },
            { "enum", TokenType.EnumKeyword },
            { "equals", TokenType.EqualsLinq },
            { "event", TokenType.EventKeyword },
            { "except", TokenType.ExceptCatchKeyword },
            { "exit", TokenType.ExitReturnKeyword },
            { "extension", TokenType.ExtensionKeyword },
            { "external", TokenType.ExternalKeyword },
            { "final", TokenType.FinalOverrideKeyword },
            { "finalizer", TokenType.FinalizerDeconstructorKeyword },
            { "finally", TokenType.FinallyTryKeyword },
            { "flags", TokenType.FlagsKeyword },
            { "for", TokenType.ForKeyword },
            { "from", TokenType.FromLinq },
            { "future", TokenType.FutureType },
            { "global", TokenType.GlobalAspect },
            { "group", TokenType.GroupLinq },
            { "has", TokenType.HasRequirementClassConstraint },
            { "if", TokenType.IfKeyword },
            { "implementation", TokenType.ImplementationSection },
            { "implements", TokenType.ImplementsInheritanceKeyword },
            { "implies", TokenType.UNKNOWN },
            { "in", TokenType.InForLoop },
            { "index", TokenType.IndexForLoop },
            { "inherited", TokenType.InheritedCall },
            { "inline", TokenType.InlineCompilationCall },
            { "interface", TokenType.InterfaceKeyword },
            { "into", TokenType.IntoLinq },
            { "invariants", TokenType.UNKNOWN },
            { "is", TokenType.IsTestCast },
            { "iterator", TokenType.IteratorMethod },
            { "join", TokenType.JoinLinq },
            { "lazy", TokenType.LazyKeyword },
            { "lifetimestrategy", TokenType.UNKNOWN },
            { "locked", TokenType.LockedMethodKeyword },
            { "locking", TokenType.LockingOnTypeKeyword },
            { "loop", TokenType.LoopInfiniteKeyword },
            { "mapped", TokenType.UNKNOWN },
            { "matching", TokenType.LoopMatchingTypeConstaint },
            { "method", TokenType.MethodKeyword },
            { "module", TokenType.UNKNOWN },
            { "namespace", TokenType.NamespaceKeyword },
            { "nested", TokenType.NestedTypeKeyword },
            { "new", TokenType.NewKeyword },
            { "notify", TokenType.NotifyPropertyChangedImplementation },
            { "nullable", TokenType.NullableTypeDefinition },
            { "of", TokenType.OfTypeKeyword },
            { "old", TokenType.UNKNOWN },
            { "on", TokenType.OnCatchExceptionType },
            { "operator", TokenType.OperatorCustom },
            { "optional", TokenType.UNKNOWN },
            { "or", TokenType.OrBoolean },
            { "order", TokenType.OrderLinq },
            { "out", TokenType.OutKeyword },
            { "override", TokenType.OverrideKeyword },
            { "parallel", TokenType.ParallelForLoop },
            { "param", TokenType.ParamAspectKeyword },
            { "params", TokenType.ParamsCaptureArgsMethod },
            { "partial", TokenType.PartialClassKeyword },
            { "pinned", TokenType.PinnedTypeGCKeyword },
            { "private", TokenType.PrivateKeyword },
            { "property", TokenType.PropertyKeyword },
            { "protected", TokenType.ProtectedKeyword },
            { "public", TokenType.PublicKeyword },
            { "published", TokenType.UNKNOWN },
            { "queryable", TokenType.UNKNOWN },
            { "raise", TokenType.RaiseThrowKeyword },
            { "read", TokenType.ReadGetterKeyword },
            { "readonly", TokenType.ReadonlyKeyword },
            { "record", TokenType.RecordKeyword },
            { "reintroduce", TokenType.UNKNOWN },
            { "remove", TokenType.RemoveEventUnsubscribe },
            { "repeat", TokenType.RepeatDoUntilKeyword },
            { "require", TokenType.RequireMethodPrecondition },
            { "result", TokenType.ResultVariableKeyword },
            { "reverse", TokenType.ReverseLinq },
            { "sealed", TokenType.SealedKeyword },
            { "select", TokenType.SelectLinq },
            { "self", TokenType.SelfThisKeyword },
            { "sequence", TokenType.SequenceCollectionKeyword },
            { "set", TokenType.SetOrdinalValuesType },
            { "skip", TokenType.SkipLinq },
            { "soft", TokenType.UNKNOWN },
            { "static", TokenType.StaticKeyword },
            { "step", TokenType.StepForLoopIncrement },
            { "strong", TokenType.UNKNOWN },
            { "take", TokenType.TakeLinq },
            { "then", TokenType.ThenKeyword },
            { "to", TokenType.ForLoopIterationConstraint },
            { "try", TokenType.TryCatchKeyword },
            { "tuple", TokenType.TupleKeyword },
            { "type", TokenType.TypeDeclarationKeyword },
            { "unit", TokenType.UNKNOWN },
            { "unretained", TokenType.UNKNOWN },
            { "unsafe", TokenType.UnsafeKeyword },
            { "until", TokenType.UntilDoUntilKeyword },
            { "uses", TokenType.UsesKeyword },
            { "using", TokenType.UsingStatementKeyword },
            { "var", TokenType.VarKeyword },
            { "virtual", TokenType.VirtualKeyword },
            { "volatile", TokenType.VolatileKeyword },
            { "weak", TokenType.UNKNOWN },
            { "where", TokenType.WhereConstaintAndLinq },
            { "while", TokenType.WhileKeyword },
            { "with", TokenType.WithLocalVariableScopeKeyword },
            { "write", TokenType.WriteSetterKeyword },
            { "writeln", TokenType.WriteLnMagicFunction },
            { "yield", TokenType.YieldIteratorReturnCollectionKeyword },
        };

        public static readonly Dictionary<string, TokenType> OperatorSyntax = new Dictionary<string, TokenType>
        {
            { ":=",  TokenType.ClassDefinitionAndPropertyNullAccessor },
            { "=",  TokenType.BooleanEquals },
            { "<>",  TokenType.BooleanNotEquals },
            { "+",  TokenType.AddOperator },
            { "-",  TokenType.SubtractOperator },
            { "/",  TokenType.DivideOperator },
            { "*",  TokenType.MultiplyOperator },
            { "xor", TokenType.XorBoolean },
            { "not", TokenType.NotBoolean },
            { "and", TokenType.AndBoolean },
            { "mod", TokenType.ModuloOperator },
            { "shr", TokenType.ShiftBitwiseRight },
            { "shl", TokenType.ShiftBitwiseLeft },
        };

        public static readonly Dictionary<string, TokenType> LiteralSyntax = new Dictionary<string, TokenType>
        {
            { "true", TokenType.TrueBoolean },
            { "false", TokenType.FalseBoolean },
            { "nil", TokenType.NullKeyword },
        };

        public static readonly Dictionary<string, TokenType> SeperatorSyntax = new Dictionary<string, TokenType>
        {
            { ",",  TokenType.Comma },
            { ";",  TokenType.SemiColon },
            { ":",  TokenType.Colon },
            { "(",  TokenType.OpenParathesis },
            { ")",  TokenType.ClosedParathesis },
            { "[",  TokenType.OpenBracket },
            { "]",  TokenType.ClosedBracket },
        };

        public static readonly string MatchStringLiteralRegex = "\"[^\\\"]*\"";
        public static readonly string MatchSingleQuoteStringLiteralRegex = @"'[^\']*'";
        public static readonly string MatchSingleLineCommentRegex = "(\\/{2})([^\n\r]+)";
        public static readonly string MatchSingleLineDocCommentRegex = "(\\/{3})([^\n\r]+)";
    }
}