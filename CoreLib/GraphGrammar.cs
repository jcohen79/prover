
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace GrammarDLL
{
    // Control spacing that goes between tokens
    public enum SpacingType
    {
        spcIdentifier,
        spcOperator,
        spcLeft,
        spcRight,
        spcDot,
        spcComma,
        spcColon,
        spcKeyword,
        spcPush,   // invisible token that marks start of nonterminal
        spcPop     // invisible token that marks end of nonterminal
    }
    
    public interface Ils
    {

    }

    public class TokenInfo
    {

        public static TokenInfo infoLatest;
        public TokenInfo infoPrev;
        public Terminal term;
        public readonly string text;
        public Ils ilsLiteral;
        public readonly SpacingType spacing;
        public readonly bool fStartNewLine;
        public readonly bool fConstant;

        public TokenInfo(string text, SpacingType spacing, bool fStartNewLine = false, bool fConstant = true)
        {
            this.text = text;
            this.spacing = spacing;
            this.fStartNewLine = fStartNewLine;
            this.fConstant = fConstant;
            infoPrev = infoLatest;
            infoLatest = this;
        }

        public TokenInfo(Terminal term, SpacingType spacing, Ils ilsLiteral = null, bool fStartNewLine = false, bool fConstant = true)
        {
            this.term = term;
            this.spacing = spacing;
            this.ilsLiteral = ilsLiteral;
            this.fStartNewLine = fStartNewLine;
            this.fConstant = fConstant;
        }

        public TokenInfo(SpacingType spacing, bool fStartNewLine = false, bool fConstant = true)
        {
            this.spacing = spacing;
            this.fStartNewLine = fStartNewLine;
            this.fConstant = fConstant;
        }


        public static void AddToGrammar(ExpressionEvaluatorGrammar grammar)
        {
            while (infoLatest != null)
            {
                TokenInfo info = TokenInfo.infoLatest;
                if (info.text.StartsWith("\\"))
                    grammar.MarkReservedWords(info.text);
                info.term = grammar.ToTerm(info.text);
                infoLatest = info.infoPrev;

                info.infoPrev = grammar.tokens;
                grammar.tokens = info;
            }
        }

        public Terminal K { get { return term; } }
    }

    // For documentation, see http://en.wikibooks.org/wiki/Irony_-_Language_Implementation_Kit
    [Language("ExpressionEvaluator", "1.0", "Multi-line expression evaluator")]
    public class ExpressionEvaluatorGrammar : InterpretedLanguageGrammar
    {
        static TokenInfo token(string text, SpacingType spacing, bool fStartNewLine = false)
        {
            return new TokenInfo(text, spacing, fStartNewLine);
        }
        public TokenInfo tokens;

        public bool fBuildAst = false;

            public NumberLiteral number = new NumberLiteral("number");
            public IdentifierTerminal identifier = new IdentifierTerminal("identifier");
            public RegexBasedTerminal latexIdentifier = new RegexBasedTerminal("latexIdentifier", @"\\\w+", @"\");

            public CommentTerminal commentToEOL = new CommentTerminal("comment", "#", "\n", "\r");  // comment from # to EOL
            //comment must be added to NonGrammarTerminals list; it is not used directly in grammar rules,
            // so we add it to this list to let Scanner know that it is also a valid terminal. 
            public KeyTerm lineSplit = new KeyTerm(@"\\", "backback");

            //String literal with embedded expressions  ------------------------------------------------------------------
            public StringLiteral stringLit = new StringLiteral("string", "\"", StringOptions.AllowsAllEscapes); // | StringOptions.IsTemplate);

            public NonTerminal Expr = new NonTerminal("Expr"); //declare it here to use in template definition 

            public StringTemplateSettings templateSettings = new StringTemplateSettings(); //by default set to Ruby-style settings 

            public TokenInfo minus = token("-", SpacingType.spcOperator);
            public TokenInfo minusMinus = token("--", SpacingType.spcOperator);
            public TokenInfo exclamation = token("!", SpacingType.spcOperator);
            public TokenInfo exclamationEquals = token("!=", SpacingType.spcOperator);
            public TokenInfo ampersand = token("&", SpacingType.spcOperator);
            public TokenInfo ampersandAmpersand = token("&&", SpacingType.spcOperator);
            public TokenInfo leftParenthesis = token("(", SpacingType.spcLeft);
            public TokenInfo rightParenthesis = token(")", SpacingType.spcRight);
            public TokenInfo asterisk = token("*", SpacingType.spcOperator);
            public TokenInfo asteriskAsterisk = token("**", SpacingType.spcOperator);
            public TokenInfo asteriskEquals = token("*=", SpacingType.spcOperator);
            public TokenInfo comma = token(",", SpacingType.spcComma);
            public TokenInfo period = token(".", SpacingType.spcDot);
            public TokenInfo divide = token("/", SpacingType.spcOperator);
            public TokenInfo divideEquals = token("/=", SpacingType.spcOperator);
            public TokenInfo colon = token(":", SpacingType.spcColon);
            public TokenInfo semicolon = token(";", SpacingType.spcComma);
            public TokenInfo question = token("?", SpacingType.spcOperator);
            public TokenInfo atsign = token("@", SpacingType.spcDot);
            public TokenInfo leftSquare = token("[", SpacingType.spcLeft);
            public TokenInfo backLeftBrace = token("\\{", SpacingType.spcKeyword, fStartNewLine:true);
            public TokenInfo backRightBrace = token("\\}", SpacingType.spcKeyword, fStartNewLine: true);
            public TokenInfo backBox = token("\\box", SpacingType.spcOperator);
            public TokenInfo backExists = token("\\exists", SpacingType.spcKeyword);
            public TokenInfo backForall = token("\\forall", SpacingType.spcKeyword);
            public TokenInfo backLand = token("\\land", SpacingType.spcOperator);
            public TokenInfo backleftrightarrow = token("\\leftrightarrow", SpacingType.spcOperator);
            public TokenInfo backLeftrightarrow = token("\\Leftrightarrow", SpacingType.spcOperator);
            public TokenInfo backLor = token("\\lor", SpacingType.spcOperator);
            public TokenInfo backNeg = token("\\neg", SpacingType.spcOperator);
            public TokenInfo backTo = token("\\to", SpacingType.spcOperator);
            public TokenInfo backNexists = token("\\nexists", SpacingType.spcKeyword);
            public TokenInfo backrightarrow = token("\\rightarrow", SpacingType.spcOperator);
            public TokenInfo backRightarrow = token("\\Rightarrow", SpacingType.spcOperator);
            public TokenInfo rightSquare = token("]", SpacingType.spcRight);
            public TokenInfo caret = token("^", SpacingType.spcDot);
            public TokenInfo leftBrace = token("{", SpacingType.spcLeft);
            public TokenInfo bar = token("|", SpacingType.spcOperator);
            public TokenInfo barBar = token("||", SpacingType.spcOperator);
            public TokenInfo rightBrace = token("}", SpacingType.spcRight);
            public TokenInfo tilde = token("~", SpacingType.spcOperator);
            public TokenInfo plus = token("+", SpacingType.spcOperator);
            public TokenInfo plusPlus = token("++", SpacingType.spcOperator);
            public TokenInfo plusEquals = token("+=", SpacingType.spcOperator);
            public TokenInfo less = token("<", SpacingType.spcOperator);
            public TokenInfo lessEquals = token("<=", SpacingType.spcOperator);
            public TokenInfo equals = token("=", SpacingType.spcOperator);
            public TokenInfo minusEquals = token("-=", SpacingType.spcOperator);
            public TokenInfo colonEquals = token(":=", SpacingType.spcOperator);
            public TokenInfo greater = token(">", SpacingType.spcOperator);
            public TokenInfo greaterEquals = token(">=", SpacingType.spcOperator);


            public TokenInfo tCorrespondence = token("correspondence", SpacingType.spcKeyword, fStartNewLine: true);
            public TokenInfo tRule = token("rule", SpacingType.spcKeyword, fStartNewLine: true);
            public TokenInfo tSequent = token("sequent", SpacingType.spcKeyword, fStartNewLine: true);
            public TokenInfo tTo = token("to", SpacingType.spcKeyword, fStartNewLine: true);
            public TokenInfo tUsing = token("using", SpacingType.spcKeyword, fStartNewLine: true);

            // 2. Non-terminals
            public NonTerminal Term = new NonTerminal("Term");
            public NonTerminal BinExpr = new NonTerminal("BinExpr");
            public NonTerminal LogicExpr = new NonTerminal("LogicExpr");
            public NonTerminal LogicExpr1 = new NonTerminal("LogicExpr1");
            public NonTerminal LogicExpr2Seq = new NonTerminal("LogicExpr2Seq");
            public NonTerminal LogicExpr2List = new NonTerminal("LogicExpr2List");
            public NonTerminal LogicExpr2Pair = new NonTerminal("LogicExpr2Pair");
            public NonTerminal LogicExpr2 = new NonTerminal("LogicExpr2");
            public NonTerminal LogicExpr3 = new NonTerminal("LogicExpr3");
            public NonTerminal LogicExpr4 = new NonTerminal("LogicExpr4");
            public NonTerminal LogicExpr5 = new NonTerminal("LogicExpr5");
            public NonTerminal LogicTerm = new NonTerminal("LogicTerm");
            public NonTerminal BiEntailsExpr = new NonTerminal("BiEntailsExpr");
            public NonTerminal EntailsExpr = new NonTerminal("EntailsExpr");
            public NonTerminal EquivExpr = new NonTerminal("EquivExpr");
            public NonTerminal ImpliesExpr = new NonTerminal("ImpliesExpr");
            public NonTerminal DisjunctionExpr = new NonTerminal("DisjunctionExpr");
            public NonTerminal ConjunctionExpr = new NonTerminal("ConjunctionExpr");

            public NonTerminal QuadTerm = new NonTerminal("QuadTerm");
            public NonTerminal QuadTermPair = new NonTerminal("QuadTermPair");
            public NonTerminal QuadTermList = new NonTerminal("QuadTermList");
            public NonTerminal QuadTermSeq = new NonTerminal("QuadTermSeq");
            public NonTerminal Quad = new NonTerminal("Quad");
            public NonTerminal QuadPair = new NonTerminal("QuadPair");
            public NonTerminal QuadList = new NonTerminal("QuadList");
            public NonTerminal Sequent = new NonTerminal("Sequent");

            public NonTerminal QuadCall = new NonTerminal("QuadCall");
            public NonTerminal CodeTerm = new NonTerminal("CodeTerm");
            public NonTerminal CodePair = new NonTerminal("CodePair");
            public NonTerminal CodeSeq = new NonTerminal("CodeSeq");
            public NonTerminal CodeRule = new NonTerminal("CodeRule");

            public NonTerminal ListContent = new NonTerminal("ListContent");
            public NonTerminal ListElement = new NonTerminal("ListElement");
            public NonTerminal OneItemList = new NonTerminal("OneItemList");
            public NonTerminal ItemAndList = new NonTerminal("ItemAndList");
            public NonTerminal EmptyList = new NonTerminal("EmptyList");  // results in nil in Lsx
            public NonTerminal NoList = new NonTerminal("NoList");   // nothing in Lsx
            public NonTerminal NonEmptyList = new NonTerminal("NonEmptyList");
            public NonTerminal ParExpr = new NonTerminal("ParExpr");
            public NonTerminal NegationExpr = new NonTerminal("NegationExpr");
            public NonTerminal NamedQuantifier = new NonTerminal("NamedQuantifier");
            public NonTerminal QuantId = new NonTerminal("QuantId");
            public NonTerminal QuantifiedExpr = new NonTerminal("Quantified");
            public NonTerminal Quantifier = new NonTerminal("Quantifier");
            public NonTerminal Quantifier1 = new NonTerminal("Quantifier1");
            public NonTerminal Quantifier2 = new NonTerminal("Quantifier2");
            public NonTerminal Quantifier3 = new NonTerminal("Quantifier3");
            public NonTerminal Quantifier4 = new NonTerminal("Quantifier4");
            public NonTerminal ListExpr = new NonTerminal("ListExpr");
            public NonTerminal UnExpr = new NonTerminal("UnExpr");
            public NonTerminal TernaryIfExpr = new NonTerminal("TernaryIf");
            public NonTerminal ArgList = new NonTerminal("ArgList");
            public NonTerminal VertexExpr = new NonTerminal("VertexExpr");
            public NonTerminal VertexExpr1 = new NonTerminal("VertexExpr1");
            public NonTerminal VertexExpr2 = new NonTerminal("VertexExpr2");
            public NonTerminal EdgeExpr = new NonTerminal("EdgeExpr");
            public NonTerminal EdgeExpr1 = new NonTerminal("EdgeExpr1");
            public NonTerminal EdgeExpr2 = new NonTerminal("EdgeExpr2");
            public NonTerminal VertexMatchExpr = new NonTerminal("VertexMatchExpr");
            public NonTerminal EdgeMatchExpr = new NonTerminal("EdgeMatchExpr");

            public NonTerminal FunctionCall = new NonTerminal("FunctionCall");
            public NonTerminal MemberAccess = new NonTerminal("MemberAccess");
            public NonTerminal IndexedAccess = new NonTerminal("IndexedAccess");
            public NonTerminal ObjectRef = new NonTerminal("ObjectRef"); // foo, foo.bar or f['bar']
            public NonTerminal UnOp = new NonTerminal("UnOp");
            public NonTerminal BinOp = new NonTerminal("BinOp", "operator");
            public NonTerminal PrefixIncDec = new NonTerminal("PrefixIncDec");
            public NonTerminal PostfixIncDec = new NonTerminal("PostfixIncDec");
            public NonTerminal IncDecOp = new NonTerminal("IncDecOp");
            public NonTerminal AssignmentStmt = new NonTerminal("AssignmentStmt");
            public NonTerminal AssignmentOp = new NonTerminal("AssignmentOp", "assignment operator");
            public NonTerminal ArgPair = new NonTerminal("ArgPair");
            public NonTerminal ArgSeq = new NonTerminal("ArgSeq");
            public NonTerminal Correspondence = new NonTerminal("Correspondence");
            public NonTerminal Statement = new NonTerminal("Statement");
            public NonTerminal StmtAndProgram = new NonTerminal("StmtAndProgram");
            public NonTerminal Program = new NonTerminal("Program");


        public ExpressionEvaluatorGrammar()
            : base(caseSensitive: true)
        {

            this.GrammarComments =
                @"Graph matching.";
            // 1. Terminals
            //Let's allow big integers (with unlimited number of digits):
            number.DefaultIntTypes = new System.TypeCode[] { System.TypeCode.Int32, System.TypeCode.Int64, NumberLiteral.TypeCodeBigInt };
            latexIdentifier.AstConfig.NodeType = typeof(IdentifierNode);
            base.NonGrammarTerminals.Add(commentToEOL);
            base.NonGrammarTerminals.Add(lineSplit);
            stringLit.AddStartEnd("'", StringOptions.AllowsAllEscapes | StringOptions.IsTemplate);
            templateSettings.ExpressionRoot = Expr; //this defines how to evaluate expressions inside template
            this.SnippetRoots.Add(Expr);
            stringLit.AstConfig.Data = templateSettings;

            TokenInfo.AddToGrammar(this);

            // 3. BNF rules
            LogicExpr.Rule = LogicExpr1 | BiEntailsExpr;
            LogicExpr1.Rule = LogicExpr2 | EntailsExpr;
            LogicExpr2Seq.Rule = NoList | LogicExpr2List;
            LogicExpr2List.Rule = LogicExpr2 | LogicExpr2Pair;
            LogicExpr2Pair.Rule = LogicExpr2 + comma.K + LogicExpr2List;
            LogicExpr2.Rule = LogicExpr3 | EquivExpr;
            LogicExpr3.Rule = LogicExpr4 | ImpliesExpr;
            LogicExpr4.Rule = LogicExpr5 | DisjunctionExpr;
            LogicExpr5.Rule = LogicTerm | ConjunctionExpr;
            LogicTerm.Rule = NegationExpr | QuantifiedExpr | Expr;
            Expr.Rule = Term | BinExpr | UnExpr | PrefixIncDec | PostfixIncDec | TernaryIfExpr;
            Term.Rule = number | ParExpr | stringLit | FunctionCall | identifier | latexIdentifier | MemberAccess | IndexedAccess;

            BiEntailsExpr.Rule = LogicExpr + backLeftrightarrow.K + LogicExpr1;
            EntailsExpr.Rule = LogicExpr2Seq + backRightarrow.K + LogicExpr2Seq;
            EquivExpr.Rule = LogicExpr2 + backleftrightarrow.K + LogicExpr3;
            ImpliesExpr.Rule = LogicExpr3 + backrightarrow.K + LogicExpr4;
            DisjunctionExpr.Rule = LogicExpr4 + backLor.K + LogicExpr5 | LogicExpr4 + backTo.K + LogicExpr5;
            ConjunctionExpr.Rule = LogicExpr5 + backLand.K + LogicTerm;
            NegationExpr.Rule = backNeg.K + LogicTerm;
            NamedQuantifier.Rule = identifier + atsign.K + identifier;
            QuantId.Rule = identifier | NamedQuantifier;
            QuantifiedExpr.Rule = Quantifier + QuantId + LogicTerm;
            Quantifier.Rule = backForall.K | backExists.K | backNexists.K;
            //Quantifier3.Rule = ExistsTerm + "!";

            QuadCall.Rule = identifier + leftParenthesis.K + QuadTermSeq + rightParenthesis.K;
            QuadTerm.Rule = identifier | latexIdentifier | QuadCall;
            QuadTermPair.Rule = QuadTerm + QuadTermList;
            QuadTermList.Rule = QuadTerm | QuadTermPair;
            EmptyList.Rule = new BnfExpression();
            NoList.Rule = new BnfExpression();
            QuadTermSeq.Rule = EmptyList | QuadTermList;
            Quad.Rule = QuadTermSeq + backBox.K + QuadTermSeq;
            QuadPair.Rule = QuadList + comma.K + Quad;
            QuadList.Rule = Quad | QuadPair;
            Sequent.Rule = tSequent.K + QuadList;

            CodeTerm.Rule = identifier | latexIdentifier | BinOp | AssignmentOp | semicolon.K | colon.K | leftParenthesis.K | rightParenthesis.K;
            CodePair.Rule = CodeTerm + CodeSeq;
            CodeSeq.Rule = CodeTerm | CodePair;
            CodeRule.Rule = tRule.K + LogicExpr + backLeftBrace.K + CodeSeq + backRightBrace.K + LogicExpr;


            VertexExpr1.Rule = identifier + atsign.K + Expr;
            VertexExpr2.Rule = identifier + atsign.K;
            // VertexExprRef.Rule = identifier + "$";
            VertexExpr.Rule = VertexExpr1 | VertexExpr2;
            EdgeExpr1.Rule = identifier + tilde.K + identifier + caret.K + Expr;
            EdgeExpr2.Rule = identifier + tilde.K + identifier;
            EdgeExpr.Rule = EdgeExpr1 | EdgeExpr2;
            VertexMatchExpr.Rule = identifier + leftBrace.K + ListContent + rightBrace.K;
            EdgeMatchExpr.Rule = EdgeExpr + leftBrace.K + ListContent + rightBrace.K;
            ListElement.Rule = VertexExpr | EdgeExpr | identifier | VertexMatchExpr | EdgeMatchExpr | ListExpr | AssignmentStmt;
            OneItemList.Rule = ListElement;
            ItemAndList.Rule = ListElement + comma.K + NonEmptyList;
            NonEmptyList.Rule = OneItemList | ItemAndList;
            ListContent.Rule = Empty | NonEmptyList;
            ListExpr.Rule = leftBrace.K + ListContent + rightBrace.K;

            ParExpr.Rule = leftParenthesis.K + LogicExpr + rightParenthesis.K;
            UnExpr.Rule = UnOp + Term + ReduceHere();
            UnOp.Rule = plus.K | minus.K | exclamation.K;
            BinExpr.Rule = Expr + BinOp + Expr;
            BinOp.Rule = plus.K | minus.K | asterisk.K | divide.K | asteriskAsterisk.K | equals.K | less.K | lessEquals.K | greater.K
                    | greaterEquals.K | exclamationEquals.K | ampersandAmpersand.K | barBar.K | ampersand.K | bar.K ;
            PrefixIncDec.Rule = IncDecOp + identifier;
            PostfixIncDec.Rule = identifier + PreferShiftHere() + IncDecOp;
            IncDecOp.Rule = plusPlus.K | minusMinus.K;
            TernaryIfExpr.Rule = Expr + question.K + Expr + colon.K + Expr;
            MemberAccess.Rule = Expr + PreferShiftHere() + period.K + PreferShiftHere() + identifier;
            AssignmentStmt.Rule = ObjectRef + AssignmentOp + Expr;
            AssignmentOp.Rule = colonEquals.K | plusEquals.K | minusEquals.K | asteriskEquals.K | divideEquals.K ;
            ArgPair.Rule = Expr + comma.K + ArgSeq;
            ArgSeq.Rule = Expr | ArgPair;
            ArgList.Rule = NoList | ArgSeq;
            FunctionCall.Rule = Expr + PreferShiftHere() + leftParenthesis.K + ArgList + rightParenthesis.K;
            FunctionCall.NodeCaptionTemplate = "call #{0}(...)";
            ObjectRef.Rule = identifier | MemberAccess | IndexedAccess;
            IndexedAccess.Rule = Expr + PreferShiftHere() + leftSquare.K + Expr + rightSquare.K;

            Correspondence.Rule = tCorrespondence.K + identifier + ListExpr + tTo.K + ListExpr + tUsing.K + ListExpr;

            Statement.Rule = AssignmentStmt | LogicExpr | ListExpr | Sequent | CodeRule | Correspondence | Empty;
            StmtAndProgram.Rule = Statement + semicolon.K + Program;
            Program.Rule = Statement | StmtAndProgram;
            // MakePlusRule(Program, NewLine, Statement);

            this.Root = Program; // Set grammar root

            // 4. Operators precedence
            RegisterOperators(5, "@", "~", "$");
            RegisterOperators(10, "?");
            RegisterOperators(15, "&", "&&", "|", "||");
            RegisterOperators(20, "=", "<", "<=", ">", ">=", "!=");
            RegisterOperators(30, "+", "-");
            RegisterOperators(40, "*", "/");
            RegisterOperators(50, Associativity.Right, "**");
            RegisterOperators(60, "!", "\\");
            // For precedence to work, we need to take care of one more thing: BinOp. 
            //For BinOp which is or-combination of binary operators, we need to either 
            // 1) mark it transient or 2) set flag TermFlags.InheritPrecedence
            // We use first option, making it Transient.  

            // 5. Punctuation and transient terms
            MarkPunctuation("(", ")", "?", ":", "[", "]", "{", "}", ",");
            RegisterBracePair("(", ")");
            RegisterBracePair("[", "]");
            RegisterBracePair("{", "}");
            MarkTransient(Term, Expr, Statement, BinOp, UnOp, IncDecOp, AssignmentOp, ParExpr, ObjectRef,
                          ListContent, VertexExpr, ListElement, EdgeExpr, NonEmptyList, ArgSeq);
            // ListExpr

            // MarkReservedWords("\\neg", "\\forall");

            // 7. Syntax error reporting
            MarkNotReported("++", "--");
            AddToNoReportGroup("(", "++", "--");
            AddToNoReportGroup(NewLine);
            AddOperatorReportGroup("operator");
            AddTermsReportGroup("assignment operator", ":=", "+=", "-=", "*=", "/=");

            //8. Console
            ConsoleTitle = "Graph Expression Evaluator";
            ConsoleGreeting =
                @"Graph Expression Evaluator 

  Supports variable assignments, arithmetic operators (+, -, *, /),
    augmented assignments (+=, -=, etc), prefix/postfix operators ++,--, string operations. 
  Supports big integer arithmetics, string operations.
  Supports strings with embedded expressions : ""name: #{name}""
  Supports graphs as a list of vertices - vertexName@valueExpr and edges - vertexExpr~vertexExpr^descExpr. Enclose a list in { , }

Press Ctrl-C to exit the program at any time.
";
            ConsolePrompt = "?";
            ConsolePromptMoreInput = "?";

            //9. Language flags. 
            // Automatically add NewLine before EOF so that our BNF rules work correctly when there's no final line break in source
            this.LanguageFlags = LanguageFlags.NewLineBeforeEOF | LanguageFlags.CreateAst | LanguageFlags.SupportsBigInt;
        }

        public override void BuildAst(LanguageData language, ParseTree parseTree)
        {
            // PrintSubtree(parseTree.Root);
            if (fBuildAst)
                base.BuildAst(language, parseTree);
        }
    }
}