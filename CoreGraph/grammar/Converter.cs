
using System.Collections.Generic;
using System.Text;
using GraphMatching;
using Irony.Parsing;
using GrammarDLL;
using reslab;
/// <summary>
/// Convert parse tree into graph, including codegen for output.
/// </summary>
namespace ConsoleApplication1
{

    public class GraphConverter<V, E> where V : class
    {
        IGraphGrammarBuilder<V, E> builder;
        CommentTerminal comment;

        public GraphConverter(IGraphGrammarBuilder<V, E> builder)
        {
            this.builder = builder;
            this.comment = builder.getComment();
        }

        public V BuildGraph(ParseTree parseTree)
        {
            vPrev = default(V);
            V vRoot = BuildSubtree(parseTree.Root);
            return vRoot;
        }

        private V vPrev;
        public V BuildSubtree(ParseTreeNode node)
        {

            if (node.Comments != null)
            {
                foreach (var c in node.Comments)
                {
                    SourceLocation sl = c.Location;
                    vPrev = builder.vertexNonGrammar(vPrev, comment, sl.Line, sl.Column, c.Length, c.Text);
                }
            }

            V vNode = default(V);

            if (node.Term is Terminal)
            {
                SourceLocation sl = node.Span.Location;
                vNode = builder.vertexTerminal(vPrev, node.Term, sl.Line, sl.Column, node.Span.Length, node.Token.Text);
                vPrev = vNode;
            }
            else 
                vNode = builder.vertexNonterminal(node.Term);
            
            ParseTreeNodeList list = node.ChildNodes;
            int cChild = 0;
            foreach (var child in list)
            {
                V vChild = BuildSubtree(child);
                builder.edgeForTerm(vNode, vChild, node.Term, cChild);
                cChild++;
                if (vNode == default(V))
                    vNode = vChild;  // flatten tree
            }
            return vNode;
        }

        int iIndent = 0;

        public void PrintSubtree(ParseTreeNode node)
        {
            iIndent += 2;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < iIndent; i++)
            {
                sb.Append(' ');
            }
            sb.Append(node.Span.Location.ToString() + "." + node.Span.Length);
            if (node.Term != null)
                sb.Append(node.Term.ToString());
            if (node.Token != null)
                sb.Append(" token: " + node.Token.ToString());
            if (node.Comments != null)
            {
                sb.Append(" Comments:");
                foreach (var c in node.Comments)
                    sb.Append(" " + c.Text);
            }
            System.Console.WriteLine(sb.ToString());
            ParseTreeNodeList list = node.ChildNodes;
            foreach (var child in list)
            {
                PrintSubtree(child);
            }
            iIndent -= 2;
        }
    }


    public class ParseGraphBuilder  : IGraphGrammarBuilder<Vertex, Edge>
    {
        public readonly Graph graph;
        public readonly Restrictor parseTreeContext;
        private ExpressionEvaluator ee;
        public readonly ExpressionEvaluatorGrammar grammar;
        public readonly EmitConfig ec;

        public static TokenInfo tokenIdentifier;
        public static TokenInfo tokenLatexIdentifier;
        public static TokenInfo tokenNumber;
        public static TokenInfo tokenString;


        public static ColorDef colorParseBnfEdge = ColorDef.Red;
        public static ColorDef colorBnfDefEdge = ColorDef.Yellow;
        public static ColorDef colorNextTokenEdge = ColorDef.Gray;
        public static ColorDef colorParseTree = ColorDef.Black;
        public static ColorDef colorCodeGen = ColorDef.Blue;
        public static ColorDef colorLsxGen = ColorDef.Purple;

        public static EdgeDescriptor edParseBnf = new EdgeDescriptor("edParseBnf", colorParseBnfEdge);
        public static EdgeDescriptor edParseCar = new EdgeDescriptor("edParseCar", colorParseTree);
        public static EdgeDescriptor edParseCdr = new EdgeDescriptor("edParseCdr", colorParseTree);
        public static EdgeDescriptor edParseArg = new EdgeDescriptor("edParseArg", colorParseTree);
        public static EdgeDescriptor edParseName = new EdgeDescriptor("edParseName", colorParseTree);
        public static EdgeDescriptor edParseLhs = new EdgeDescriptor("edParseLhs", colorParseTree);
        public static EdgeDescriptor edParseOp = new EdgeDescriptor("edParseOp", colorParseTree);
        public static EdgeDescriptor edParseRhs = new EdgeDescriptor("edParseRhs", colorParseTree);
        public static EdgeDescriptor edParseCode = new EdgeDescriptor("edParseCode", colorParseTree);
        public static EdgeDescriptor edBnfDef = new EdgeDescriptor("edBnfDef", colorBnfDefEdge);
        public static EdgeDescriptor edNextTokenEdge = new EdgeDescriptor("edNextTokenEdge", colorNextTokenEdge);
        public static EdgeDescriptor edCodeGen = new EdgeDescriptor("edCodeGen", colorCodeGen);
        public static EdgeDescriptor edLsxGen = new EdgeDescriptor("edLsxGen", colorLsxGen);

        public Dictionary<BnfTerm, TermInfo> mpbnf_termInfo = new Dictionary<BnfTerm, TermInfo>();
        public Vertex vBnfDef;  // identifies term definitions
        public Vertex vCodeGenTemplateSeq;  // payload is path of edgedefs, used to to find template to invoke
        public Vertex vLsxTemplateSeq;
        public Vertex vEmptyList;

        public TokenInfo tokenLabel = new TokenInfo("label", SpacingType.spcKeyword, fConstant:true);
        public Vertex vTokenLabel;

        public TermInfo termIdentifier;
        public TermInfo termLatexIdentifier;
        public TermInfo termNumber;
        public TermInfo termString;
        public TermInfo termLeftParenthesis;
        public TermInfo termRightParenthesis;

        public ParseGraphBuilder(Graph graph, Restrictor parseTreeContext, ExpressionEvaluator ee, EmitConfig ec)
        {
            this.graph = graph;
            this.parseTreeContext = parseTreeContext;
            this.ee = ee;
            this.ec = ec;
            this.grammar = ee.Grammar;
            StartLanguage();
        }

        public void StartLanguage()
        {
            vBnfDef = new Vertex("vBnfDef");
            graph.Add(vBnfDef);
            EdgeDescriptor[] rgCodeGenEdgeDesc = { edParseBnf, edCodeGen };
            vCodeGenTemplateSeq = new Vertex("parseTreeCodeGenTemplateRoleSeq", new ValueLiteral(rgCodeGenEdgeDesc));
            EdgeDescriptor[] rgLsxEdgeDesc = { edParseBnf, edLsxGen };
            vLsxTemplateSeq = new Vertex("parseTreeLsxTemplateRoleSeq", new ValueLiteral(rgLsxEdgeDesc));

            GenConfig configCodeGen = new GenConfigCode("CG", edCodeGen, vCodeGenTemplateSeq);
            GenConfig configLsx = new GenConfigLsx("GLsx", edLsxGen, vLsxTemplateSeq);
            GenConfig configLsxList = new GenConfigLsxList("GLsxl", edLsxGen, vLsxTemplateSeq);

            GenConfigSet gcsStd = new GenConfigSet(configCodeGen, configLsx);
            GenConfigSet gcsList = new GenConfigSet(configCodeGen, configLsxList);

            tokenIdentifier = new TokenInfo(grammar.identifier, SpacingType.spcIdentifier, ilsLiteral:Lsm.lsmIdentifier, fConstant: false);
            tokenLatexIdentifier = new TokenInfo(grammar.latexIdentifier, SpacingType.spcIdentifier, ilsLiteral: Lsm.lsmIdentifier, fConstant: false);
            tokenNumber = new TokenInfo(grammar.number, SpacingType.spcIdentifier, ilsLiteral: Lsm.lsmIdentifier, fConstant: false);
            tokenString = new TokenInfo(grammar.stringLit, SpacingType.spcIdentifier, fConstant: false);

            termIdentifier = DefineTerminal(gcsStd, tokenIdentifier);
            termLatexIdentifier = DefineTerminal(gcsStd, tokenLatexIdentifier);
            termNumber = DefineTerminal(gcsStd, tokenNumber);
            termString = DefineTerminal(gcsStd, tokenString);

            vTokenLabel = new Vertex("vTokenLabel", new ValueLiteral(tokenLabel));

            termLeftParenthesis = DefineTerminal(gcsStd, grammar.leftParenthesis);
            termRightParenthesis = DefineTerminal(gcsStd, grammar.rightParenthesis);

            TokenInfo info = grammar.tokens;
            while (info != null)
            {
                DefineTerminal(gcsStd, info);
                info = info.infoPrev;
            }
            EdgeDescriptor[] rgedInfix = new EdgeDescriptor[] { edParseLhs, edParseOp, edParseRhs };
            EdgeDescriptor[] rgedPrefix = new EdgeDescriptor[] { edParseOp, edParseLhs, edParseRhs };

            DefineNonterminal(gcsStd, grammar.BiEntailsExpr, rgedInfix, rgedPrefix);
            DefineNonterminal(gcsStd, grammar.LogicExpr2Seq, edParseArg);
            DefineNonterminal(gcsList, grammar.LogicExpr2Pair, edParseLhs, edParseRhs);
            DefineNonterminal(gcsStd, grammar.EntailsExpr, rgedInfix, rgedPrefix);
            DefineNonterminal(gcsStd, grammar.EquivExpr, rgedInfix, rgedPrefix);
            DefineNonterminal(gcsStd, grammar.ImpliesExpr, rgedInfix, rgedPrefix);
            DefineNonterminal(gcsStd, grammar.DisjunctionExpr, rgedInfix, rgedPrefix);
            DefineNonterminal(gcsStd, grammar.ConjunctionExpr, rgedInfix, rgedPrefix);
            DefineNonterminal(gcsStd, grammar.NegationExpr, edParseOp, edParseArg);
            DefineNonterminal(gcsStd, grammar.NamedQuantifier, edParseLhs, null, edParseRhs);
            DefineNonterminal(gcsStd, grammar.QuantifiedExpr, edParseOp, edParseLhs, edParseRhs);
            DefineNonterminal(gcsStd, grammar.QuadCall, edParseLhs, edParseRhs);   // parenthesis do not show up in parse tree?
            DefineNonterminal(gcsStd, grammar.QuadTermPair, edParseCar, edParseCdr);
            DefineNonterminal(gcsStd, grammar.EmptyList);
            DefineNonterminal(gcsList, grammar.NoList);
            DefineNonterminal(gcsStd, grammar.QuadPair, edParseLhs, edParseRhs);  // comma not in parse tree
            DefineNonterminal(gcsStd, grammar.Quad, edParseLhs, null, edParseRhs);
            DefineNonterminal(gcsStd, grammar.Sequent, null, edParseArg);
            DefineNonterminal(gcsList, grammar.CodePair, edParseCar, edParseCdr);
            DefineNonterminal(gcsStd, grammar.CodeRule, null, edParseLhs, null, edParseCode, null, edParseRhs);
            DefineNonterminal(gcsStd, grammar.VertexExpr1, edParseLhs, null, edParseRhs);
            DefineNonterminal(gcsStd, grammar.VertexExpr2, edParseLhs, null);
            DefineNonterminal(gcsStd, grammar.EdgeExpr1, edParseLhs, null, edParseRhs, null, edParseArg);  // is in parse tree
            DefineNonterminal(gcsStd, grammar.EdgeExpr2, edParseLhs, null, edParseRhs);  // is in parse tree
            DefineNonterminal(gcsStd, grammar.VertexMatchExpr, edParseLhs, edParseRhs, null);  // not in parse tree
            DefineNonterminal(gcsStd, grammar.EdgeMatchExpr, edParseLhs, edParseRhs);  // not in parse tree
            DefineNonterminal(gcsList, grammar.OneItemList, edParseCar);
            DefineNonterminal(gcsList, grammar.ItemAndList, edParseCar, edParseCdr);  // comma not in parse tree
            DefineNonterminal(gcsStd, grammar.ListExpr, edParseArg);  // braces not in parse tree
            DefineNonterminal(gcsStd, grammar.ParExpr, edParseArg);   // parens not in parse tree
            DefineNonterminal(gcsStd, grammar.UnExpr, edParseOp, edParseArg);
            DefineNonterminal(gcsStd, grammar.BinExpr, rgedInfix, rgedPrefix);
            DefineNonterminal(gcsStd, grammar.PrefixIncDec, edParseOp, edParseArg);
            DefineNonterminal(gcsStd, grammar.PostfixIncDec, edParseArg, edParseOp);
            DefineNonterminal(gcsStd, grammar.TernaryIfExpr, edParseArg, null, edParseLhs, null, edParseRhs);
            DefineNonterminal(gcsStd, grammar.MemberAccess, edParseLhs, null, edParseLhs);
            DefineNonterminal(gcsStd, grammar.AssignmentStmt, rgedInfix, rgedPrefix);
            DefineNonterminal(gcsList, grammar.ArgPair, edParseCar, edParseCdr);
            DefineNonterminal(gcsStd, grammar.FunctionCall, edParseLhs, edParseRhs);
            DefineNonterminal(gcsStd, grammar.IndexedAccess, edParseLhs, null, edParseRhs);
            DefineNonterminal(gcsStd, grammar.Correspondence, null, edParseName, edParseLhs, null, edParseRhs, null, edParseArg);
            DefineNonterminal(gcsList, grammar.StmtAndProgram, edParseCar, null, edParseCdr);

            DefineEmptyList();
        }

        void DefineEmptyList()
        {
            vEmptyList = new Vertex("EmptyList");

            Vertex vDef = new Vertex("defEmptyList", new ValueLiteral("EmptyList"));
            Edge eDef = new Edge(edBnfDef, vDef, vBnfDef, parseTreeContext);
            graph.Add(vDef);

            Vertex vStart = new Vertex("CGEmptyList", new ValueLiteral("EmptyList"));
            graph.Add(vStart);
            Edge eDef2 = new Edge(edCodeGen, vDef, vStart, parseTreeContext);
            graph.Add(eDef2);
            // no contents
        }

        

        void MakeCodeGenForTerminal(GenConfig config, BnfTerm term, Vertex vDef, bool fConstant)
        {
            Vertex vStart = new Vertex(config.stPrefix + term.Name, new ValueLiteral(term.Name));
            graph.Add(vStart);
            Edge eDef = new Edge(config.edGetTemplate, vDef, vStart, parseTreeContext);
            graph.Add(eDef);
            Edge eToken = new Edge(ec.edTokenInfo, vStart, vDef, parseTreeContext);
            graph.Add(eToken);

            Vertex vEmitAtom = fConstant ? ec.vEmitLiteral : ec.vEmitBasisPayload;
            Edge e1 = new Edge(ec.edEmitAtom, vStart, vEmitAtom, parseTreeContext);
            graph.Add(e1);
            Vertex vPrev = vStart;
        }

        void MakeCodeGenForNonterminal(GenConfig config, BnfTerm term, EdgeDescriptor[] rgEdgedefs, Vertex vDef)
        {
            Vertex vStart = new Vertex(config.stPrefix + term.Name);
            graph.Add(vStart);
            Edge eDef = new Edge(config.edGetTemplate, vDef, vStart, parseTreeContext);
            graph.Add(eDef);
            Vertex vPrev = config.vGenPush(this, vStart);
            config.Header(this, ref vPrev, term);

            foreach (var edChild in rgEdgedefs)
            {
                if (edChild == null)
                    continue;

                string stArg = term.Name + edChild.Name;
                Vertex vStep;
                if (vPrev == null)
                {
                    vStep = vStart;
                }
                else
                {
                    vStep = new Vertex("step" + stArg);
                    Edge e6 = new Edge(ec.edNextStep, vPrev, vStep, parseTreeContext);
                    graph.Add(e6);
                }
                Edge e2 = new Edge(ec.edEmitAtom, vStep, ec.vEmitApply, parseTreeContext);
                graph.Add(e2);
                Edge e3 = new Edge(ec.edEmitFunctionSeq, vStep, config.vFindTemplatePath, parseTreeContext);
                graph.Add(e3);

                Vertex vBasisArg = new Vertex("basis" + stArg, new LookupExpression(ec.vBasis, edChild));
                Edge e4 = new Edge(ec.edArgument, vStep, vBasisArg, parseTreeContext);
                graph.Add(e4);
                Edge e5 = new Edge(ec.edParameterVal, vBasisArg, ec.vBasis, parseTreeContext);
                graph.Add(e5);
                vPrev = vStep;
            }
            config.Footer(this, ref vPrev, term);
            vPrev = config.vGenPop(this, vPrev);
        }

        public TermInfo DefineNonterminal(GenConfigSet genConfigSet, BnfTerm term, params EdgeDescriptor[] edgedefs)
        {
            return DefineNonterminal(genConfigSet, term, edgedefs, edgedefs);
        }

        public TermInfo DefineNonterminal(GenConfigSet genConfigSet, BnfTerm term, EdgeDescriptor[] edgedefsCodeGen, EdgeDescriptor[] edgedefsLsx)
        {
            Vertex vDef = new Vertex("def" + term.Name, new ValueLiteral(term));
            Edge eDef = new Edge(edBnfDef, vDef, vBnfDef, parseTreeContext);
            graph.Add(vDef);
            MakeCodeGenForNonterminal(genConfigSet.gcCode, term, edgedefsCodeGen, vDef);
            MakeCodeGenForNonterminal(genConfigSet.gcLsx, term, edgedefsLsx, vDef);

            TermInfo info = new TermInfo(term, vDef, null, edgedefsCodeGen);
            mpbnf_termInfo.Add(term, info);
            return info;
        }

        public TermInfo DefineTerminal(GenConfigSet genConfigSet, TokenInfo tokenInfo)
        {
            TermInfo termInfo = null;
            if (!mpbnf_termInfo.TryGetValue(tokenInfo.term, out termInfo))
            {
                BnfTerm term = tokenInfo.term;
                Vertex vDef = new Vertex("def" + term.Name, new ValueLiteral(tokenInfo));
                Edge eDef = new Edge(edBnfDef, vDef, vBnfDef, parseTreeContext);
                graph.Add(vDef);
                MakeCodeGenForTerminal(genConfigSet.gcCode, term, vDef, tokenInfo.fConstant);
                MakeCodeGenForTerminal(genConfigSet.gcLsx, term, vDef, tokenInfo.fConstant);

                termInfo = new TermInfo(term, vDef, tokenInfo, null);
                mpbnf_termInfo.Add(term, termInfo);
            }
            return termInfo;
        }

        public CommentTerminal getComment()
        {
            return ee.Grammar.commentToEOL;
        }

        /// <summary>
        /// Create vertex that will have edges to the lower nonterminals and eventually tokens
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public Vertex vertexNonterminal(BnfTerm term)
        {
            TermInfo info = null;
            if (mpbnf_termInfo.TryGetValue(term, out info))
            {
                Vertex vRes = new Vertex(term.Name);
                Edge eDef = new Edge(edParseBnf, vRes, info.vDef, parseTreeContext);
                graph.Add(vRes);
                graph.Add(eDef);
                return vRes;
            }
            // else: don't create vertex
            return null;
        }

        public Vertex vertexTerminal(Vertex vPrev, BnfTerm term, int line, int column, int length, string text)
        {
            Vertex vRes = new Vertex(term.Name, new ValueLiteral(text));
            graph.Add(vRes);
            if (vPrev != null)
            {
                Edge eNext = new Edge(edNextTokenEdge, vPrev, vRes, parseTreeContext);
                graph.Add(eNext);
            }

            TermInfo info = null;
            if (mpbnf_termInfo.TryGetValue(term, out info))
            {
                Edge eDef = new Edge(edParseBnf, vRes, info.vDef, parseTreeContext);
                graph.Add(eDef);
            }
            else
                throw new ArgumentError("terminal not defined");
            return vRes;
        }

        public Vertex vertexNonGrammar(Vertex vPrev, BnfTerm term, int line, int column, int length, string text)
        {
            return vertexTerminal(vPrev, term, line, column, length, text);
        }

        public void edgeForTerm(Vertex vParent, Vertex vChild, BnfTerm term, int positionNum)
        {
            TermInfo info = null;
            if (mpbnf_termInfo.TryGetValue(term, out info))
            {
                EdgeDescriptor edRole = info.edgedefsBuild[positionNum];
                if (edRole != null)
                {
                    if (vChild == null)
                        vChild = vEmptyList;
                    Edge eRole = new Edge(edRole, vParent, vChild, parseTreeContext);
                    graph.Add(eRole);
                }
            }
        }

        public static BnfTerm bnfTerm (Vertex vParse)
        {
            ValueLiteral vl = (ValueLiteral)vParse.Value;
            TokenInfo ti = (TokenInfo)vl.V;
            return ti.term;
        }
    }

}
