
using NUnit.Framework;
using System.Collections.Generic;
using GraphMatching;

namespace reslab.test
{
    [TestFixture]
    [Category("Inference")]
    class CmrPtiTest : TpTestBase
    {

        public void CmrTest(string stL, string stR, string stExpected, bool fFailExpected = false)
        {
            /*
            Atp atpToEquate = atpMake(stL, stR);
            Eqs eqsStart = new Eqs(res, atpToEquate);
            eqsStart.TransferLeft(etr);
            */
            Cbd cbd = new Cbd();
            Res res = new Res();
            res.fUseNegatedGoals = false;
            res.Init();

            Pti ptiLeft = ptiMake(cbd, stL);
            Pti ptiRight = ptiMake(cbd, stR);
            Cpg cpg = new Cpg(res, null);
            cpg.TransferLeft(ptiLeft);
            cpg.TransferRight(ptiRight);
            res.prs.Add(cpg);
            res.prs.Run();

            Asc ascR = cbd.ascBuild(stExpected);
            ascR.nResolveTerm = Asc.nNoResolveTerm;
            if (!fFailExpected)
                Assert.IsTrue(res.gnpAscAsc.fFind(ascR));
            else
                Assert.IsFalse(res.gnpAscAsc.fFind(ascR));
        }

        [Test]
        public void CmrMake()
        {
            CmrTest("(() (= a (f1 x)) )",
                    "(() (= (f1 c) b))",
                    "(() (= a b))");
            CmrTest("(() (= a c))",
                    "(() (= c b))",
                    "(() (= a b))");
            CmrTest("(() (= a (f1 x x)) )",
                    "(() (= (f1 c c) b))",
                    "(() (= a b))");
            CmrTest("(() (= a (f1 x x)) )",
                    "(() (= (f1 c d) b))",
                    "(() (= a b))", fFailExpected: true);
            CmrTest("(() (= a (f1 x)) (P x) )",
                    "(() (= (f1 c) b))",
                    "(() (= a b) (P c))");
        }
        [Test]
        public void CmrMakeNew()
        {
            CmrTest("(() (= a (f1 x)) (P x) )",
                    "(() (= (f1 c) b))",
                    "(() (= a b) (P d))", fFailExpected: true);
            CmrTest("(() (= a (f1 x y)) (P y x) )",
                    "(() (= (f1 c d) b))",
                    "(() (= a b) (P d c))");
            CmrTest("(() (= a (f1 x y)) (P y x) )",
                    "(() (= (f1 c d) b))",
                    "(() (= a b) (P d e))", fFailExpected: true);
            //            CmrTest("(() (f (c z x) (a b x)))", "(() (g (d x) (c x y)))",
            //                    "(((f (c @0 @1) (a b @1))) ((g (d @2) (c @2 @3))))");
            CmrTest("(() (= (f x (f y z)) (f (f x y) z) )",
                                  "(() (= (f x (f y z)) (f (f x y) z) )",
                    "(() (= (f x (f y (f w z)) )  (f (f (f x y) w) z) ))");
        }
    }

    [TestFixture]
    [Category("Proof Examples")]
    class CgsTest
    {
        const string stCgs1 = @"
a=b, Q(a)
\Rightarrow
Q(b)";
        const string stCgs2 = @"
a=b, b=c, Q(a)
\Rightarrow
Q(c)";
        const string stCgs3 = @"
a=b, b=c, c=d, Q(a)
\Rightarrow
Q(d)";
        const string stCgs4 = @"
a=b, b=c, c=d, Q(f1(a))
\Rightarrow
Q(f1(d))";
        const string stCgs5 = @"
a=b, b=c, c=d,
e=f, f=g,
Q(f1(a,e))
\Rightarrow
Q(f1(d,g))";
        const string stCgs6 = @"
a=b, b=c, c=d,
g1(e)=f, f=g,
Q(f1(a,g1(e)))
\Rightarrow
Q(f1(d,g))";
        const string stCgs7 = @"
a=b,
\forall x Q(x)
\Rightarrow
Q(b)";
        const string stCgs8 = @"
f1(a)=g1(a), a=b,
Q(f1(a))
\Rightarrow
Q(g1(b))";
        const string stCgs9 = @"
\forall x a=f2(x,b),
\forall y f2(y,b) = g2(y,b),
Q(a)
\Rightarrow
Q(g2(d,b))";
        const string stCgs10 = @"
\forall x a=x,
Q(b)
\Rightarrow
Q(a)";
        const string stCgs11 = @"
a=b, b=c, Q(f1(a))
\Rightarrow
Q(f1(c))";

        string[] rgstCases1 = { stCgs4, stCgs2, stCgs1, stCgs3, stCgs8, stCgs9, stCgs11 };
        string[] rgstCases2 = { stCgs10, stCgs5, stCgs6, stCgs7 };

        [Test]
        public void CgsTest1()
        {
            LogRes logRes = new LogRes();
            foreach (string stEx in rgstCases1)
            {
                Lsx lsxA = SkoTest.lsxParse(stEx);
                Res res = new Res(irr: logRes); //, imd:Pss.imdSetOfSupport);
                res.fDoParamodulation = true;
                SkoTest.ascProveClausal(lsxA, res);
            }
        }

        [Test]
        public void CgsTest2()
        {
            LogRes logRes = new LogRes();
            foreach (string stEx in rgstCases2)
            {
                Lsx lsxA = SkoTest.lsxParse(stEx);
                Res res = new Res(irr: logRes); //, imd:Pss.imdSetOfSupport);
                res.fUseNegatedGoals = false;
                res.fDoParamodulation = true;
                SkoTest.ascProveClausal(lsxA, res);
            }
        }
    }
    [TestFixture]
    [Category("Inference")]
    class EubPtiTest : TpTestBase
    {
        /// <summary>
        /// Check if Atp/Pti solution works
        /// </summary>
        public void EubTest(string stAtpL, string stPti, string stAtpR, bool fFailExpected = false)
        {
            Cbd cbd = new Cbd();
            Res res = new Res();
            res.Init();

            Atp atpToEquate = atpMake(stAtpL, stAtpR);
            Pti pti = ptiMake(cbd, stPti);

            Lsm lsmL = atpToEquate.rglsmData[Asb.nLsmId - atpToEquate.rgnTree[Atp.nOffsetFirstTerm]];
            Lsm lsmR = atpToEquate.rglsmData[Asb.nLsmId - atpToEquate.rgnTree[Atp.nOffsetLeftSize + Atp.nOffsetFirstTerm]];
            Prl prl = new Prl(lsmL, lsmR);
            Epr epr = res.eprObtain(prl);
            Eqs eqs = res.eqsObtain(atpToEquate);
            epr.TransferLeft(eqs);
            epr.TransferRight(pti);
            Etr etr = new Etr(res, atpToEquate);
            eqs.TransferLeft(etr);  // check answer
            res.prs.Run();
            Assert.IsTrue(etr.rgesnFound != null);
        }

        [Test]
        public void EubTest1()
        {
            EubTest("(() (h  (h  @0)))", " (nil    (=  (h  @0) @0))", "(() (h  @1))");
        }

        Etr etrRunWithPti (string stAtpL, string stPti, string stAtpR, uint nLitNum, bool fFailExpected)
        {
            Cbd cbd = new Cbd(lparse);
            Res res = new Res();
            res.fUseNegatedGoals = false;
            res.Init();

            Atp atpToEquate = atpMake(stAtpL, stAtpR);
            Pti pti = ptiMake(cbd, stPti, nLitNum);    // this makes the pti in one direction only

            Lsm lsmL = atpToEquate.rglsmData[Asb.nLsmId - atpToEquate.rgnTree[Atp.nOffsetFirstTerm]];
            Lsm lsmR = atpToEquate.rglsmData[Asb.nLsmId - atpToEquate.rgnTree[Atp.nOffsetLeftSize + Atp.nOffsetFirstTerm]];
            Prl prl = new Prl(lsmL, lsmR);
            Epr eub = res.eprObtain(prl);
            Eqs eqs = res.eqsObtain(atpToEquate);
            eub.TransferLeft(eqs);
            res.SavePti(pti);   
            Etr etr = new Etr(res, atpToEquate);
            eqs.TransferLeft(etr);  // check answer
            res.prs.Run();
            return etr;
        }

        /// <summary>
        /// Like EubTest, but have Etp apply the pti to each term when needed
        /// </summary>
        public void EtpTest(string stAtpL, string stPti, string stAtpR, uint nLitNum = 0, bool fFailExpected = false)
        {
            Etr etr = etrRunWithPti(stAtpL, stPti, stAtpR, nLitNum, fFailExpected);
            Assert.IsTrue(etr.rgesnFound != null);
        }

        [Test]
        public void AscParamodulateNew()
        {
            EtpTest("(nil  (p2  (f2  e x) x))", "(nil  (=  (f2  e y) y))", "(nil  (p2 z z))");
            EtpTest("(nil  (=  (f2  e @0) @0))", "(nil  (=  (f2  e @0) @0))", "(nil  (= @0 @0))");

        }

        [Test]
        public void AscParamodulate()
        {
            EtpTest("(() (p2 a a))", "(nil (= a b))", "(nil (p2 b a))");
            EtpTest("(() (p2 a a))", "(nil (= a b))", "(nil (p2 a b))");
            EtpTest("(() (q c) (p1 a) (q d))", "(nil (= a b))", "(nil (q c) (p1 b) (q d))");
            EtpTest("(() (p1 (f2 b x)) (q x))", "(nil (= (f2 x c) (g x a)))", "(nil (p1 (g b a)) (q c))");
            EtpTest("(nil (= (f2 @0 (f2 (g1 @1) @1)) @0))", "(nil (= (g1 e) e))", "(nil (= (f2 @0 (f2 e e)) @0))");
            EtpTest("(() (p1 (f1 b)))", "(nil (= (f1 x) (g x a)))", "(nil (p1 (g b a)))");
            EtpTest("(nil (= e (g1 e)))", "(nil (= @0 (f2 e @0) ))", "(nil (= e (g1 (f2 e e))))");

            // x -> (g y), z -> (g x) -> (g (g y))
            EtpTest("(nil (= (f2 @0 e) @0))",
                    "(nil (= (f2 (f2 (g1 @0) @0) @1) @1))",
                    "(nil (= e (f2 (g1 @0) @0)))");

            // note: fMatches doesn't not check out of order (replace with Equals when that is fixed)
            EtpTest("(nil (p3 y (f2 (g1 y) z) z) (r z))",
                    "(nil (= (f2 x (g1 x)) e) (q x))",
                    "(nil (p3 y e (g1 (g1 y))) (r (g1 (g1 y))) (q (g1 y)) )");
            EtpTest("(nil  (=  (f  e c) (f  a b)))",
                    "(((=  @0 @1)) (=  (f  e @0) @1))",
                    " (((=  c @0)) (=  @0 (f  a b)))",
                    1);
        }
#if false
        [Test]
        public void AscOnDemandParamodulate()
        {

            EubTest("(() (p (f b c)))", "(nil (= (f x d) (g x a)))", "(nil (p (g b a)) (p e))", "(nil (= c d) (p e))");
            EubTest("(() (p (f b c)))", "(nil (= (f x d) (g x a)))", "(nil (p (g b a)))", "(nil (= c d))");
            EubTest("(() (p (f b c)))", "(nil (= (f x d) (g x a)))", "(((p e)) (p (g b a)))", "(((p e)) (= c d) )");
            EubTest("(() (p (f b (g1 b))))", "(nil (= (f x (h1 b)) (g x a)))", "(nil (p (g b a)))", "(nil (= (g1 y) (h1 y)))");
        }

        [Test]
        public void AscOnDemandParamodulateNew()
        {
            EubTest("(() (p (f c)))", "(nil (= (f (h c)) (g a)))", "(nil (p (g a)))", "(nil (= x (h x)))");
            EubTest("(() (p (f b c)))", "(nil (= (f x (h c)) (g x a)))", "(nil (p (g b a)))", "(nil (= x (h x)))");
            EubTest("(nil (p (f b (g1 b))))",
                                "(nil (= (f (h1 b) x) (g x a)))",
                                "(nil (p (g b a)))",
                                "(nil (= (g1 y) (h1 y))) (nil (= (f x y) (f y x)))");
        }
#endif
        public void DoPtiVbvTest(string stAtpL, string stPti, string stAtpR, uint nLitNum=0, bool fFailExpected=false)
        {
            Etr etr1 = etrRunWithPti(stAtpL, stPti, stAtpR, nLitNum, fFailExpected);
            Etr etr2 = etrRunWithPti(stAtpL, stPti, stAtpR, nLitNum, fFailExpected);
            AtpTest.CheckVbvInTwoEtr(etr1, etr2);
        }

        [Test]
        public void VbvWithPtiTest()
        {
            DoPtiVbvTest("(() (p2 a a))", "(nil (= a b))", "(nil (p2 b a))");
            DoPtiVbvTest("(() (p2 a a))", "(nil (= a b))", "(nil (p2 a b))");
            DoPtiVbvTest("(() (q c) (p1 a) (q d))", "(nil (= a b))", "(nil (q c) (p1 b) (q d))");
            DoPtiVbvTest("(() (p1 (f2 b x)) (q x))", "(nil (= (f2 x c) (g x a)))", "(nil (p1 (g b a)) (q c))");
            DoPtiVbvTest("(nil (= (f2 @0 (f2 (g1 @1) @1)) @0))", "(nil (= (g1 e) e))", "(nil (= (f2 @0 (f2 e e)) @0))");
            DoPtiVbvTest("(() (p1 (f1 b)))", "(nil (= (f1 x) (g x a)))", "(nil (p1 (g b a)))");
            DoPtiVbvTest("(nil (= e (g1 e)))", "(nil (= @0 (f2 e @0) ))", "(nil (= e (g1 (f2 e e))))");

            // x -> (g y), z -> (g x) -> (g (g y))
            DoPtiVbvTest("(nil (= (f2 @0 e) @0))",
                    "(nil (= (f2 (f2 (g1 @0) @0) @1) @1))",
                    "(nil (= e (f2 (g1 @0) @0)))");

            // note: fMatches doesn't not check out of order (replace with Equals when that is fixed)
            DoPtiVbvTest("(nil (p3 y (f2 (g1 y) z) z) (r z))",
                    "(nil (= (f2 x (g1 x)) e) (q x))",
                    "(nil (p3 y e (g1 (g1 y))) (r (g1 (g1 y))) (q (g1 y)) )");
            DoPtiVbvTest("(nil  (=  (f  e c) (f  a b)))",
                    "(((=  @0 @1)) (=  (f  e @0) @1))",
                    " (((=  c @0)) (=  @0 (f  a b)))",
                    1);

        }
    }
}