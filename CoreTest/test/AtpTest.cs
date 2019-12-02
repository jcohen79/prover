using System;
using GrammarDLL;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using GraphMatching;

namespace reslab.test
{
    public class TestInfo
    {
        public string stName;

        public TestInfo (string stName)
        {
            this.stName = stName;
        }
    }

    public class TpTestBase
    {
        public LParse lparse;
        public int nTestNum = 0;

        [SetUp]
        public void Init()
        {
            lparse = new LParse();
            Sko.AddSyms(lparse, new ExpressionEvaluatorGrammar());

            foreach (string st in Cbd.rgstVarNames)
                Cbd.AddVar(lparse, st);

        }

        public Atp atpMake(string stL, string stR)
        {
            return Atp.atpMake(lparse, stL, stR);
        }

        protected Pti ptiMake(Cbd cbd, string stAsc, uint nLiteralNum = 0)
        {

            Asc asc = cbd.ascBuild(stAsc);
            asc.nResolveTerm = Asc.nNoResolveTerm;
            Ipr iprPriority = Prn.prnObtain(asc.rgnTree.Length);
            int nFromOffset = Asc.nClauseLeadingSizeNumbers;
            int nSkipLit = (int) nLiteralNum;
            while (nSkipLit-- > 0)
            {
                nFromOffset += asc.nTermSize(nFromOffset);
            }
            nFromOffset++;  // From is first term after the =
            int nToOffset = nFromOffset + asc.nTermSize(nFromOffset);
            Pti pti = Pti.ptiMakeEq(asc, nFromOffset, nToOffset, iprPriority, nLiteralNum);
            return pti;
        }

        public bool fMatch(Lsx lsxA, Lsx lsxB)
        {
            if (lsxA == lsxB)
                return true;
            if (lsxA is Lsm)
            {
                Lsm lsmA = (Lsm)lsxA;
                if (!(lsxB is Lsm))
                    return false;
                Lsm lsmB = (Lsm)lsxB;
                return lsmA.stName.Equals(lsmB.stName);
            }
            Lpr lprA = (Lpr)lsxA;
            if (lsxB is Lsm)
                return false;
            Lpr lprB = (Lpr)lsxB;
            if (!fMatch(lprA.lsxCar, lprB.lsxCar))
                return false;
            if (!fMatch(lprA.lsxCdr, lprB.lsxCdr))
                return false;
            return true;
        }

    }

    /// <summary>
    /// Test requestor
    /// </summary>
    public class Etr : Bid, Ent
    {
        public List<Esn> rgesnFound;
        Ent entNext;
        Atp atpToEquate;
        Res res;

        public Etr(Res res, Atp atpToEquate)
        {
            this.res = res;
            this.atpToEquate = atpToEquate;
        }

        public bool fProcessSolution(Esn esnSolution)
        {
            if (rgesnFound == null)
                rgesnFound = new List<Esn>();
            rgesnFound.Add(esnSolution);

            Vbv vbv = (Vbv)esnSolution;

            int nLeftSize = atpToEquate.rgnTree[Atp.nOffsetLeftSize];
            Moa mobToOffsetForChildValidate;
            Mva mvbMapToVblIdForChildValidate;
            Spl spl = new Spl();
            spl.Init(atpToEquate, Atp.nOffsetFirstTerm,
                atpToEquate, (ushort)(Atp.nOffsetFirstTerm + nLeftSize),
                vbv, vbv, false);
            // vbvRight is null: don't apply the pti to the rhs, that is assumed done to the lhs.

            Atp atpResult = spl.atpCreateOutput(
                out mobToOffsetForChildValidate, out mvbMapToVblIdForChildValidate);
            if (!atpResult.fSymmetric(true))
                throw new ArgumentException();

            res.prs.Done();

            return false;
        }

        public Lsx lsxTo(Asy asy)
        {
            throw new NotImplementedException();
        }

        public void NextSameSize(Ent riiNext)
        {
            entNext = riiNext;
        }

        public Ipr iprComplexity()
        {
            return Prn.prnObtain(0);
        }

        public Ent riiNextSameSize()
        {
            return entNext;
        }

    }

    [TestFixture]
    [Category("Clause")]
    class AtpTest : TpTestBase
    {
        public void AscEqualTest(string stL, string stR, bool fExpected = true)
        {
            Lsx lsxL = lparse.lsxParse(stL);
            Asc ascL = Asc.ascFromLsx(lsxL);
            Lsx lsxR = lparse.lsxParse(stR);
            Asc ascR = Asc.ascFromLsx(lsxR);
            if (fExpected)
                Assert.IsTrue(ascL.Equals(ascR));
            else
                Assert.IsFalse(ascL.Equals(ascR));
        }

        [Test]
        public void AscEqual()
        {
            AscEqualTest("(((=  @0 (f  @1 a))  (f  @2 e) (=  @1 b)))",
                         "(( (f  @0 e) (=  @1 b) (=  @2 (f  @1 a))))");
            AscEqualTest("(((=  @0 (f  @1 a))  (f  @2 e) (=  @1 b)))",
                         "(( (f  @1 e) (=  @2 b) (=  @0 (f  @2 a))))");
            AscEqualTest("(((=  @0 (f  @1 a)) (=  (f  a b) (f  @2 e)) (=  @1 b)))",
                         "(((=  (f  a b) (f  @1 e)) (=  @2 b) (=  @0 (f  @2 a))))");
            AscEqualTest("(((=  @0 (f  @1 a)) (=  @0 @2) (=  (f  a b) (f  @2 e)) (=  @1 b)))",
                         "(((=  @0 @1) (=  (f  a b) (f  @1 e)) (=  @2 b) (=  @0 (f  @2 a))))");
            AscEqualTest("(((f @0 a)) (f b @1))", "(((f @0 a)) (f b @1))");
            AscEqualTest("(((f @0 a)) (f b @1))", "(((f @0 a)) (f b @0))", false);
        }

        public void MakeAtpTest(string stL, string stR, string stExpected)
        {
            Atp atp = atpMake(stL, stR);
            Lsx lsxExpected = lparse.lsxParse(stExpected);
            Asy asy = new Asy();
            Lsx lsxAtp = atp.lsxTo(asy);
            Assert.IsTrue(fMatch(lsxAtp, lsxExpected));
        }

        [Test]
        public void AtpMake()
        {
            MakeAtpTest("(() (a b x))", "(() (c x y))", "(((a b @0)) ((c @1 @2)))");
            MakeAtpTest("(() (f (c z x) (a b x)))", "(() (g (d x) (c x y)))",
                        "(((f (c @0 @1) (a b @1))) ((g (d @2) (c @2 @3))))");
        }

#if false
        public void MakeAtpChildTest(bool fAfter, string stL, string stR, string stExpected)
        {
            Atp atp = atpMake(stL, stR);
            Lsx lsxExpected = lparse.lsxParse(stExpected);
            Asy asy = new Asy();
            int nSkipFn = fAfter ? 1 : 0;
            Etp etpPrev = new Etp();
            sbyte[] rgnMapToOffset;
            sbyte[] rgnMapToVblId;
            Atp atpChild = atp.atpSplitAtFirstChild(etpPrev, null, out rgnMapToOffset, out rgnMapToVblId);
            Lsx lsxAtpChild = atpChild.lsxTo(asy);
            Assert.IsTrue(fMatch(lsxAtpChild, lsxExpected));
        }

        [Test]
        public void AtpMakeChild()
        {
            MakeAtpChildTest(false, "(() (a b x))", "(() (c x y))", "(((a b @0)) ((c @1 @2)))");
            MakeAtpChildTest(false, "(() (a b x) (c z x))", "(() (c x y) (d x))", "(((a b @0)) ((c @1 @2)))");
            MakeAtpChildTest(true, "(() (f (c z x) (a b x)))", "(() (g (d x) (c x y)))",
                             "(((a b @0)) ((c @1 @2)))");
        }
#endif

        [Test]
        public void VbvEqualTest()
        {
            /*
             <#3868 
                <#3869 1/6 0:2$3868>     (nil  (=  (f  @0 e) @0))
                >     (((f  b e)) (b))
            */
            // {<#3868 <#3869 1/6 0:2$3868>>}	
            Lsx lsxA = lparse.lsxParse("(nil  (=  (f  @0 e) @0))");
            Lsx lsxB = lparse.lsxParse("(((f  b e)) (b))");
            Asc ascA = Asc.ascFromLsx(lsxA);
            Asc ascB = Asc.ascFromLsx(lsxB);
            Vbv vbvChild = new Vbv(ascA);
            Vbv vbvParent = new Vbv(ascB);
            vbvChild.nReplaceAtPosn = 1;
            vbvChild.nReplaceWithPosn = 6;
            vbvChild.vbaAdd(0, 2, vbvParent);
            vbvParent.vbvFirst = vbvChild;
            Assert.AreNotEqual(vbvParent, vbvChild);
            Assert.AreNotEqual(vbvChild, vbvParent);
        }

        [Test]
        public void HlaTest()
        {
            Hla<TestInfo> hla = new Hla<TestInfo>();
            Atp atp = atpMake("(() (a b x))", "(() (c x y))");
            TestInfo info = new TestInfo("info");
            Assert.IsNull(hla.valGet(atp.rgnTree));
            hla.Add(atp.rgnTree, info);
            TestInfo info2 = hla.valGet(atp.rgnTree);
            Assert.AreEqual(info.stName, info2.stName);
        }

        [Test]
        public void HuaTest()
        {
            Hua<TestInfo> hua = new Hua<TestInfo>();
            Atp atp = atpMake("(() (a b x))", "(() (c x y))");
            TestInfo info = new TestInfo("info");
            Assert.IsNull(hua.valGet(atp));
            hua.Add(atp, info);
            TestInfo info2 = hua.valGet(atp);
            Assert.AreEqual(info.stName, info2.stName);
        }

#if false
        public void DoSbstTest(string stL, string stExpected, ushort nSbstAtOffset, params Object[] rgobjVbv)
        {
            string stPredName = "sbstTest" + nTestNum++;
            Vbv vbv = new Vbv(null, (Asb) null);
            sbyte nId = 0;
            bool fGetId = true;
            string stValues = "";
            foreach (Object obj in rgobjVbv)
            {
                if (fGetId)
                    nId = (sbyte)(int)obj;
                else
                {
                    string stValue = (string)obj;
                    stValues = " " + stValue + stValues;
                    vbv.Add(nId, 0, Vbv.vbvA);
                }

                fGetId = !fGetId;
            }
            string stRight = "(() (" + stPredName + stValues + "))";
            Vba vba = vbv.rgvbaList;
            Atp atp = atpMake(stL, stRight);
            int nPosn = atp.rgnTree[Atp.nOffsetLeftSize] + Atp.nOffsetFirstTerm + 1;
            while (vba != null)
            {
                vba.nValue = (ushort) nPosn;
                vba = vba.vbaPrev;
                nPosn += atp.nTermSize(nPosn);
            }

            Asc ascResult = atp.ascSbst(vbv, nSbstAtOffset, atp, vbv);

            Lsx lsxExpected = lparse.lsxParse(stExpected);


            Asy asy = new Asy();
            Lsx lsxAsc = ascResult.lsxTo(asy);
            Assert.IsTrue(fMatch(lsxAsc, lsxExpected));
        }

        [Test]
        public void SbstTest()
        {
            DoSbstTest("(() (a3 x y x))", "(() (a3 @0 @1 @0))", 1);
            DoSbstTest("(() (a3 x y x))", "(() (a3 c @0 c))", 1, 0, "c");
            DoSbstTest("(() (a3 x y x))", "(() (a3 c (g @0) c))", 1, 0, "c", 1, "(g x)");
        }
#endif
        static Etr etrRunEqs(Atp atpToEquate)
        {
            Res res = new Res();
            res.fUseNegatedGoals = false;
            res.Init();
            Etr etr = new Etr(res, atpToEquate);
            Eqs eqsStart = new Eqs(res, atpToEquate);
            eqsStart.FirstStep();
            eqsStart.TransferLeft(etr);
            res.prs.Add(eqsStart);
            res.prs.Run();
            return etr;
        }


        /// <summary>
        /// Check that eqs can be solved or is expected to not have solution
        /// </summary>
        public void DoEqsTest(bool fSolnExpected, string stL, string stR)
        {
            Atp atpToEquate = atpMake(stL, stR);
            Etr etr = etrRunEqs(atpToEquate);
            if (etr.rgesnFound == null)
            {
                Assert.IsFalse(fSolnExpected);
                return;
            }
            Assert.IsTrue(etr.rgesnFound != null);
            foreach (Esn esn in etr.rgesnFound)
            {
                // how to check solution?
            }
        }


        [Test]
        public void EqsTest()
        {
            DoEqsTest(true, "(() (a1 x))", "(() (a1 b))");
            DoEqsTest(true, "(() (a0))", "(() (a0))");
            DoEqsTest(true, "(() (a1 b))", "(() (a1 b))");
            DoEqsTest(true, "(() (a2 b c))", "(() (a2 b c))");
            DoEqsTest(true, "(() (a2 (a1 b) c))", "(() (a2 (a1 b) c))");
            DoEqsTest(true, "(() (a2 (a1 (b1 d)) c))", "(() (a2 (a1 (b1 d)) c))");
            DoEqsTest(false, "(() (a2 x c))", "(() (a2a b c))");
            DoEqsTest(true, "(() (a2 x y))", "(() (a2 b c))");
            DoEqsTest(true, "(() (a3 x y x))", "(() (a3 b c b))");
            DoEqsTest(false, "(() (a3 x y x))", "(() (a3 b c c))");
            DoEqsTest(false, "(() (a2 x (g a)))", "(() (a2 (f y) y))");
            DoEqsTest(false, "(() (a3 x (g a) x))", "(() (a3 (f y) y (f (g a))))");
            DoEqsTest(false, "(() (a2 x y))", "(() (a2 (f y) (f x)))");
            DoEqsTest(false, "(() (a2 x y))", "(() (a2 (f y) (g y)))");
            DoEqsTest(true, "(() (a2 x y))", "(() (a2 y x))");
            DoEqsTest(true, "(() (a3 x y z))", "(() (a3 y z x))");
            DoEqsTest(true, "(nil (p x a))", "(((p (f b) y))");
            DoEqsTest(true, "(nil  (P  @0 (H  @1 @1) @0))", "(((P  (K  @0) @0 (K  @0))))");
        }

        [Test]
        public void EqsTestNew()
        {
            DoEqsTest(false, "(() (a0))", "(() (b0))");
            DoEqsTest(false, "(() (a2 x x))", "(() (a2 b c))");
        }

        public static void CheckVbvInTwoEtr(Etr etr1, Etr etr2)
        {
            Assert.IsFalse(etr1.rgesnFound == null);
            Assert.IsFalse(etr2.rgesnFound == null);
            Assert.AreEqual(etr1.rgesnFound.Count, etr2.rgesnFound.Count);
            int nPosn = 0;
            foreach (Esn esn in etr1.rgesnFound)
            {
                Vbv vbv1 = (Vbv)esn;
                for (int nOther = 0; nOther <= nPosn; nOther++)
                {
                    Vbv vbv2 = (Vbv)etr2.rgesnFound[nPosn];
                    int nHash1 = vbv1.GetHashCode();
                    int nHash2 = vbv2.GetHashCode();
                    if (nOther == nPosn)
                    {
                        Assert.IsTrue(vbv1.Equals(vbv2));
                        Assert.AreEqual(nHash1, nHash2);
                    }
                    else
                    {
                        Assert.IsTrue(!vbv1.Equals(vbv2));
                        Assert.AreNotEqual(nHash1, nHash2);
                    }
                }
                nPosn++;
            }
        }

        /// <summary>
        /// Use Eqs to create a series of Vbv to be checked
        /// </summary>
        public void DoVbvTest(string stL, string stR)
        {
            Atp atpToEquate = atpMake(stL, stR);
            Etr etr1 = etrRunEqs(atpToEquate);
            Etr etr2 = etrRunEqs(atpToEquate);
            CheckVbvInTwoEtr(etr1, etr2);
        }

        [Test]
        public void VbvTest()
        {
            DoVbvTest("(() (a0))", "(() (a0))");
            DoVbvTest("(() (a1 b))", "(() (a1 b))");
            DoVbvTest("(() (a2 b c))", "(() (a2 b c))");
        }

        [Test]
        public void VbvTestNew()
        {
            DoVbvTest("(() (a2 (a1 b) c))", "(() (a2 (a1 b) c))");
            DoVbvTest("(() (a2 (a1 (b1 d)) c))", "(() (a2 (a1 (b1 d)) c))");
            DoVbvTest("(() (a2 x y))", "(() (a2 b c))");
            DoVbvTest("(() (a3 x y x))", "(() (a3 b c b))");
            DoVbvTest("(() (a2 x y))", "(() (a2 y x))");
            DoVbvTest("(() (a3 x y z))", "(() (a3 y z x))");
            DoVbvTest("(nil (p x a))", "(((p (f b) y))");
        }


    }
}