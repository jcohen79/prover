using System;
using System.Collections.Generic;
using System.Text;
using GrammarDLL;
using NUnit.Framework;
using System.Diagnostics;

namespace reslab.test
{

    public class Qst
    {
        Asc ascLeft;
        Asc ascRight;
        int nNumLeft = 0;
        int nNumRight = 0;
        Dictionary<Asc, int> mpasc_nTable;
        GnpR gnp;

        void LogPair()
        {
            int nLeft = mpasc_nTable[ascLeft];
            int nRight = mpasc_nTable[ascRight];
            Debug.WriteLine("(" + nLeft + "," + nRight + ")");
        }

        void Add(Asc ascNew, sbyte nResolveTerm, GnpR.KSide kSide)
        {
            ascNew.nResolveTerm = nResolveTerm;
            Bas basPrev;
            bool fRight = kSide == GnpR.KSide.kRight;
            Bas basSize = fRight ? gnp.basRights : gnp.basLefts;
            Bas.FindBasForSize(Prn.prnObtain(ascNew.rgnTree.Length), ref basSize, out basPrev);
            Bas.AddToFiltered(fRight, gnp, ascNew, basSize, basPrev, true);
        }

        public void SeqTestRun(params int[] rgnCase)
        {
            Debug.WriteLine("");
            mpasc_nTable = new Dictionary<Asc, int>();
            nNumLeft = 0;
            nNumRight = 0;
            LParse lparse = new LParse();
            Sko.AddSyms(lparse, new ExpressionEvaluatorGrammar());
            gnp = new GnpR(null, null);
            int nSoFar = 0;
            int nCount = 0;

            foreach (int nCase in rgnCase)
            {
                if (nCase < 0)
                {
                    if (nCase == -1)
                    {
                        nNumRight++;
                        Asc asc = Asc.ascFromLsx(new Lpr(new Lpr(new Lsm("R" + nCount++), Lsm.lsmNil), new Lpr(Lsm.lsmTrue, Lsm.lsmNil)));
                        Add(asc, Asc.nHasResolveTerm, GnpR.KSide.kRight);
                        mpasc_nTable.Add(asc, nNumRight);
                        //Debug.WriteLine("add Right " + nNumRight);
                    }
                    else if (nCase == -2)
                    {
                        nNumLeft++;
                        Asc asc = Asc.ascFromLsx(new Lpr(Lsm.lsmNil, new Lpr(new Lsm("L" + nCount++), Lsm.lsmNil)));
                        Add(asc, Asc.nNoResolveTerm, GnpR.KSide.kLeft);
                        mpasc_nTable.Add(asc, nNumLeft);
                        //Debug.WriteLine("add Left " + nNumLeft);
                    }
                }
                else
                {
                    nSoFar += nCase;
                    for (int i = 0; i < nCase; i++)
                    {
                        if (!gnp.fGetNextPair(out ascLeft, out ascRight))
                            throw new ArgumentException();
                        LogPair();
                    }
                }
            }
            int nExpected = (nNumLeft * nNumRight) - nSoFar;
            for (int i = 0; i < nExpected; i++)
            {
                if (!gnp.fGetNextPair(out ascLeft, out ascRight))
                    Assert.Fail();
                LogPair();
            }
            if (gnp.fGetNextPair(out ascLeft, out ascRight))
                Assert.Fail();
        }

    }

    public class Pas
    {
        public Asc ascLeft;
        public Asc ascRight;

        public override int GetHashCode()
        {
            return (int)((17 * ascLeft.nId + ascRight.nId) % 1000000);
        }

        public override bool Equals(object obj)
        {
            Pas pas = (Pas)obj;
            return ascLeft == pas.ascLeft && ascRight == pas.ascRight;
        }
    }

    public class Qsp
    {
        Asc ascLeft;
        Asc ascRight;
        int nNumLeft = 0;
        int nNumRight = 0;
        Dictionary<Asc, int> mpasc_nTable;
        HashSet<Pas> mppasSet;
        int nNumLprs = 15;
        Lpr[] rglprLefts;
        Lpr[] rglprRights;
        GnpR gnp;
        Lsx lsxTail = new Lpr(Lsm.lsmTrue, Lsm.lsmNil);
        Lpr lprPos = new Lpr(Lsm.lsmFalse, Lsm.lsmNil);

        public Qsp()
        {
            rglprLefts = new Lpr[nNumLprs];
            rglprRights = new Lpr[nNumLprs];
            for (int i = 0; i < nNumLprs; i++)
            {
                rglprRights[i] = new Lpr(lprPos, lsxTail);
                lsxTail = new Lpr(new Lsm("T" + i), lsxTail);
                rglprLefts[i] = new Lpr(Lsm.lsmNil, lsxTail);
            }
        }

        void Add(Asc ascNew, sbyte nResolveTerm, GnpR.KSide kSide)
        {
            ascNew.nResolveTerm = nResolveTerm;
            Bas basPrev;
            bool fRight = kSide == GnpR.KSide.kRight;
            Bas basSize = fRight ? gnp.basRights : gnp.basLefts;
            Bas.FindBasForSize(Prn.prnObtain(ascNew.rgnTree.Length), ref basSize, out basPrev);
            Bas.AddToFiltered(fRight, gnp, ascNew, basSize, basPrev, true);
        }

        void LogPair()
        {
            int nLeft = mpasc_nTable[ascLeft];
            int nRight = mpasc_nTable[ascRight];

            StringBuilder sb = new StringBuilder();
            sb.Append(nLeft.ToString());
            sb.Append(":");
            sb.Append(ascLeft.rgnTree.Length.ToString());
            sb.Append("(");
            sb.Append(ascLeft.nId.ToString());
            sb.Append("), ");
            sb.Append(nRight.ToString());
            sb.Append(":");
            sb.Append(ascRight.rgnTree.Length.ToString());
            sb.Append("(");
            sb.Append(ascRight.nId.ToString());
            sb.Append(")");
            Debug.WriteLine(sb.ToString());

            Pas pas = new Pas();
            pas.ascLeft = ascLeft;
            pas.ascRight = ascRight;
            Assert.IsFalse(mppasSet.Contains(pas));
            mppasSet.Add(pas);
        }

        /// <summary>
        /// Negative numbers are lefts, positives are rights. 0 means to fetch a pair of resolvants.
        /// The absolute value of the numbers is the size (inverse priority) of the clause.
        /// </summary>
        /// <param name="rgnCase"></param>
        public void SeqTestRun(params int[] rgnCase)
        {
            Debug.WriteLine("");
            mpasc_nTable = new Dictionary<Asc, int>();
            mppasSet = new HashSet<Pas>();
            nNumLeft = 0;
            nNumRight = 0;
            LParse lparse = new LParse();
            Sko.AddSyms(lparse, new ExpressionEvaluatorGrammar());
            gnp = new GnpR(null, null);
            int nSoFar = 0;


            for (int nTestStepNum = 0; nTestStepNum < rgnCase.Length; nTestStepNum++)
            {
                int nCase = rgnCase[nTestStepNum];
                if (nCase == 0)
                {
                    if (!gnp.fGetNextPair(out ascLeft, out ascRight))
                        Assert.Fail();
                    LogPair();
                    nSoFar++;
                }
                else if (nCase < 0)
                {
                    nNumLeft++;
                    lsxTail = new Lpr(new Lsm("T" + nTestStepNum), lsxTail);
                    Asc asc = Asc.ascFromLsx(new Lpr(Lsm.lsmNil, lsxTail));
                    Add(asc, Asc.nHasResolveTerm, GnpR.KSide.kLeft);
                    mpasc_nTable.Add(asc, nNumLeft);
                }
                else
                {

                    nNumRight++;
                    lsxTail = new Lpr(new Lsm("T" + nTestStepNum), lsxTail);
                    Asc asc = Asc.ascFromLsx(new Lpr(lprPos, lsxTail));
                    Add(asc, Asc.nNoResolveTerm, GnpR.KSide.kRight);
                    mpasc_nTable.Add(asc, nNumRight);
                    //Debug.WriteLine("add Right " + nNumRight);
                }
            }
            int nExpected = (nNumLeft * nNumRight) - nSoFar;
            for (int nAfter = 0; nAfter < nExpected; nAfter++)
            {
                if (!gnp.fGetNextPair(out ascLeft, out ascRight))
                    Assert.Fail();
                LogPair();
            }
            if (gnp.fGetNextPair(out ascLeft, out ascRight))
            {
                LogPair();
                Assert.Fail();
            }
        }

    }

    [TestFixture]
    [Category("Clause")]
    class GnpTest
    {
        [Test]
        public void ResolvantBaseTest1()
        {
            Qst qst = new Qst();
            qst.SeqTestRun(-2, -1);
            qst.SeqTestRun(-2, -1, -2, -1);
            qst.SeqTestRun(-2, -1, -1, 1);
            qst.SeqTestRun(-2, -1, 1, -2, -1);

            qst.SeqTestRun(-2, -1, -2, -1, 3, -2, -1);
            qst.SeqTestRun(-2, -1, -1, -2, 1, -1);
            qst.SeqTestRun(-2, -1, -1, -2, 1, -2);
            qst.SeqTestRun(-2, -1, -1, -2, 1, -2, 1, -1, -2);
        }

        [Test]
        public void ResolventPriorityTest1()
        {
            Qsp qsp = new Qsp();
            qsp.SeqTestRun(8, 7, 2, -3, 0, 2, 5, 2, -10, -4, -4, 2, -1, 7);
            qsp.SeqTestRun(2, -2, 1, -1, 0, 0, 3, -3);
            qsp.SeqTestRun(-1, 1, 0, 2, 2, -2, -2, 0, 0, 1, -1, 0, 0, 3, -3);
            qsp.SeqTestRun(-6, 1, -5, 3, -2, 0, -10, -9, -2, 4, -10, -7);

        }

        [Test]
        public void ResolventPriorityTest2()
        {
            Qsp qsp = new Qsp();
            qsp.SeqTestRun(-1, 2, 1, 0, 3);
            qsp.SeqTestRun(1, -2, -3);
            qsp.SeqTestRun(-1, 1);
            qsp.SeqTestRun(-1, 1, 0, 2, -2, 0, 0, 1);
            qsp.SeqTestRun(-1, 1, 0, 2, 2, 0, 1, 0, 3);
            qsp.SeqTestRun(-1, 1, 0, 2, 2, -2, -2, 0, 0, 1);
        }

        [Test]
        public void ResolventPriorityTestRnd()
        {
            Random r = new Random(50);
            for (int i = 3; i < 100; i++)
            {
                Qsp qsp = new Qsp();
                int nLen = r.Next(i);
                int[] rgnCase = new int[nLen];

                Debug.Write("\nqsp.SeqTestRun(");
                bool fPos = false;
                bool fNeg = false;
                for (int j = 0; j < nLen; j++)
                {
                    int n = r.Next(20) - 10;
                    if (n == 0)
                    {
                        if ((!fPos) || !fNeg)
                            n = 2;
                    }

                    rgnCase[j] = n;
                    if (n > 0)
                        fPos = true;
                    if (n < 0)
                        fNeg = true;
                    if (j == 0)
                        Debug.Write(" " + n);
                    else
                        Debug.Write(", " + n);
                }
                Debug.WriteLine(");");
                if (fPos && fNeg)
                    qsp.SeqTestRun(rgnCase);
            }
        }
    }
}
