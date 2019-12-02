using ConsoleApplication1;
using GrammarDLL;
using reslab;
using System;
using System.Text;

namespace GraphMatching
{
    public class REPL
    {
        public ExpressionEvaluator ee;
        Graph graph = new Graph(ExpressionEvaluator.cGlobalRestrictor);
        ParseGraphBuilder gb;
        SymbolTable stGlobal = new SymbolTable(null);
        EmitConfig ec = new EmitConfig();
        Vertex vCodeGenParseTreeTemplate;
        Vertex vLsxParseTreeTemplate;

        public static Vertex vRootParseGen(EmitConfig ec, Graph graph, Vertex vTemplateSeq)
        {
            Vertex vRootGen = new Vertex("rootParseTreeGen");
            graph.Add(vRootGen);
            Edge e2 = new Edge(ec.edEmitAtom, vRootGen, ec.vEmitApply, graph.context);
            graph.Add(e2);
            Edge e3 = new Edge(ec.edEmitFunctionSeq, vRootGen, vTemplateSeq, graph.context);
            graph.Add(e3);
            Vertex vBasisArg = new Vertex("rootBasis", new LookupExpression(ec.vBasis));
            Edge e4 = new Edge(ec.edArgument, vRootGen, vBasisArg, graph.context);
            graph.Add(e4);
            Edge e5 = new Edge(ec.edParameterVal, vBasisArg, ec.vBasis, graph.context);
            graph.Add(e5);

            return vRootGen;
        }

        public static Vertex vMakeCodeGenParseTreeTemplate(EmitConfig ec, Graph graph, ParseGraphBuilder gb)
        {
            Vertex vRootCodeGen = vRootParseGen(ec, graph, gb.vCodeGenTemplateSeq);

            Vertex vStepNewline = new Vertex("vStepNewline");
            Edge e6 = new Edge(ec.edEmitAtom, vStepNewline, ec.vEmitNewline, graph.context);
            graph.Add(e6);
            Edge enl = new Edge(ec.edNextStep, vRootCodeGen, vStepNewline, graph.context);
            graph.Add(enl);

            return vRootCodeGen;
        }

        public void Prepare()
        {
            ec.Setup(stGlobal);  // before ParseGraphBuilder
            ee = new ExpressionEvaluator(new ExpressionEvaluatorGrammar(), graph.context);
            gb = new ParseGraphBuilder(graph, ExpressionEvaluator.cGlobalRestrictor, ee, ec);
            ee.converter = new GraphConverter<Vertex, Edge>(gb);
            ee.Grammar.fBuildAst = false;
            vCodeGenParseTreeTemplate = vMakeCodeGenParseTreeTemplate(ec, graph, gb);  // after gb
            vLsxParseTreeTemplate = vRootParseGen(ec, graph, gb.vLsxTemplateSeq);
        }

        public void ProcessExpr(string expr)
        {
            StringBuilder sb = new StringBuilder();
            Fwt formatter = new Fwt(sb);
            PrettyFormatter pretty = new PrettyFormatter(formatter);

            var root = ee.Parse(expr);

            SymbolTable symtabLocal = new SymbolTable(stGlobal);
            symtabLocal.Add(ec.vBasis, (Vertex)root);
            Emitter em = new Emitter(ec, pretty, vCodeGenParseTreeTemplate, symtabLocal, graph.context);
            em.Emit();
            Console.WriteLine(expr + "  => " + sb);
        }

        public Lsx lsxParse (string stText, LParse lparse = null)
        {
            var root = ee.Parse(stText);
            if (lparse == null)
                lparse = new LParse();
            Sko.AddSyms(lparse, ee.Grammar);
            GenLsx genLsx = new GenLsx(lparse);
            SymbolTable symtabLocal = new SymbolTable(stGlobal);
            symtabLocal.Add(ec.vBasis, (Vertex)root);
            Emitter em = new Emitter(ec, genLsx, vLsxParseTreeTemplate, symtabLocal, graph.context);
            em.Emit();
            return genLsx.lsxGetRoot();
        }

        public void Loop()
        {
            Prepare();

            string stLine;
            string stInput = null;
            while ((stLine = Console.ReadLine()) != null)
            {
                bool fContinued = false;
                if (stLine.EndsWith("\\"))
                {
                    fContinued = true;
                    stLine = stLine.Substring(0, stLine.Length - 1);
                }
                if (stInput == null)
                    stInput = stLine;
                else
                    stInput = stInput + stLine;
                if (fContinued)
                    continue;

                ProcessExpr(stInput);

                stInput = null;
            }
        }
        public static void L()
        {
            REPL repl = new REPL();
            repl.Loop();
        }
    }
}
