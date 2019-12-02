using NUnit.Framework;

namespace reslab.test
{

    [TestFixture]
    [Category("Proof Examples")]
    class ResExamples
    {
        // see also Harrison pg 223

        LogRes logRes = new LogRes();
        public const string stLos = @"
\forall x \forall y \forall z (P(x,y) \land P(y,z) \rightarrow P(x,z)),  
\forall x \forall y \forall z (Q(x,y) \land Q(y,z) \rightarrow Q(x,z)),  
\forall x \forall y (Q(x,y) \rightarrow Q(y,x)),  
\forall x \forall y (P(x,y) \lor Q(x,y))
\Rightarrow
(\forall x \forall y P(x,y)) \lor (\forall x \forall y Q(x,y))
"; // Harrison pg 198

        string stDavisPutnam = @"
\Rightarrow
\exists x \exists y \forall z 
   ( (F(x,y) \rightarrow (F(y,z) \land F(z,z))) \land
     ((F(x,y) \land G(x,y))   \rightarrow (G(x,z) \land G(z,z))) )
";   // Harrison pg 220

        string stHorn = @"
\forall x (P(x) \land (G(x) \lor H(x)) \rightarrow Q(x)),
\forall x (Q(x) \land H(x) \rightarrow J(x)),
\forall x (R(x) \rightarrow H(x))
\Rightarrow
\forall x (P(x) \land R(x) \rightarrow J(x))
";  // Harrison pg 208

        string stMorley = @"
\forall x (f(x) \rightarrow g(x)),
\exists x f(x),
\forall x \forall y (g(x) \land g(y) \rightarrow x=y)
\Rightarrow
\forall y (g(y) \rightarrow f(y))
"; // Harrison pg 241

        public static string st1side = @"
\forall x \forall y \forall z eq(p(x,p(y,z)), p(p(x,y),z)),
\forall x eq(p(1,x),x),
\forall x eq(p(i(x),x),1),
\forall x eq(x,x),
\forall x \forall y (eq(x,y) \rightarrow eq(y,x)),
\forall x \forall y \forall z (eq(x,y) \land eq(y,z) \rightarrow eq(x,z)),
\forall x \forall y (eq(x,y) \rightarrow eq(i(x),i(y))),
\forall x1 \forall y1 \forall x2 \forall y2
    (eq(x1,y1) \land eq(x2,y2) \rightarrow eq(p(x1,x2),p(x2,y2)))
\Rightarrow
\forall x eq(p(x,i(x)),1)
";
        string stDijkstra1996 = @"
\forall x eq(f(f(x)),f(x)),
\forall x \exists y eq(f(y),x),
\forall x eq(x,x),
\forall x \forall y (eq(x,y) \rightarrow eq(y,x)),
\forall x \forall y \forall z (eq(x,y) \land eq(y,z) \rightarrow eq(x,z)),
\forall x \forall y (eq(x,y) \rightarrow eq(f(x),f(y)))
\Rightarrow
\forall x eq(f(x),x)
";


        [Test]
        public void ProveRussel()  // page 180 in Harrison
        {
            SkoTest.ProveParsed(@"
\Rightarrow
\neg (\exists b \forall x ( S(b,x) \leftrightarrow \neg S(x,x) ) )
");
        }


        // [Ignore("broken")]
        [Test] // ,Ignore("hangs")]
        public void ProveLos()
        {
            LogRes logRes = new LogRes();
            Res res = new Res(irr: logRes);
            res.fUseNegatedGoals = false;
            logRes.fdp = new Fdp();
            SkoTest.ProveParsed(stLos, irr: logRes, res: res);
        }

        [Test]
        public void ProveDavisPutnam()
        {
            LogRes logRes = new LogRes();
            SkoTest.ProveParsed(stDavisPutnam, irr: logRes);
        }

        [Test]
        public void ProveHorn()
        {
            LogRes logRes = new LogRes();
            SkoTest.ProveParsed(stHorn, irr: logRes);
        }

        [Test]
        public void ProveMorley()
        {
            LogRes logRes = new LogRes();
            SkoTest.ProveParsed(stMorley, irr: logRes);
        }

        [Test]
        public void ProveDijkstra1996()
        {
            LogRes logRes = new LogRes();
            SkoTest.ProveParsed(stDijkstra1996, irr: logRes);
        }


        // [Ignore("broken")]
        [Test]
        public void ProveEq1()
        {
            LogRes logRes = new LogRes();
            foreach (string stEx in EqExamples.rgstCases1)
            {
                Lsx lsxA = SkoTest.lsxParse(stEx);
                Res res = new Res(imd: null, irr: logRes); //, imd:Pss.imdSetOfSupport);
                // res.fDoParamodulation = false;
                SkoTest.ascProveClausal(lsxA, res);
                // SkoTest.ProveParsed(stEx, irr: logRes); //, imd:Pss.imdSetOfSupport);
            }
        }

       // [Ignore("broken")]
        [Test]
        public void Prove1side()
        {
            LogRes logRes = new LogRes();
            SkoTest.ProveParsed(ResExamples.st1side, irr: logRes);
        }



        [Test]
        public void ProveEq2()
        {
            LogRes logRes = new LogRes();
            foreach (string stEx in EqExamples.rgstCases2)
            {
                Lsx lsxA = SkoTest.lsxParse(stEx);
                Res res = new Res(imd: null, irr: logRes); //, imd:Pss.imdSetOfSupport);
                res.fDoParamodulation = false;
                SkoTest.ascProveClausal(lsxA, res);
                // SkoTest.ProveParsed(stEx, irr: logRes); //, imd:Pss.imdSetOfSupport);
            }
        }
    }

}
