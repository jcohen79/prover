using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphMatching;
using GrammarDLL;
using reslab;

namespace ConsoleApplication1.test
{
    [TestFixture]
    [Category("Clause")]
    class TestCodeGen
    {
        Graph graph = new Graph(ExpressionEvaluator.cGlobalRestrictor);
        SymbolTable stGlobal = new SymbolTable(null);
        EmitConfig ec = new EmitConfig();
        REPL repl;

        [Test]
        public void SimpleCodeGen()
        {
            Restrictor cRestrictor = new Restrictor("cRestrictor", null);
            repl = new REPL();
            repl.Prepare();
            TokenInfo tokenLabel = new TokenInfo("label", SpacingType.spcKeyword, fConstant: true);
            Vertex vTokenLabel = new Vertex("vTokenLabel", new ValueLiteral(tokenLabel));

            StringBuilder sb = new StringBuilder();
            Fwt formatter = new Fwt(sb);
            PrettyFormatter pretty = new PrettyFormatter(formatter);
            EmitConfig ec = new EmitConfig();
            SymbolTable symtbl = new SymbolTable(null);
            Graph g = new Graph(cRestrictor);
            ec.Setup(symtbl);
            Vertex vTemplate = new Vertex("simpleTemplate");
            Vertex vStep2 = new Vertex("vStep2");
            Vertex vBasis = new Vertex("simpleBasis", new ValueLiteral("simpleBasisValue"));

            Edge e1 = new Edge(ec.edEmitAtom, vTemplate, ec.vEmitLiteral, cRestrictor);
            Edge e1t = new Edge(ec.edTokenInfo, vTemplate, vTokenLabel, cRestrictor);
            vTemplate.Value = new ValueLiteral("testList");

            Edge e2 = new Edge(ec.edNextStep, vTemplate, vStep2, cRestrictor);
            Edge e2e = new Edge(ec.edEmitAtom, vStep2, ec.vEmitBasisPayload, cRestrictor);
            Edge e2t = new Edge(ec.edTokenInfo, vStep2, vTokenLabel, cRestrictor);

            Vertex vStepNewline = new Vertex("vStepNewline");
            Edge e3 = new Edge(ec.edEmitAtom, vStepNewline, ec.vEmitNewline, cRestrictor);
            Edge enl = new Edge(ec.edNextStep, vStep2, vStepNewline, cRestrictor);

            symtbl.Add(ec.vBasis, vBasis);
            Emitter em = new Emitter(ec, pretty, vTemplate, symtbl, cRestrictor);

            em.Emit();
            Assert.AreEqual(sb.ToString(), "testList simpleBasisValue");
        }

        void CheckExpr(string expr)
        {
            repl.ProcessExpr(expr);
        }

        [Test]
        public void LogicSyntax()
        {
            repl = new REPL();
            repl.Prepare();

            CheckExpr(@"P(a)");
            CheckExpr(@"P(a,b)");
            CheckExpr(@"\neg \exists a \forall b \neg c \land \neg \exists d \forall e \neg f");
            CheckExpr(@"a");
            CheckExpr(@"\neg b");
            CheckExpr(@"\forall x b");
            CheckExpr(@"\exists x b");
            // CheckExpr(@"\exists! x b");
            CheckExpr(@"\nexists x b");
            CheckExpr(@"\neg \exists x \forall y \neg x");
            CheckExpr(@"a \Rightarrow c");
            CheckExpr(@"a \Rightarrow b \Leftrightarrow c");
            CheckExpr(@"a \Rightarrow b");
            CheckExpr(@"a \land b \lor c \to d");
            CheckExpr(@"a \lor b \land \sigma");
            CheckExpr(@"\neg \exists x \forall y \neg x \land \neg \exists x \forall y \neg x \lor \neg \exists x \forall y \neg x");
            CheckExpr(@"\neg \exists x \forall y \neg x \lor \neg \exists x \forall y \neg x \rightarrow \neg \exists x \forall y \neg x");
            CheckExpr(@"\neg \exists x \forall y \neg x \rightarrow \neg \exists x \forall y \neg x \leftrightarrow \neg \exists x \forall y \neg x");
            CheckExpr(@"\neg \exists x \forall y \neg x \leftrightarrow \neg \exists x \forall y \neg x \Rightarrow \neg \exists x \forall y \neg x");
            CheckExpr(@"\neg \exists x \forall y \neg x \Rightarrow \neg \exists x \forall y \neg x \Leftrightarrow \neg \exists x \forall y \neg x");
            CheckExpr(@"b = c");
            CheckExpr(@"b = c; d = f");
            CheckExpr(@"rule a \{ b = c \} d \land c");
            CheckExpr(@"rule a \{ b = c \} d \land c");

            CheckExpr(@"sequent \box");
            CheckExpr(@"sequent a \box P(a b) ");
            CheckExpr(@"sequent a Q(G(x) X()) \box P(a b) , a Q(G(x) X()) \box P(a b)  ");
        }

        [Test]
        public void MakeLsx1()
        {
            repl = new REPL();
            repl.Prepare();

            LParse lparse = new LParse();
            GenLsx genLsx = new GenLsx(lparse);
            genLsx.Push();
            genLsx.Add("a", ParseGraphBuilder.tokenIdentifier);
            genLsx.Push();
            genLsx.Add("b", ParseGraphBuilder.tokenIdentifier);
            genLsx.Add("c", ParseGraphBuilder.tokenIdentifier);
            genLsx.Pop();
            genLsx.Pop();
            Lsx lsxRoot = genLsx.lsxGetRoot();
            Assert.IsTrue(lparse.lsxParse("(a (b c))").fEqual(lsxRoot));
        }

        [Test]
        public void MakeLsx2()
        {
            repl = new REPL();
            repl.Prepare();

            LParse lparse = new LParse();
            GenLsx genLsx = new GenLsx(lparse);
            genLsx.Push();
            genLsx.Add("a", ParseGraphBuilder.tokenIdentifier);
            genLsx.Push();
            genLsx.Add("b", ParseGraphBuilder.tokenIdentifier);
            genLsx.Push();
            genLsx.Pop();
            genLsx.Add("c", ParseGraphBuilder.tokenIdentifier);
            genLsx.Push();
            genLsx.Add("d", ParseGraphBuilder.tokenIdentifier);
            genLsx.Pop();
            genLsx.Add("e", ParseGraphBuilder.tokenIdentifier);
            genLsx.Pop();
            genLsx.Push();
            genLsx.Push();
            genLsx.Add("f", ParseGraphBuilder.tokenIdentifier);
            genLsx.Pop();
            genLsx.Pop();
            genLsx.Pop();
            Lsx lsxRoot = genLsx.lsxGetRoot();
            Assert.IsTrue(lparse.lsxParse("(a (b nil c (d) e) ((f)))").fEqual(lsxRoot));
        }

        void CheckLsx(string stLatex)
        {
            Lsx lsxRes = repl.lsxParse(stLatex);
            Console.WriteLine(stLatex + " => " + lsxRes.ToString());
        }

        [Test]
        public void ParseLsx()
        {
            repl = new REPL();
            repl.Prepare();

            CheckLsx(@"P(x,y)");
            CheckLsx(@"\neg \exists a \forall b \neg c \land \neg \exists d \forall e \neg f");
        }

    }
}