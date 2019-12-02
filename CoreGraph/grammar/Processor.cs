
using System.Collections.Generic;
using System.Text;
using GraphMatching;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using GrammarDLL;

namespace ConsoleApplication1
{
    public class ParseTreeProcessor
    {
        public ExpressionEvaluatorGrammar Grammar { get; private set; }
        public Restrictor parseContext;
        public ParseTreeProcessor(ExpressionEvaluatorGrammar grammar, Restrictor parseContext)
        {
            this.parseContext = parseContext;
            this.Grammar = grammar;
        }
        public ValueBase BuildExpr(SymbolTable dictionary, Vertex content)
        {
            Edge eParseBnf = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseBnf);
            BnfTerm contentTerm = ParseGraphBuilder.bnfTerm(eParseBnf.vHead);

            if (contentTerm == Grammar.identifier)
            {
                ValueLiteral tokenValue = (ValueLiteral)content.Value;
                string name = (string)tokenValue.V;

                return new SymbolReference(name);
            }
            else if (contentTerm == Grammar.stringLit)
            {
                ValueLiteral tokenValue = (ValueLiteral)content.Value;
                string text = (string)tokenValue.V;
                var v = new ValueLiteral(text);
                return v;
            }
            else
                throw new ArgumentError("unexpected class of expr");

        }
        public object EvaluateExpr(SymbolTable dictionary, Vertex content)
        {
            Edge eParseBnf = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseBnf);
            BnfTerm contentTerm = ParseGraphBuilder.bnfTerm(eParseBnf.vHead);

            if (contentTerm == Grammar.stringLit)
            {
                ValueLiteral tokenValue = (ValueLiteral)content.Value;
                string text = (string)tokenValue.V;
                return text;
            }
            else if (contentTerm == Grammar.identifier)
            {
                ValueLiteral tokenValue = (ValueLiteral)content.Value;
                string name = (string)tokenValue.V;

                var val = dictionary.Lookup(name);
                if (val == null)
                    throw new ArgumentError("could not find value for symbol named " + name);
                return val;
            }
            else
                throw new ArgumentError("unexpected class");

        }

        protected Edge MakeEdge(Vertex content, SymbolTable stTail, SymbolTable stHead, SymbolTable stDesc, Restrictor context)
        {
            Vertex vLhs = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseLhs).vHead;
            Vertex vRhs = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseRhs).vHead;
            Edge eArg = content.eaOutOpt(parseContext, ParseGraphBuilder.edParseArg);

            Vertex vLhsParse = vLhs.eaOutOnly(parseContext, ParseGraphBuilder.edParseBnf).vHead;
            Vertex vRhsParse = vRhs.eaOutOnly(parseContext, ParseGraphBuilder.edParseBnf).vHead;

            BnfTerm lhsTerm = ParseGraphBuilder.bnfTerm(vLhsParse);
            BnfTerm rhsTerm = ParseGraphBuilder.bnfTerm(vRhsParse);

            if (lhsTerm != Grammar.identifier)
                throw new ArgumentError("invalid type of edge lhs " + vLhs);
            if (rhsTerm != Grammar.identifier)
                throw new ArgumentError("invalid type of edge lhs " + vLhs);

            ValueLiteral lhsValue = (ValueLiteral)vLhs.Value;
            string stLhs = (string)lhsValue.V;
            ValueLiteral rhsValue = (ValueLiteral)vRhs.Value;
            string stRhs = (string)rhsValue.V;

            object vTail = stTail.Lookup(stLhs);
            if (!(vTail is Vertex))
                throw new GraphMatching.ArgumentError("could not find tail vertex " + stLhs);
            object vHead = stHead.Lookup(stRhs);
            if (!(vHead is Vertex))
                throw new GraphMatching.ArgumentError("could not find head vertex " + stRhs);

            object desc = null;
            if (eArg != null)
            {
                desc = EvaluateExpr(stDesc, eArg.vHead);
                if (!(desc is EdgeDescriptor))
                    throw new ArgumentError("could not find edge descriptor for " + stTail + "~" + stHead + "^" + desc);
            }
            return new Edge((EdgeDescriptor)desc, (Vertex)vTail, (Vertex)vHead, context);
        }
    }

    public class ExpressionEvaluator : ParseTreeProcessor
    {
        public Parser Parser { get; private set; }
        public LanguageData Language { get; private set; }

        public GraphConverter<Vertex, Edge> converter;

        //Default constructor, creates default evaluator 
        public ExpressionEvaluator(ExpressionEvaluatorGrammar grammar, Restrictor parseContext)
            : base (grammar, parseContext)
        {
            Language = new LanguageData(Grammar);
            if (Language.ErrorLevel >= GrammarErrorLevel.Conflict)
            {
                string msg = ParserDataPrinter.PrintStateList(Language);
                System.Console.WriteLine(msg);
                throw new ArgumentError("language error");
            }
            Parser = new Parser(Language);
        }

        public Vertex Parse(string script)
        {
            var parsedScript = Parser.Parse(script);
            if (parsedScript.HasErrors())
            {
                var sb = new StringBuilder();
                foreach (var s in parsedScript.ParserMessages)
                    sb.Append(s);
                string location = Parser.Context.Source.ToString();
                throw new ArgumentError(sb.ToString());
            }
            return converter.BuildSubtree(parsedScript.Root);
        }

        public static readonly Restrictor cGlobalRestrictor = new Restrictor("global", null);
        static readonly Restrictor cUsing = new Restrictor("u", cGlobalRestrictor);

        public void EvaluateCorrespondenceSource(string script, SymbolTable stCorrespondence)
        {
            EvaluateCorrespondenceParsed(Parse(script), stCorrespondence);
        }

        AstNode OneElement(AstNode list)
        {
            if (list.ChildNodes.Count == 0)
                return null;
            if (list.ChildNodes.Count != 1)
                throw new ArgumentError("extra object: " + list);
            return list.ChildNodes[0];
        }

        public void EvaluateCorrespondenceParsed(Vertex content, SymbolTable stCorrespondence)
        {
            Edge eParseBnf = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseBnf);
            BnfTerm contentTerm = (BnfTerm)((ValueLiteral)eParseBnf.vHead.Value).V;

            if (contentTerm == Grammar.Correspondence)
            {
                Edge eName = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseName);
                ValueLiteral tokenValue = (ValueLiteral)eName.vHead.Value;
                string name = (string)tokenValue.V;

                var corr = new Correspondence(name, stCorrespondence);

                // first pass: get symbols from each side
                Restrictor baseContext = cGlobalRestrictor;
                Restrictor firstContext = new Restrictor("first" + name, baseContext);
                Restrictor secondContext = new Restrictor("second" + name, baseContext);
                Restrictor usingContext = new Restrictor("using" + name, cUsing);
                var gbFirst = new GraphBuilder(Grammar, parseContext, content, ParseGraphBuilder.edParseLhs, corr.stFirst, corr.stSecond, firstContext);
                var gbSecond = new GraphBuilder(Grammar, parseContext, content, ParseGraphBuilder.edParseRhs, corr.stSecond, corr.stFirst, secondContext);
                var gbUsing = new GraphBuilder(Grammar, parseContext, corr.stUsing, corr.stFirst, corr.stSecond, usingContext);
                gbUsing.AddToGraph(content.eaOutOnly(parseContext, ParseGraphBuilder.edParseArg).vHead);

                // second pass: lookup symbol references
                gbFirst.ResolveSymbols();
                gbSecond.ResolveSymbols();
                gbUsing.ResolveSymbols();

                corr.gFirst = gbFirst.MakeGraph();
                corr.gSecond = gbSecond.MakeGraph();
                corr.gUsing = gbUsing.MakeGraph();

                stCorrespondence.Add(name, corr);

            }
            else if (contentTerm == Grammar.StmtAndProgram)
            {
                Edge eCar = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseCar);
                Edge eCdr = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseCdr);
                EvaluateCorrespondenceParsed(eCar.vHead, stCorrespondence);
                EvaluateCorrespondenceParsed(eCdr.vHead, stCorrespondence);
            }
            else
                throw new ArgumentError("unexpected class in graph match: " + content);
        }

        public Graph EvaluateGraph(string script, SymbolTable dictionary, bool fDuplicateAllowed = false)
        {
            Vertex vParse = Parse("{" + script + "}");
            var gb = new GraphBuilder(Grammar, parseContext, dictionary, dictionary, dictionary, cGlobalRestrictor);
            gb.fDuplicateAllowed = fDuplicateAllowed;
            gb.AddToGraph(vParse);
            return gb.MakeGraph();
        }

        public List<GraphMatch> EvaluateGraphMatchList(string script, SymbolTable dictionary)
        {
            var gmlb = new GraphMatchListBuilder(Grammar, parseContext, cGlobalRestrictor);
            gmlb.Dictionary = dictionary;
            Vertex vParse = Parse("{" + script + "}");
            gmlb.AddToGraphMatchList(vParse);
            return gmlb.MakeGraphMatchList();
        }

    }//class ExpressionEvaluator

    public abstract class RequiredType : Named
    {
        private class RequiredType_Vertex : RequiredType
        {
            public RequiredType_Vertex(string s, System.Type type)
                : base(s, type)
            { }

            public override Expression MakeExpression(object value)
            {
                var expr = new VertexExpression();
                expr.vRef = (Vertex)value;
                return expr;
            }
        };
        private class RequiredType_Edge : RequiredType
        {
            public RequiredType_Edge(string s, System.Type type)
                : base(s, type)
            { }

            public override Expression MakeExpression(object value)
            {
                var expr = new EdgeExpression();
                expr.eRef = (Edge)value;
                return expr;
            }
        };
        private class RequiredType_EdgeDescriptor : RequiredType
        {
            public RequiredType_EdgeDescriptor(string s, System.Type type)
                : base(s, type)
            { }

            public override Expression MakeExpression(object value)
            {
                var expr = new EdgeDescriptorExpression();
                expr.edRef = (EdgeDescriptor)value;
                return expr;
            }
        };

        public static readonly RequiredType Vertex = new RequiredType_Vertex("vertex", typeof(Vertex));
        public static readonly RequiredType Edge = new RequiredType_Edge("edge", typeof(Edge));
        public static readonly RequiredType EdgeDescriptor = new RequiredType_EdgeDescriptor("edgeDescriptor", typeof(EdgeDescriptor));



        public System.Type Type { get; private set; }

        private RequiredType(string name, System.Type type)
            : base(name)
        {
            this.Type = type;
        }

        public abstract Expression MakeExpression(object value);
    }

    /// <summary>
    ///  Hold onto objects where symbols from the other graph are needed to complete
    ///  the processing of the AST.
    /// </summary>
    public class AssignmentWithExpr
    {
        private Assignment Assignment;
        private Vertex vExpression;
        private GraphBuilder gb;

        public AssignmentWithExpr(GraphBuilder gb, Assignment Assignment, Vertex vExpression)
        {
            this.gb = gb;
            this.Assignment = Assignment;
            this.vExpression = vExpression;
        }

        public void ResolveSymbols()
        {
            Assignment.exExpr = gb.ExpressionFromAST(vExpression, gb.stOther, RequiredType.Vertex);
        }
    }

    /// <summary>
    /// Traverse an AST that describes a graph and create an instance of Graph
    /// </summary>
    public abstract class GraphBuilderBase : ParseTreeProcessor
    {
        protected Graph graph;
        public VertexLink vlFirst { get; private set; }
        public VertexLink vlLast { get; private set; }
        public EdgeLink elFirst { get; private set; }
        public EdgeLink elLast { get; private set; }
        protected List<Assignment> lAssignments = new List<Assignment>();
        protected SymbolTable stMain;
        protected SymbolTable stTail;
        protected SymbolTable stHead;
        public SymbolTable stOther { get; protected set; }
        protected Restrictor context;
        public bool fDuplicateAllowed { get; set; }

        public GraphBuilderBase(ExpressionEvaluatorGrammar grammar, Restrictor parseContext, SymbolTable stMain, SymbolTable stTail, SymbolTable stHead, SymbolTable stOther, Restrictor context)
            : base(grammar, parseContext)
        {
            this.stMain = stMain;
            this.stTail = stTail;
            this.stHead = stHead;
            this.stOther = stOther;
            this.context = context;
        }

        public void Add(Vertex v)
        {
            var link = new VertexLink(v, null);
            if (vlFirst == null)
                vlFirst = link;
            else
                vlLast.next = link;
            vlLast = link;
        }

        public void Add(Edge e)
        {
            var link = new EdgeLink(e, null);
            if (elFirst == null)
                elFirst = link;
            else
                elLast.next = link;
            elLast = link;
        }

        public Graph MakeGraph()
        {
            graph = new Graph(vlFirst, vlLast, elFirst, elLast, context);
            graph.vaAssignments = lAssignments;
            return graph;
        }
    }

    /// <summary>
    /// Traverse an AST that describes a correspondence and create an instance of Graph
    /// </summary>
    public class GraphBuilder : GraphBuilderBase
    {
        protected List<AssignmentWithExpr> Pending = new List<AssignmentWithExpr>();

        public GraphBuilder(ExpressionEvaluatorGrammar grammar, Restrictor parseContext, Vertex content, EdgeDescriptor edStart, SymbolTable stMain, SymbolTable stOther, Restrictor context)
            : base(grammar, parseContext, stMain, stMain, stMain, stOther, context)
        {
            this.stMain = stMain;
            this.stTail = stMain;
            this.stHead = stMain;
            this.stOther = stOther;
            this.context = context;
            fDuplicateAllowed = false;
            AddToGraph(content.eaOutOnly(parseContext, edStart).vHead);
        }

        public GraphBuilder(ExpressionEvaluatorGrammar grammar, Restrictor parseContext, SymbolTable stMain, SymbolTable stTail, SymbolTable stHead, Restrictor context)
            : base(grammar, parseContext, stMain, stTail, stHead, null, context)
        {
        }

        public void AddToGraph(Vertex content)
        {
            if (content == null)
                return;
            Edge eParseBnf = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseBnf);
            BnfTerm contentTerm = (BnfTerm)((ValueLiteral)eParseBnf.vHead.Value).V;

            if (contentTerm == Grammar.ItemAndList)
            {
                Edge eCar = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseCar);
                Edge eCdr = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseCdr);
                AddToGraph(eCar.vHead);
                AddToGraph(eCdr.vHead);
            }
            else if (contentTerm == Grammar.OneItemList)
            {
                Edge eCar = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseCar);
                AddToGraph(eCar.vHead);
            }
            else if (contentTerm == Grammar.ListExpr)
            {
                Edge eArg = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseArg);
                AddToGraph(eArg.vHead);
            }
            else if (contentTerm == Grammar.VertexExpr1
                  || contentTerm == Grammar.VertexExpr2)
            {
                Edge eLhs = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseLhs);
                Edge eRhs = content.eaOutOpt(parseContext, ParseGraphBuilder.edParseRhs);

                ValueLiteral tokenValue = (ValueLiteral)eLhs.vHead.Value;
                string sVName = (string)tokenValue.V;

                // var node = (VertexNode)content;
                var oldV = stMain.Lookup(sVName);
                if (oldV != null)
                {
                    if (!fDuplicateAllowed)
                        throw new ArgumentError("already have an object named " + sVName);
                    Add((Vertex)oldV);
                }
                else
                {
                    var v = new Vertex(sVName);
                    if (eRhs != null)
                    {
                        var expr = BuildExpr(stMain, eRhs.vHead);
                        var ev = expr.Evaluate(stMain, null);
                        v.Value = (ValueBase)ev;
                        // System.Console.WriteLine("payload " + v.Value);
                    }
                    Add(v);
                    stMain.Add(sVName, v);
                }
            }
            else if (contentTerm == Grammar.EdgeExpr1
                  || contentTerm == Grammar.EdgeExpr2)
            {
                var e = MakeEdge(content, stTail, stHead, stMain, context);
                Add(e);
            }
            else if (contentTerm == Grammar.AssignmentStmt)
            {
                Vertex vLhs = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseLhs).vHead;
                Vertex vOp = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseOp).vHead;
                Vertex vRhs = content.eaOutOpt(parseContext, ParseGraphBuilder.edParseRhs).vHead;

                Vertex vLhsParse = vLhs.eaOutOnly(parseContext, ParseGraphBuilder.edParseBnf).vHead;
                Vertex vOpParse = vOp.eaOutOnly(parseContext, ParseGraphBuilder.edParseBnf).vHead;

                BnfTerm lhsTerm = (BnfTerm)((ValueLiteral)vLhsParse.Value).V;
                BnfTerm opTerm = (BnfTerm)((ValueLiteral)vOpParse.Value).V;

                if (lhsTerm != Grammar.identifier)
                    throw new ArgumentError("invalid type of assignment lhs " + vLhs);
                if (opTerm != Grammar.colonEquals.term)
                    throw new ArgumentError("invalid type of assignment " + vOp);

                var assign = new Assignment();
                ValueLiteral lhsValue = (ValueLiteral)vLhs.Value;
                string sVName = (string)lhsValue.V;

                object lhsVal = stMain.Lookup(sVName);
                if (!(lhsVal is Vertex))
                    throw new ArgumentError("is not a vertex named " + sVName);
                assign.vLhs = (Vertex)lhsVal;
                lAssignments.Add(assign);
                Pending.Add(new AssignmentWithExpr(this, assign, vRhs));
            }
            else
                throw new ArgumentError("unexpected class in graph " + content);
        }



          public void ResolveSymbols()
        {
            foreach (AssignmentWithExpr awe in Pending)
            {
                awe.ResolveSymbols();
            }
        }

        public Expression ExpressionFromAST(Vertex content, SymbolTable st, RequiredType type)
        {
            Edge eParseBnf = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseBnf);
            BnfTerm contentTerm = (BnfTerm)((ValueLiteral)eParseBnf.vHead.Value).V;

            if (contentTerm == Grammar.identifier)
            {
                ValueLiteral tokenValue = (ValueLiteral)content.Value;
                string stName = (string)tokenValue.V;

                var val = st.Lookup(stName);

                if (!(type.Type.IsInstanceOfType(val)))
                    throw new ArgumentError("name should refer to a " + type.Name + ": " + stName);
                return new SymbolReference(stName);   // will lookup in Correspondence.Perform to get data that corresponds to vertex
                                                      //                  return type.MakeExpression(lhsVal);
            }
            if (contentTerm == Grammar.BinExpr)
            {
                Vertex vLhs = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseLhs).vHead;
                Vertex vOp = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseOp).vHead;
                Vertex vRhs = content.eaOutOpt(parseContext, ParseGraphBuilder.edParseRhs).vHead;

                Vertex vOpParse = vOp.eaOutOnly(parseContext, ParseGraphBuilder.edParseBnf).vHead;
                BnfTerm opTerm = (BnfTerm)((ValueLiteral)vOpParse.Value).V;

                var expr = new BinaryExpression();
                if (opTerm == Grammar.divide.term)
                    expr.op = Operator.forward;
                else if (opTerm == Grammar.divideEquals.term)   // was "\\"
                    expr.op = Operator.reverse;
                else
                    throw new ArgumentError("invalid operator " + expr.op);
                expr.exLhs = ExpressionFromAST(vLhs, st, RequiredType.Vertex);
                expr.exRhs = ExpressionFromAST(vRhs, st, RequiredType.EdgeDescriptor);
                return expr;
            }
            throw new ArgumentError("unexpected class in expression " + content);
        }
    }

    /// <summary>
    /// Traverse an AST that describes a  list of GraphMatches and create the list.
    /// </summary>
    public class GraphMatchListBuilder : ParseTreeProcessor
    {
        private List<GraphMatch> graphMatches;
        public SymbolTable Dictionary { get; set; }
        private Restrictor context;

        public GraphMatchListBuilder(ExpressionEvaluatorGrammar grammar, Restrictor parseContext, Restrictor context)
            : base (grammar, parseContext)
        { 
            graphMatches = new List<GraphMatch>();
            this.context = context;
        }

        private void AddMatch(Vertex content)
        {
            GraphMatchBuilder gmb = new GraphMatchBuilder(Grammar, parseContext, context);
            gmb.Dictionary = Dictionary;
            gmb.AddToGraphMatch(content);
            graphMatches.Add(gmb.MakeGraphMatch());
        }

        public void AddToGraphMatchList(Vertex content)
        {
            Edge eParseBnf = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseBnf);
            BnfTerm contentTerm = (BnfTerm)((ValueLiteral)eParseBnf.vHead.Value).V;

            if (contentTerm == Grammar.ItemAndList)
            {
                Vertex vCar = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseCar).vHead;
                Vertex vCdr = content.eaOutOpt(parseContext, ParseGraphBuilder.edParseCdr).vHead;

                AddMatch(vCar);
                AddToGraphMatchList(vCdr);
            }
            else if (contentTerm == Grammar.OneItemList)
            {
                Vertex vCar = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseCar).vHead;
                AddMatch(vCar);
            }
            else if (contentTerm == Grammar.ListExpr)
            {
                Vertex vArg = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseArg).vHead;
                AddToGraphMatchList(vArg);
            }
            else
                throw new ArgumentError("unexpected class in graph match: " + content);
        }

        public List<GraphMatch> MakeGraphMatchList()
        {
            return graphMatches;
        }
    }

    /// <summary>
    /// Traverse and AST that describes a GraphMatch and create it
    /// </summary>
    public class GraphMatchBuilder : ParseTreeProcessor
    {
        private GraphMatch graphMatch;
        public SymbolTable Dictionary { get; set; }
        private List<VertexMatch> vml = new List<VertexMatch>();
        private List<EdgeMatch> eml = new List<EdgeMatch>();
        private Restrictor context;

        public GraphMatchBuilder(ExpressionEvaluatorGrammar grammar, Restrictor parseContext, Restrictor context)
            : base (grammar, parseContext)
        {
            this.context = context;
        }

        public void AddToGraphMatch(Vertex content)
        {
            Edge eParseBnf = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseBnf);
            BnfTerm contentTerm = (BnfTerm)((ValueLiteral)eParseBnf.vHead.Value).V;

            if (contentTerm == Grammar.ItemAndList)
            {
                Vertex vCar = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseCar).vHead;
                Vertex vCdr = content.eaOutOpt(parseContext, ParseGraphBuilder.edParseCdr).vHead;

                AddToGraphMatch(vCar);
                AddToGraphMatch(vCdr);
            }
            else if (contentTerm == Grammar.OneItemList)
            {
                Vertex vCar = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseCar).vHead;

                AddToGraphMatch(vCar);
            }
            else if (contentTerm == Grammar.ListExpr)
            {
                Vertex vArg = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseArg).vHead;

                AddToGraphMatch(vArg);
            }
            else if (contentTerm == Grammar.VertexMatchExpr)
            {
                Vertex vLhs = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseLhs).vHead;
                Vertex vRhs = content.eaOutOpt(parseContext, ParseGraphBuilder.edParseRhs).vHead;

                ValueLiteral tokenValue = (ValueLiteral)vLhs.Value;
                string vertexName = (string)tokenValue.V;

                object vPattern = Dictionary.Lookup(vertexName);
                if (!(vPattern is Vertex))
                    throw new ArgumentError("could not find vertex " + vertexName);
                List<Vertex> rgvData = new List<Vertex>();
                Vertex vCdr = vRhs;
                while (vCdr != null)
                {
                    Vertex vCar = vCdr.eaOutOnly(parseContext, ParseGraphBuilder.edParseCar).vHead;
                    ValueLiteral vlValue = (ValueLiteral)vCar.Value;
                    string dataName = (string)vlValue.V;
                    object vData = Dictionary.Lookup(dataName);
                    if (!(vData is Vertex))
                        throw new ArgumentError("name should refer to a vertex " + dataName);
                    rgvData.Add((Vertex)vData);
                    Edge eCdr = vCdr.eaOutOpt(parseContext, ParseGraphBuilder.edParseCdr);
                    if (eCdr == null)
                        break;
                    vCdr = eCdr.vHead;
                }
                var vm = new VertexMatch((Vertex)vPattern, rgvData.ToArray());
                vml.Add(vm);
            }
            else if (contentTerm == Grammar.EdgeMatchExpr)
            {
                Vertex vLhs = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseLhs).vHead;
                Vertex vRhs = content.eaOutOpt(parseContext, ParseGraphBuilder.edParseRhs).vHead;

                // var emn = (EdgeMatchNode)content;
                var ePattern = MakeEdge(vLhs, Dictionary, Dictionary, Dictionary, context);
                List<Edge> rgeData = new List<Edge>();
                Vertex vCdr = vRhs;
                while (vCdr != null)
                {
                    Vertex vCar = vCdr.eaOutOnly(parseContext, ParseGraphBuilder.edParseCar).vHead;

                    var eData = MakeEdge(vCar, Dictionary, Dictionary, Dictionary, context);
                    rgeData.Add(eData);

                    Edge eCdr = vCdr.eaOutOpt(parseContext, ParseGraphBuilder.edParseCdr);
                    if (eCdr == null)
                        break;
                    vCdr = eCdr.vHead;
                }
                var em = new EdgeMatch(ePattern, rgeData.ToArray());
                eml.Add(em);
            }
            else
                throw new ArgumentError("unexpected class in graph match: " + content);
        }

        public GraphMatch MakeGraphMatch()
        {
            graphMatch = new GraphMatch(vml.ToArray(), eml.ToArray());
            return graphMatch;
        }
    }

}
