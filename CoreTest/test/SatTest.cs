using GrammarDLL;
using NUnit.Framework;
using reslab.test;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WBS;

namespace reslab
{

    public interface Icf
    {
        Ici iciMakeInstance(int nId);
    }

    public interface Ici
    {
        void DoStep();

        string stProgress();

        int nGetProgressPercent();
    }

    public class SatTestFactory : Icf
    {
        Ivd dvdData = null;
        TqsGid tqsGid;
        public LParse res;

        public SatTestFactory(Gid gid)
        {
            res = new LParse();
            tqsGid = new TqsGid(gid, res);
            Assert.AreEqual(tqsGid.fInitialize(dvdData), KInitializeResult.Succeeded);

        }

        public Lsx lsxGetNextSequent(Imc imc)
        {
            Lsx lsxExpr;
            lock (this)
            {
                lsxExpr = (Lsx)tqsGid.objCurrent(imc);
                tqsGid.fMoveNext(dvdData);
            }
            return lsxExpr;
        }

        public Ici iciMakeInstance(int nId)
        {
            return new SatTestWorker(nId, this);
        }

    }

    public class SatTestWorker : Ici
    {
        SatTestFactory stf;
        public CompareExpr ce;
        public readonly int nId;
        public int nStepNum;
        public Lsx lsxExpr;
        public Lsx lsxASko;
        Smf smfWorker;

        public SatTestWorker(int nId, SatTestFactory stf)
        {
            this.nId = nId;
            this.stf = stf;

            EvalIst ei = new EvalIst();
            ce = new CompareExpr(stf.res, ei);
            smfWorker = new Smf();
        }

        public void DoStep()
        {
            nStepNum++;

            // Dictionary<Lsm, Lsm> rglsm_lsm = new Dictionary<Lsm, Lsm>();
            // lsxExpr = EvalIst.lsxCopyUniqueLsm(stf.lsxGetNextSequent(), rglsm_lsm);
            lsxExpr = stf.lsxGetNextSequent(smfWorker);

            Sus susHere;
            Sko sko = new Sko();
            int nNumNotNegated;
            lsxASko = sko.lsxClausalTransform(lsxExpr, out susHere, out nNumNotNegated);

            if (ce.fProcess(lsxExpr))
                throw new ArgumentException();
        }

        public string stProgress()
        {
            return "\n" + lsxExpr.ToString();
        }
        public int nGetProgressPercent()
        {
            return nStepNum % 100;
        }
    }

    class Smf : Imc
    {
        Dictionary<Lsm, Lsm> rglsm_lsm = new Dictionary<Lsm, Lsm>();

        public Lsm lsmReplace(Lsm lsmExpr)
        {
            if (lsmExpr.fPredefined())
                return lsmExpr;
            Lsm lsmBefore = null;
            if (rglsm_lsm.TryGetValue(lsmExpr, out lsmBefore))
                return lsmBefore;
            lsmBefore = new Lsm(lsmExpr.stName);
            rglsm_lsm.Add(lsmExpr, lsmBefore);
            return lsmBefore;

        }

    }

    class CheckSatResult : Ist
    {
        Lpr lprHead = null;
        Lpr lprTail = null;
        public bool fFailed = false;

        public override void Failed()
        {
            fFailed = true;
        }

        public override bool fPass(Lpr lprQuad)
        {
            // Console.WriteLine(lprQuad);
            Sko.AddToResult(lprQuad, ref lprHead, ref lprTail);
            return true;
        }

        public Lsx lsxResult()
        {
            return lprHead != null ? (Lsx)lprHead : Lsm.lsmNil;
        }
    }


    public class EvalIst : Ist
    {
        Lsx lsxR;
        Sko sko;
        bool fFlipRhs = false;

        public EvalIst()
        {
        }

        public override bool fPass(Lpr lprSatSol)
        {
            this.nMatched++;
            Lsx lsxNegAssignments = lprSatSol.lsxCar;
            Lsx lsxPosAssignments = lprSatSol.lsxCdr;
            Eval eval = new Eval(lsxNegAssignments, lsxPosAssignments, sko.qtlList);
            Lsx lsxRCdr = ((Lpr)lsxR).lsxCdr;
            Lsx lsxVal = eval.lsxEvalEntails(lsxRCdr, false, false, Sus.susRoot, fFlipRhs);
            Assert.AreEqual(lsxVal, Lsm.lsmTrue); // fFlipRhs ? Lsm.lsmFalse : Lsm.lsmTrue);
            return false;
        }

        public override void Failed()
        {
            if (fFlipRhs)
                // quite trying for now
                // CheckSat("(nil (p1  a))  (((p1  b)) (p1  x)) (((p1  x)) (p1  b))", ""); //  "((((p1  x) (p1  b)) (p1  a)))");
                return;

            nFailed++;
            EvalAndCompare(lsxR, true);
        }

        public static Lsx lsxCopyUniqueLsm(Lsx lsxExpr, Dictionary<Lsm,Lsm> rglsm_lsm)
        {
            if (lsxExpr is Lpr)
            {
                Lpr lprExpr = (Lpr)lsxExpr;
                Lsx lsxL = lsxCopyUniqueLsm(lprExpr.lsxCar, rglsm_lsm);
                Lsx lsxR = lsxCopyUniqueLsm(lprExpr.lsxCdr, rglsm_lsm);
                if (lsxL == lprExpr.lsxCar && lsxR == lprExpr.lsxCdr)
                    return lprExpr;
                return new Lpr(lsxL, lsxR);
            }
            if (lsxExpr is Lsm)
            {
                Lsm lsmExpr = (Lsm) lsxExpr;
                if (lsmExpr.fPredefined())
                    return lsmExpr;
                Lsm lsmBefore = null;
                if (rglsm_lsm.TryGetValue(lsmExpr, out lsmBefore))
                    return lsmBefore;
                lsmBefore = new Lsm(lsmExpr.stName);
                rglsm_lsm.Add(lsmExpr, lsmBefore);
                return lsmBefore;
            }
            return lsxExpr;
        }


        public void EvalAndCompare(Lsx lsxA, bool fFlipRhs = false)
        {
            lsxR = lsxA;
            this.fFlipRhs = fFlipRhs;
            Sus susVarList;
            sko = new Sko();

            int nNumNotNegated;
            Lsx lsxSko = sko.lsxClausalTransform(lsxR, out susVarList, out nNumNotNegated, fFlipRhs);
            if (lsxSko == Lsm.lsmTrue)
            {
                this.nTautologyTrue++;
                return;
            }
            if (lsxSko == Lsm.lsmFalse)
            {
                this.nTautologyFalse++;
                return;
            }

            Sus susVar = susVarList;
            while (susVar.lsmSymbol != null)
            {
                susVar.lsmSymbol.MakeVariable();
                // res.AddSym(susVar.lsmSymbol, null);
                susVar = susVar.susPrev;
            }

            /*
            * Rationale for test: SKO finds a model that satisfies the requirements of the assertion (an entails).
            * But entails asserts there are no values for the model it talking about, so it returns false when passed the results of SKO.
            * This is not a contradiction because the entails statement is referred to just a particular model.
            * (Book talks about pure(L), but that has no individuals. SAT instroduces individuals.)
            */


            Sst.Perform((Lpr)lsxSko, this, 1);   // calls Pass and Failed
        }
    }

    public class CompareExpr : Igc
    {
        LParse res;
        EvalIst ei;

        public CompareExpr(LParse res, EvalIst ei)
        {
            this.res = res;
            this.ei = ei;
        }

        // return true to stop processing
        public bool fProcess(Lsx lsxExpr)
        {
            ei.EvalAndCompare(lsxExpr);
            return false;
        }
    }

    [TestFixture]
    [Category("Clause")]
    class SatTest
    {
        LParse res;

        void CheckSat(string stInputCNF, string stExpected)
        {
            CheckSatResult csr = new CheckSatResult();

            Lpr lprA = (Lpr)res.lsxParse("( " + stInputCNF + " )");
            Sst.Perform(lprA, csr, 0);
            // Assert.IsFalse(csr.fFailed);

            Lsx lsxRes = csr.lsxResult();
            Lsx lsxExpected = res.lsxParse("( " + stExpected + ")");
            // stExpected is a (DNF) list of clauses. Each clause has two lists: neg terms and pos terms

            Assert.IsTrue(Scm.scmSequent.fSame(lsxRes, lsxExpected));
        }


        public static void UsualVariables(LParse lparse)
        {
            foreach (string stVar in new string[] { "r", "s", "u", "v", "w", "x", "y", "z", "X", "Y", "Z" })
            {
                Lsm lsmVar = new Lsm(stVar);
                lsmVar.MakeVariable();
                lparse.AddSym(lsmVar, null);
            }

        }

        [Test]
        public void SatVarTest()
        {

            res = new LParse();
            Sko.AddSyms(res, new ExpressionEvaluatorGrammar());
            UsualVariables(res);

            CheckSat("(nil (p1  a))   (nil (p1  b)) (((p1  b) (p1  a)))", "");


            CheckSat("(nil (p1  a))  (((p1  b)) (p1  x)) (((p1  x)) (p1  b))", ""); //  "((((p1  x) (p1  b)) (p1  a)))");
                                // and/or exchange of: "(nil (p1 a)) ((p1 b)) ((p1 x)))"

            CheckSat("(((P y (G x)))) (nil (P z (G x)))", "");
            CheckSat("(((P y z y))) (nil (P z a a))", "");
            CheckSat("(((P u v w x y z y u a))) (nil (P v w u y z x v a z))", "");
            CheckSat("(((P u v w x y z y))) (nil (P v w x y z a a))", "");
            CheckSat("(((P u v w x y z))) (nil (P v w x y z u))", "");

            CheckSat("(((R a))) (nil (P x b))", "(((R a)) (P x b))");
            CheckSat("(((P a))) (nil (P x))", "");
            CheckSat("(((P a))) (nil (P b))", "(((P a)) (P b))");
            CheckSat("(((P a))) (nil (P a))", "");
            CheckSat("(((P y a))) (nil (P x y))", "");
            CheckSat("(((P y a z))) (nil (P x y c))", "");
            CheckSat("(((P y a b))) (nil (P x y c))", "(((P y a b)) (P x y c))");

            CheckSat("(nil (P (G x)))", "(nil (P (G x)))");
        }

        [Test]
        public void MdlTest()
        {
            Mdl mdl = new Mdl();
            Lsm lsmA = new Lsm("a");
            Lsm lsmB = new Lsm("b");
            Lsm lsmX = new Lsm("x");
            Lsm lsmG = new Lsm("G");
            Lsm lsmP = new Lsm("P");
            Lsm lsmF = new Lsm("F");
            lsmG.MakeVariable();
            lsmX.MakeVariable();
            mdl.DefFunction(lsmP, false, 2);
            mdl.DefFunction(lsmF, true, 1);
            mdl.AddConstant(lsmA);
            mdl.AddConstant(lsmB);

            mdl.Reset();
            Assert.IsTrue(mdl.fValueForTerm(Lpr.lprList(lsmP, lsmX, lsmA), true));
            Assert.IsFalse(mdl.fValueForTerm(Lpr.lprList(lsmP, lsmB, lsmX), false));

            mdl.Reset();
            Assert.IsTrue(mdl.fValueForTerm(Lpr.lprList(lsmP, lsmA, lsmA), true));
            Assert.IsTrue(mdl.fValueForTerm(Lpr.lprList(lsmP, lsmB, lsmB), false));
            Assert.IsFalse(mdl.fValueForTerm(Lpr.lprList(lsmP, lsmX, lsmX), false));

            mdl.Reset();
            Assert.IsTrue(mdl.fValueForTerm(Lpr.lprList(lsmP, lsmX, Lpr.lprList(lsmF, lsmA)), true));
        }

        public void ParseEvalAndCompare(string stExpr)
        {
            res = new LParse();
            Sko.AddSyms(res, new ExpressionEvaluatorGrammar());
            Lsx lsxR = res.lsxParse(stExpr);
            EvalIst ei = new EvalIst();
            ei.EvalAndCompare(lsxR);
        }

        [Test]
        public void EvalTest()
        {
            ParseEvalAndCompare("(entails (list (p1  a)) (list (iff (p1  a) (exists x (q1  x)))))");
            ParseEvalAndCompare("(entails (list (p1  a)) (list (exists  x (q1  x))))");
            ParseEvalAndCompare("(entails (list (p1  a)) (list (forall  x (iff (p1  b) (p1  x)))))");
            // ParseEvalAndCompare("(entails (list (forall x (exists y (and (P x) (R x y)))) ) (list )  )");
            ParseEvalAndCompare("(entails (list (p1  a)) (list (exists  x (p1  b))))");
            ParseEvalAndCompare("(entails (list (p1  a)) (list (exists  x (and (p1  b) (p1  c)))))");
            ParseEvalAndCompare("(entails (list (p1  a)) (list (exists  x (implies (p1  a) (p1  b)))))");
            ParseEvalAndCompare("(entails (list (p1  a)) (list (exists  x (iff (p1  a) (p1  b)))))");
            ParseEvalAndCompare("(entails(list (p1  a)) (list (exists x (not (p1  a)))))");
            ParseEvalAndCompare("(entails (list (p1  a)) (list (implies (exists x (p1  a)) (p1  b))))");
            ParseEvalAndCompare("(entails (list a) (list (iff b c)))");
            ParseEvalAndCompare("(entails (list a) (list (iff (implies b c) (implies d e))))");
            ParseEvalAndCompare("(entails (list a) (list (and (implies b c) (implies d e))))");
            ParseEvalAndCompare("(entails (list (forall x (A x)) (forall x (forall y (B x y))))  (list (forall x (forall y (and (A x) (B x y)))) ))");
            ParseEvalAndCompare("(entails (list (H a) (forall x (implies (H x) (M x)))) (list (M a)))");   // pg 172
            ParseEvalAndCompare("(entails (list (p1  a)) (list (iff (p1  a) (exists x (q1  x)))))");
        }

        [Test]
        public void EvalTestNew()
        {
            ParseEvalAndCompare(@"(entails (list (p1  a)) (list (and  (implies (p1  a) (exists x (q1  x)))
                                                                      (implies (exists x (q1  x)) (p1  a) )))");
            ParseEvalAndCompare("(entails (list (p1  a)) (list (iff (exists x (p2 a x)) (iff (p1 a) (p1 a)))))");
        }

        [Test]
        public void EvalSavedSequent()
        {
            LParse res = new LParse();
            Sko.AddSyms(res, new ExpressionEvaluatorGrammar());

            EvalIst ei = new EvalIst();
            Lsx lsxOld = res.lsxParse(GenTest.stOld);
            Lsx lsxTerms = lsxOld;
            int n = 0;
            while (lsxTerms != Lsm.lsmNil)
            {
                Lpr lprTerms = (Lpr)lsxTerms;
                ei.EvalAndCompare(lprTerms.lsxCar);
                lsxTerms = lprTerms.lsxCdr;
                n++;
            }

        }

        public static void EvalGenSequent(bool fMain, long nMax)
        {
            GenAndCompare gac = new GenAndCompare();
            gac.fShowReport = true;
            gac.fMain = fMain;

            SatTestFactory stf = new SatTestFactory(new Gid(3, false)); // , Gid.nTypical()));
            SatTestWorker stw = new SatTestWorker(0, stf);

            gac.Start();
            for (int i = 0; i < nMax; i++)
            {
                stw.DoStep();
                gac.Report(stw.lsxExpr, stw.lsxASko);
            }

#if false

            String stStats = String.Format("\n\ntaut_true={0} taut_false={1} pass={2}\n\n", ei.nTautologyTrue, ei.nTautologyFalse, ei.nMatched);
            if (fMain)
            {
                Console.WriteLine(stStats);
            }
            else
            {
                Debug.WriteLine(stStats);
            }
#endif

        }

        [Test]
        public void TestGenSequent()
        {
            EvalGenSequent(false, 10000);
        }
    }
}