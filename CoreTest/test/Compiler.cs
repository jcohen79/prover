using GraphMatching;
using NUnit.Framework;

using System.Collections.Generic;

using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using Newtonsoft.Json.Linq;
using System;
using GrammarDLL;

namespace ConsoleApplication1.test
{
    public class ParseNodeType : ValueLiteral
    {
        public ParseNodeType(string name)
            : base(name)
        {
        }
    }
    public class InstructionType : ValueLiteral
    {
        public InstructionType(string name)
            : base(name)
        {
        }
    }

    [TestFixture]
    public class Compiler
    {
        //          arg1/arg2 extend value edge
        // http://coolmaxhot.com/graphics/hex-color-palette.htm

        // AST
        static ColorDef cSyntaxType = new ColorDef("syntaxType", "#FF00FF");   // statement type (e.g. assignment)
        static ColorDef cLhs = new ColorDef("lhs", "#6600FF");
        static ColorDef cRhs = new ColorDef("rhs", "#0000FF");
        static ColorDef cControlPlace = new ColorDef("controlPlace", "#FF0099");
        static ColorDef cControlValue = new ColorDef("controlValue", "#CC66FF");
        static ColorDef cControlStmt = new ColorDef("controlStmt", "#0066FF");
        static ColorDef cControlInst = new ColorDef("controlInst", "#FFCCFF");
        static EdgeDescriptor edSyntaxType = new EdgeDescriptor("", cSyntaxType);
        static EdgeDescriptor edLhs = new EdgeDescriptor("", cLhs);
        static EdgeDescriptor edRhs = new EdgeDescriptor("", cRhs);
        static EdgeDescriptor edControlPlace = new EdgeDescriptor("", cControlPlace);
        static EdgeDescriptor edControlValue = new EdgeDescriptor("", cControlValue);
        static EdgeDescriptor edControlStmt = new EdgeDescriptor("", cControlStmt);
        static EdgeDescriptor edControlInst = new EdgeDescriptor("", cControlInst);

        // IL
        static ColorDef cInstruction = new ColorDef("ins", "#990099");   // instruction type
        static ColorDef cArg1 = new ColorDef("arg1", "#FF0066");   // tail is instruction, head payload is arg1 register
        static ColorDef cArg2 = new ColorDef("arg2", "#66FFFF");   // tail is instruction, head payload is arg2 register
        static ColorDef cDest = new ColorDef("dest", "#00FF66");   // tail is instruction, head payload is dest register
        static ColorDef cAddress = new ColorDef("address", "#00CC33");   // tail is instruction, head payload is memory location
        static ColorDef cDependsOn = new ColorDef("dependsOn", "#009966");   // tail is after head
        static ColorDef cPlace = new ColorDef("place", "#00FFFF");  // tail is value to store, dest is where to store
        static ColorDef cValue = new ColorDef("value", "#0099FF");  // tail is where to hold result, dest is value to get
        static ColorDef cRegister = new ColorDef("register", "#006600");  // tail is where used, head is register
        static EdgeDescriptor edInstruction = new EdgeDescriptor("", cInstruction);
        static EdgeDescriptor edArg1 = new EdgeDescriptor("", cArg1);
        static EdgeDescriptor edArg2 = new EdgeDescriptor("", cArg2);
        static EdgeDescriptor edDest = new EdgeDescriptor("", cDest);
        static EdgeDescriptor edAddress = new EdgeDescriptor("", cAddress);
        static EdgeDescriptor edDependsOn = new EdgeDescriptor("", cDependsOn);
        static EdgeDescriptor edPlace = new EdgeDescriptor("", cPlace);
        static EdgeDescriptor edValue = new EdgeDescriptor("", cValue);
        static EdgeDescriptor edRegister = new EdgeDescriptor("", cRegister);
        SymbolTable stGlobal;

        static ParseNodeType pSymbol = new ParseNodeType("symbol");
        static ParseNodeType pAssignment = new ParseNodeType("assignment");
        static ParseNodeType pAddition = new ParseNodeType("addition");
        static Vertex vSymbol = new Vertex("symbol", pSymbol);
        static Vertex vAssignment = new Vertex("assignment", pAssignment);
        static Vertex vAddition = new Vertex("addition", pAddition); 
        static InstructionType pAdd = new InstructionType("add");
        static InstructionType pLoad = new InstructionType("load");
        static InstructionType pStore = new InstructionType("store");

        static SymbolTable stCorrespondence;

        class NodePropertySetter : INodePropertySetter
        {
            public bool SetNodeProperties(Vertex v, DirectionType direction, EdgeDescriptor edgeDesc, JObject node)
            {
                try
                {
                    bool isHeadColor = false;
                    if (edgeDesc == edRegister)
                        isHeadColor = true;
                    if (edgeDesc == edInstruction)
                        isHeadColor = true;
                    if (edgeDesc == edControlInst)
                        isHeadColor = true;
                    if (edgeDesc == edControlStmt)
                        isHeadColor = true;
                    if (edgeDesc == edControlValue)
                        isHeadColor = true;
                    if (edgeDesc == edControlPlace)
                        isHeadColor = true;
                    if (edgeDesc == edRhs)
                        isHeadColor = true;
                    if (edgeDesc == edLhs)
                        isHeadColor = true;
                    if (edgeDesc == edSyntaxType)
                        isHeadColor = true;

                    string color = null;
                    if (DirectionType.Incoming == direction && isHeadColor)
                        color = edgeDesc.Color.RGB;
                    if (color == null)
                        return false;
                    node.Add("color", color);
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine("EdgeDescriptor.SetNodeProperties caught " + e.Message + " on " + ToString() + " " + v.ToString() + " " + direction);
                    // Console.Out.WriteLine(e.StackTrace.ToString());
                }
                return true;
            }
        }

        void Add(SymbolTable st, Named obj)
        {
            st.Add(obj.Name, obj);
        }

        void Add(SymbolTable st, string name, Base obj)
        {
            st.Add(name, obj);
        }

        [SetUp]
        public void Init()
        {
            WebServer.AddNodePropertySetter(WebServer.propertySetterName, new NodePropertySetter());

            stGlobal = new SymbolTable(null);
            stGlobal.Add("edSyntaxType", edSyntaxType);
            stGlobal.Add("edLhs", edLhs);
            stGlobal.Add("edRhs", edRhs);
            stGlobal.Add("edInstruction", edInstruction);
            stGlobal.Add("edArg1", edArg1);
            stGlobal.Add("edArg2", edArg2);
            stGlobal.Add("edDest", edDest);
            stGlobal.Add("edAddress", edAddress);
            stGlobal.Add("edDependsOn", edDependsOn);
            stGlobal.Add("edRegister", edRegister);
            stGlobal.Add("edControlStmt", edControlStmt);
            stGlobal.Add("edControlPlace", edControlPlace);
            stGlobal.Add("edControlValue", edControlValue);
            stGlobal.Add("edControlInst", edControlInst);

            //syntax
            Add(stGlobal, "symbol", pSymbol);
            Add(stGlobal, "assignment", pAssignment);
            Add(stGlobal, "addition", pAddition);

            // il
            Add (stGlobal, "load", pLoad);
            Add (stGlobal, "store", pStore);
            Add (stGlobal, "add", pAdd);

            stCorrespondence = new SymbolTable(stGlobal);

  /*
             * replace ParsedInput with subclass of GraphBuilder.AddToGraph
             * 
{Graph
   {Vertex parsedBase={ValueLiteral =} in: out: global.assignment^syntaxType global.lhs^lhs global.rhs^rhs} 
   {Vertex lhs={ValueLiteral x} in: global.parsedBase^lhs out:} 
   {Vertex rhs={ValueLiteral Add} in: global.parsedBase^rhs out: global.addition^syntaxType global.lhs^lhs global.rhs^rhs} 
   {Vertex lhs={ValueLiteral a} in: global.rhs^lhs out: global.symbol^syntaxType} 
   {Vertex rhs={ValueLiteral b} in: global.rhs^rhs out: global.symbol^syntaxType} 
   {Edge 0 parsedBase->assignment, syntaxType} 
   {Edge 1 parsedBase->lhs, lhs} 
   {Edge 2 parsedBase->rhs, rhs} 
   {Edge 3 rhs->addition, syntaxType} 
   {Edge 4 rhs->lhs, lhs} 
   {Edge 5 rhs->rhs, rhs} 
   {Edge 6 lhs->symbol, syntaxType} 
   {Edge 7 rhs->symbol, syntaxType}}
   * 
   * each rule match edge to syntax on input, il instruction on output
   * constructs dependsOn edge
   * create step edge in middle, add edge to register to use
   * 
   * The symbol after the @ on a pattern vertex identifies a symbol whose value is a ValueLiteral.
   * That allows the vertex to be specific to the pattern graph, and multiple vertices refer to same literal.
   * A pattern vertex with payload will only match a data vertex with the same payload. 
   * Data vertex with syntax payload are constructed by the parser.
   * 
   */ 
            BuildCorrespondence(@"

correspondence Assignment
   { stmt@, lhs@, rhs@, ctlStmt@, synAssignment@assignment,
          stmt~synAssignment^edSyntaxType, stmt~lhs^edLhs, stmt~rhs^edRhs, stmt~ctlStmt^edControlStmt }
   to {    ctlPlace@, ctlValue@, rValue@,
           ctlValue~rValue^edRegister,
           ctlPlace~rValue^edRegister, 
           ctlPlace~ctlValue^edDependsOn }
   using { ctlStmt~ctlPlace^edLhs,
           ctlStmt~ctlValue^edRhs, 
           lhs~ctlPlace^edControlPlace,
           rhs~ctlValue^edControlValue };
             
correspondence BinaryOp
   { expr@, lhs@, rhs@, ctlStmt@, synAddition@addition,
          expr~synAddition^edSyntaxType, expr~lhs^edLhs, expr~rhs^edRhs, expr~ctlStmt^edControlValue }
   to { inst@, rDest@, rArg1@, rArg2@, 
        ctl1@, ctl2@, instAdd@add,
        inst~instAdd^edInstruction,
        inst~rDest^edDest,
        inst~rArg1^edArg1,
        inst~rArg2^edArg2, 
        ctl1~rArg1^edRegister, 
        ctl2~rArg2^edRegister  }
   using { ctlStmt~inst^edControlInst,
           lhs~ctl1^edControlValue,
           rhs~ctl2^edControlValue };

correspondence ValueSymbol
   { expr@, ctlValue@, synSymbol@symbol,
     expr~synSymbol^edSyntaxType, expr~ctlValue^edControlValue }
   to { inst@, rArg@, address@, ctlValue@, instLoad@load,
        inst~ctlValue^edControlValue,
        inst~instLoad^edInstruction,
        inst~rArg^edArg1,
        inst~address^edAddress }
   using { ctlValue~inst^edControlInst };

correspondence PlaceSymbol
   { expr@, ctlPlace@, synSymbol@symbol,
          expr~synSymbol^edSyntaxType, expr~ctlPlace^edControlPlace }
   to { inst@, address@, ctlPlace@, rArg@, instStore@store,
        ctlPlace = ctlPlace,
        rArg = ctlPlace/edRegister,
        inst~ctlPlace^edControlPlace,
        inst~instStore^edInstruction,
        inst~rArg^edArg1,
        inst~address^edAddress }
   using { ctlPlace~inst^edControlInst }

");
        }

        public SymbolTable BuildCorrespondence(string pattern)
        {
            REPL repl = new REPL();
            repl.Prepare();
            repl.ee.EvaluateCorrespondenceSource(pattern, stCorrespondence);
            return stCorrespondence;
        }

        // Parser to convert program input to be transformed to IL
        public class CompilationGraphBuilder : GraphBuilderBase
        {

            public CompilationGraphBuilder(ExpressionEvaluatorGrammar grammar, Restrictor parseContext, object content, SymbolTable stMain, SymbolTable stTail, SymbolTable stHead, Restrictor context)
              : base(grammar, parseContext, stMain, stTail, stHead, null, context)
          {
          }

            public void ParsedInput(Vertex content, Vertex vBase)
            {
                Edge eParseBnf = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseBnf);
                BnfTerm contentTerm = (BnfTerm)((ValueLiteral)eParseBnf.vHead.Value).V;

                if (contentTerm == Grammar.identifier)
                {
                    ValueLiteral tokenValue = (ValueLiteral)content.Value;
                    string name = (string)tokenValue.V;

                    Add(new Edge(edSyntaxType, vBase, vSymbol, context));
                    vBase.Value = new ValueLiteral(name);
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
                        throw new GraphMatching.ArgumentError("invalid type of assignment lhs " + vLhs);
                    if (opTerm != Grammar.colonEquals.term)
                        throw new GraphMatching.ArgumentError("invalid type of assignment " + vOp);

                    Add(new Edge(edSyntaxType, vBase, vAssignment, context));
                    vBase.Value = vOp.Value;
                    var vGLhs = new Vertex("assignLhs");
                    var vGRhs = new Vertex("assignRhs");
                    Add(vGLhs);
                    Add(vGRhs);

                    ValueLiteral lhsValue = (ValueLiteral)vLhs.Value;
                    string sVName = (string)lhsValue.V;
                    vGLhs.Value = new ValueLiteral(sVName);

                    var eLhs = new Edge(edLhs, vBase, vGLhs, context);
                    var eRhs = new Edge(edRhs, vBase, vGRhs, context);
                    Add(eLhs);
                    Add(eRhs);
                    ParsedInput(vRhs, vGRhs);
                }
                else if (contentTerm == Grammar.BinExpr)
                {
                    Vertex vLhs = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseLhs).vHead;
                    Vertex vOp = content.eaOutOnly(parseContext, ParseGraphBuilder.edParseOp).vHead;
                    Vertex vRhs = content.eaOutOpt(parseContext, ParseGraphBuilder.edParseRhs).vHead;

                    Add(new Edge(edSyntaxType, vBase, vAddition, context));
                    ValueLiteral opValue = (ValueLiteral)vOp.Value;
                    vBase.Value = new ValueLiteral(opValue);
                    var vGLhs = new Vertex("binaryLhs");
                    var vGRhs = new Vertex("binaryRhs");
                    Add(vGLhs);
                    Add(vGRhs);

                    var eGLhs = new Edge(edLhs, vBase, vLhs, context);
                    var eGRhs = new Edge(edRhs, vBase, vRhs, context);
                    Add(eGLhs);
                    Add(eGRhs);
                    ParsedInput(vLhs, vGLhs);
                    ParsedInput(vRhs, vGRhs);
                }
                else
                    throw new GraphMatching.ArgumentError("unexpected class in graph " + content);
            }
        }

        //[Test]
        public void AssignToIL()
         {
             string s1 = @"x = a + b";
             REPL repl = new REPL();
             repl.Prepare();
             var content = repl.ee.Parse(s1);
             var stTest = new SymbolTable(stGlobal);
             var stFirst = new SymbolTable(stTest);
             var stSecond = new SymbolTable(stTest);
             var stSubst = new SymbolTable(stTest);
             var inputContext = ExpressionEvaluator.cGlobalRestrictor;
             var outputContext = new Restrictor("output", inputContext);
             var gbInput = new CompilationGraphBuilder(repl.ee.Grammar, repl.ee.parseContext, stTest, stFirst, stSecond, null, inputContext);
             var vBase = new Vertex("parsedBase");
             var vInputControl = new Vertex("inputControl");
             var eControl = new Edge(edControlStmt, vBase, vInputControl, inputContext);
             gbInput.Add(vBase);
             gbInput.Add(eControl); 
             gbInput.ParsedInput(content, vBase);
             Graph gInput = gbInput.MakeGraph();
            TraceNode trace = TraceNode.full;
            // string webPath = @"..\..\..\ConsoleApplication1\web";
            // Trace.WriteAngular(webPath);
            trace.Log("startOnGraph").A("gInput",gInput);   // show link entry for graph at top of log

            var direction = Correspondence.ReplacementDirectionType.Forward;

            // var gOutput = new Graph(outputContext);
            var gOutput = gInput;   // need to use same graph, so iterators will see new edges as they are added.

            var matchHandler = new CorrespondenceMatchHandler(stSubst, direction, inputContext, outputContext, gOutput);

            CorrespondenceSetMatcher cm = new CorrespondenceSetMatcher(gInput, stCorrespondence, direction, matchHandler, trace);

             /* check a single graph match
              List<GraphMatch> matches;
              Correspondence c = (Correspondence)stCorrespondence.Lookup("Assignment");
              Graph pattern = c.gFirst;
              // var single = PatternFinderSingle.make(pattern, vBase);
              var single = new PatternFinder(pattern, gInput, trace);
              matches = single.Match();
             */

             /* check a single correspondence match
             List<GraphMatch> matches;
              Correspondence c = (Correspondence)stCorrespondence.Lookup("Assignment");
              var stSingle = new SymbolTable(stGlobal);
              stSingle.Add("singleC", c);
              // Graph gSingle = new Graph(PatternFinderSingle.cSingleRestrictor);
              // gSingle.Add(vBase);
              CorrespondenceSetMatcher cms = new CorrespondenceSetMatcher(gInput, stSingle, Correspondence.ReplacementDirectionType.Forward, trace);
              matches = cms.Match();
             */

             cm.Match();

             trace.Log("result").A("cm", cm).A("gOutput", gOutput);   // show link entry for graph at top of log
         }


    }
}