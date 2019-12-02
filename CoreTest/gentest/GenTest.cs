using GrammarDLL;
using GraphMatching;
using NUnit.Framework;
using System;
using System.Diagnostics;
using WBS;

namespace reslab.test
{
    public interface Igc
    {
        // return true to stop processing
        bool fProcess(Lsx lsxExpr);
    }

    public class GenAndCompare
    {
        public bool fShowReport = false;
        long msInterval = 5000;
        long msNext;
        long msPrev;
        long cExprs;
        long cExprsSince;
        Stopwatch watch;
        public bool fMain = false;


        public void Start()
        {
            watch = new Stopwatch();
            watch.Start();
            msNext = msInterval;
            msPrev = watch.ElapsedMilliseconds;
            cExprs = 0;
            cExprsSince = 0;
        }

        public void Report(Lsx lsxExpr, Lsx lsxSko)
        {
            cExprsSince++;
            long msNow = watch.ElapsedMilliseconds;
            if (msNow > msNext)
            {
                cExprs += cExprsSince;
                double dRate = 1000.0 * ((double)cExprsSince) / (double)(msNow - msPrev);
                String stInfo = String.Format("time={0:n0} exprs={1:n0} rate={2:n0}", msNow / 1000, cExprs, dRate);
                if (fMain)
                {
                    Console.Clear();
                    Console.WriteLine(stInfo);
                    Console.WriteLine();
                    Console.WriteLine(lsxExpr);
                    Console.WriteLine();
                    Console.WriteLine(lsxSko);
                }
                else
                {
                    // Debug.WriteLine(stInfo);
                    Debug.WriteLine(lsxExpr);
                    // Debug.WriteLine(lsxSko);
                    Debug.WriteLine("");
                }
                msNext = msNext + msInterval;
                msPrev = msNow;
                cExprsSince = 0;
            }
        }

        public void GenExprs(Tqd tqdRoot, Igc igc, long nMax)
        {
            Start();

            Ivd dvdData = null;
            // CheckObjCurrent cjc = new CheckObjCurrent();

            Tqs tqsMain = tqdRoot.TqsMake(null);
            Assert.AreEqual(tqsMain.fInitialize(dvdData), KInitializeResult.Succeeded);
            int nCount = 0;

            while (nCount++ < nMax)
            {
                // cjc.Reset();

                object obj = tqsMain.objCurrent(null); //  cjc);
                Lsx lsxExpr = (Lsx)obj;
                Sus susHere;

                // Lsx lsxASko = null;
                Sko sko = new Sko();
                int nNumNotNegated;
                Lsx lsxASko = sko.lsxClausalTransform(lsxExpr, out susHere, out nNumNotNegated);

                if (fShowReport)
                    Report(lsxExpr, lsxASko);

                if (igc != null)
                {
                    if (igc.fProcess(lsxExpr))
                        break;
                }

                if (tqsMain.fMoveNext(dvdData) == KVisitResult.Continue)
                    break;
            }

        }
    }

    public class ExprRemover : Igc
    {
        public ExprRemover(Lsx lsxOld)
        {
            this.lprOld = lsxOld;
        }

        public void Remove(Lsx lsxMem, ref Lsx lsxList)
        {
            if (!(lsxList is Lpr))
                return;
            Lpr lprPrev = null;
            Lsx lsxItem = lsxList;

            while (lsxItem != Lsm.lsmNil)
            {
                Lpr lprItem = (Lpr)lsxItem;
                if (lprItem.lsxCar.fEqual(lsxMem))
                {
                    if (lprPrev == null)
                        lsxList = lprItem.lsxCdr;
                    else
                        lprPrev.lsxCdr = lprItem.lsxCdr;
                }
                lsxItem = lprItem.lsxCdr;
            }
        }

        Lsx lprOld;

        public bool fProcess(Lsx lsxExpr)
        {

                Remove(lsxExpr, ref lprOld);
                if (lprOld == Lsm.lsmNil)
                    return true;
            return false;

        }
    }

    [TestFixture]
    [Category("Clause")]
    public class GenTest
    {

        int cIterations(int nBits, bool fSkipSameName)
        {
            int cCount = 0;
            Gid gid = new Gid(nBits, fSkipSameName, 0);
            long nPrev = 0;
            while (gid.fMore())
            {
                long nVal = gid.nState;
                // Debug.WriteLine(String.Format("{0:X}", nVal));
                Assert.True(nVal != nPrev);
                nPrev = nVal;
                cCount++;
            }
            return cCount;
        }

        int nCombos(int nRange, int nCount)
        {
            int nR = nRange;
            for (int i = 1; i < nCount; i++)
                nR *= (nRange - i);
            for (int i = 2; i <= nCount; i++)
                nR /= i;
            return nR;
        }

        [Test]
        public void _00_BitPatterns()
        {
            int nExpected = 0;
            for (int n = 1; n < 3; n++)
            {
                int nS = nCombos(Gid.nValues, n);
                nExpected += nS;
                Assert.AreEqual(cIterations(n, false), nExpected);
            }

            Assert.AreEqual(cIterations(1, true), Gid.nValues);
            nExpected = Gid.nValues + nCombos(Gid.nValues, 2) - 3 * Gid.nValues;
        }

        [Test]
        public void _01_Vars()
        {
            LParse res = new LParse();
            Sko.AddSyms(res, new ExpressionEvaluatorGrammar());
            Ivd dvdData = null;

            Gid gid = new Gid(2, false);
            GL gl = new GL(res, gid);
            Tqda tqdaValueTwo = new Tqda("valueTwo");
            tqdaValueTwo.SetRg(gl.tqdrConstants);

            Tqdl tqdlTop = new Tqdlr("twoVar",
                        gl.tqdvList,
                        new Tqd[] { tqdaValueTwo, tqdaValueTwo, tqdaValueTwo, tqdaValueTwo, tqdaValueTwo },
                        new Tqdha[] { gl.tqdhaConstants });

            Tqs tqsMain = tqdlTop.TqsMake(null);
            Assert.AreEqual(tqsMain.fInitialize(dvdData), KInitializeResult.Succeeded);
            int nCount = 0;

            while (nCount++ < 1000)
            {
                object obj = tqsMain.objCurrent(null); //  cjc);
                Lsx lsxExpr = (Lsx)obj;

                //Debug.WriteLine(lsxExpr);

                if (tqsMain.fMoveNext(dvdData) == KVisitResult.Continue)
                    break;
            }
            Assert.AreEqual(41, nCount);
        }

        private bool fCheckFirstVar(Lsx lsxNode)
        {
            if (lsxNode is Lsm)
            {
                Lsm lsmNode = (Lsm)lsxNode;
                Assert.False(lsmNode.stName.Equals("b"));
                return lsmNode.stName.Equals("a");
            }
            Lpr lprNode = (Lpr)lsxNode;
            if (fCheckFirstVar(lprNode.lsxCar))
                return true;
            return fCheckFirstVar(lprNode.lsxCdr);
        }

        private bool fCheckMax (Tqs tqs, GL gl)
        {
            if (tqs is Tqsl)
            {
                Tqsl tqsl = (Tqsl)tqs;
                foreach (Tqs tqsChild in tqsl.rgtqsChildren)
                {
                    if (fCheckMax(tqsChild, gl))
                        return true;
                }
            }
            else if (tqs is Tqsa)
            {
                Tqsa tqsa = (Tqsa)tqs;
                return fCheckMax(tqsa.tqsActive, gl);
            }
            else if (tqs is Tqsr)
            {
                Tqsr tqsr = (Tqsr)tqs;
                if (tqsr.tqdha == gl.tqdhaConstants)
                    Assert.True(tqsr.nMax == 1);
                return true;
            }
            return false;
        }

        [Test]
        public void _01a_MoreVars()
        {
            LParse res = new LParse();
            Sko.AddSyms(res, new ExpressionEvaluatorGrammar());
            Ivd dvdData = null;

            Gid gid = new Gid(2, false);
            GL gl = new GL(res, gid);

            Tqda tqdaExprList = new Tqda("exprlist", gl.tqdlExprList1 //, tqdlExprList2 // ,tqdlExprList3
                                        );

            Tqdlr tqdlSequent = new Tqdlr("sequent",
                                    gl.tqdvEntails,
                                    new Tqd[] { tqdaExprList, tqdaExprList },
                                    new Tqdha[] { gl.tqdhaConstants, gl.tqdhaFnNames1,  gl.tqdhaFnNames2,
                                                    gl.tqdhaPredNames1, gl.tqdhaPredNames2
                                                });
            gl.tqdaValue.SetRg(gl.tqdrConstants,
                            gl.tqdaVarRefs, gl.tqdlFnCall1 
                            );
            gl.tqdaBool.SetRg(
                           gl.tqdlPredCall1,
                            // gl.tqdlQuantExpr,
                            gl.tqdlBinExpr,
                            gl.tqdlUnaryExpr,
                            gl.tqdlNaryExpr1 // , gl.tqdlNaryExpr2
                            );
            gl.SetDepth(4);

            Tqs tqsMain = tqdlSequent.TqsMake(null);
            Assert.AreEqual(tqsMain.fInitialize(dvdData), KInitializeResult.Succeeded);
            int nCount = 0;

            while (nCount++ < 100000)
            {
                fCheckMax(tqsMain, gl);
                object obj = tqsMain.objCurrent(null); //  cjc);
                Lsx lsxExpr = (Lsx)obj;

                // Debug.WriteLine(lsxExpr);
                // if (nCount == 14413)
                // fCheckFirstVar(lsxExpr);

                //if (nCount == 7256)
                //    Debug.WriteLine("look");
                if (tqsMain.fMoveNext(dvdData) == KVisitResult.Continue)
                    break;
            }
        }

        [Test]
        public void _01b_TestGidStates()
        {
            Gid gid = new Gid(5, false);
            gid.TestMore(3678217, 5, new int[] { 19, 20, 21 });
        }

        [Test]
        public void _02_ShowGenSequents()
        {
            LParse res = new LParse();
            Sko.AddSyms(res, new ExpressionEvaluatorGrammar());

            Gid gid = new Gid(2, false);
            GL gl = new GL(res, gid);
            // Tqd tqdRoot = GL.tqdlQuantExpr;
            Tqd tqdRoot = gl.tqdlSequent;

            GenAndCompare gac = new GenAndCompare();
            gac.fShowReport = true;
            gac.GenExprs(tqdRoot, null, 100);
        }

        [Test]
        public void _03_CheckGenSequents()
        {
            LParse res = new LParse();
            Sko.AddSyms(res, new ExpressionEvaluatorGrammar());

            Gid gid = new Gid(2, false);
            GL gl = new GL(res, gid);
            // Tqd tqdRoot = GL.tqdlQuantExpr;
            Tqd tqdRoot = gl.tqdlSequent;

            Lsx lsxOld = res.lsxParse(stOld);
            GenAndCompare gac = new GenAndCompare();
            ExprRemover er = new ExprRemover(lsxOld);

            gac.GenExprs(tqdRoot, er, 1000);
        }

        public static string stOld = @"(
(entails 
   (list  (p1  a))
   (list  (p1  a)))

(entails 
   (list  (p1  a))
   (list  (p1  b)))

(entails 
   (list  (p1  a))
   (list 
      (p1  (f1  a))))

(entails 
   (list  (p1  a))
   (list 
      (p1  (f1  b))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f1  (f1  a)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f1  (f1  b)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f1  (g1  a)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f1  (g1  b)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f1  (f2  a a)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f1  (f2  a b)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f1  (f2  b a)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f1  (f2  b b)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f1  (f2  b c)))))

(entails 
   (list  (p1  a))
   (list 
      (p1  (f2  a a))))

(entails 
   (list  (p1  a))
   (list 
      (p1  (f2  a b))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  a (f1  a)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  a (f1  b)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  a (f2  a a)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  a (f2  a b)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  a (f2  b a)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  a (f2  b b)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  a (f2  b c)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  a (g2  a a)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  a (g2  a b)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  a (g2  b a)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  a (g2  b b)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  a (g2  b c)))))

(entails 
   (list  (p1  a))
   (list 
      (p1  (f2  b a))))

(entails 
   (list  (p1  a))
   (list 
      (p1  (f2  b b))))

(entails 
   (list  (p1  a))
   (list 
      (p1  (f2  b c))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  b (f1  a)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  b (f1  b)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  b (f1  c)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  b (f2  a a)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  b (f2  a b)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  b (f2  a c)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  b (f2  b a)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  b (f2  b b)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  b (f2  b c)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  b (f2  c a)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  b (f2  c b)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  b (f2  c c)))))

(entails 
   (list  (p1  a))
   (list 
      (p1 
         (f2  b (g2  a a)))))
)";

    }
}
