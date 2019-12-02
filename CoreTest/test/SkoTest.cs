using GrammarDLL;
using GraphMatching;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace reslab.test
{
    [TestFixture]
    [Category("Clause")]
    class ClauseTest
    {
        private bool fSame(Lsx lsxA, Lsx lsxB)
        {
            Sus susBase = Sus.susNone;
            return Scm.scmExpr.fSame(lsxA, lsxB, ref susBase);
        }

        [Test]
        public void SkoEqualTest()
        {
            LParse lparse = new LParse();
            Sko.AddSyms(lparse, new ExpressionEvaluatorGrammar());
            Lsx lsxA = lparse.lsxParse("(entails (list (or (and A B C) (and D E))) (list false))");
            Lsx lsxB1 = lparse.lsxParse("(entails (list (or (and A B C) (and D E))) (list F))");
            Assert.IsFalse(fSame(lsxA, lsxB1));
            Lsx lsxB = lparse.lsxParse("(entails (list (or (and A B C) (and D E))) (list false))");
            Assert.IsTrue(fSame(lsxA, lsxB));
            Assert.IsFalse(fSame(lsxB1, lsxA));
            Lsx lsxB2 = lparse.lsxParse("(entails (list (or (and A C B) (and D E))) (list false))");
            Assert.IsTrue(fSame(lsxA, lsxB2));
            Assert.IsTrue(fSame(lsxB2, lsxA));
            Lsx lsxB3 = lparse.lsxParse("(entails (list (or (and D E) (and A C B) )) (list false))");
            Assert.IsTrue(fSame(lsxA, lsxB3));
            Assert.IsTrue(fSame(lsxB3, lsxA));
            Lsx lsxB4 = lparse.lsxParse("(entails (list (or (and D E) (and D E) (and A C B) )) (list false))");
            Assert.IsTrue(fSame(lsxA, lsxB4));
            Assert.IsTrue(fSame(lsxB4, lsxA));
        }

#if false
    would need to do backtracking
        [Test]
        public void SkoEqualTestNew()
        {
            LParse res = new LParse();
            //string st1 = "( ( ((P x v w)   (P y z v)   (P x y u))   (P u z w)) )";
//string st2 = "((((P x y u) (P y z v) (P x v w))  (P u z w)) )";

            string st1 = @"
((((P X1 V1 W1)
  (P  Y1 Z1 V1) 
  (P  X1 Y1 U1))
  (P  U1 Z1 W1))
 (((P  U2 Z2 W2) (P  Y2 Z2 V2) (P  X2 Y2 U2))
  (P  X2 V2 W2))
 (nil 
    (P  (Q_12  S R) R S))
 (nil 
    (P  A (Q_16  B A) B))
 (((P  (Q_18  T) T (Q_18  T)))))";
            string st2 = @"
((((P x1 y1 u1) (P y1 z1 v1) (P x1 v1 w1))
  (P u1 z1 w1))
 (((P x2 y2 u2)(P y2 z2 v2)(P u2 z2 w2))
  (P x2 v2 w2))
 (nil
    (P(Q_g1 s r) r s))
 (nil
    (P a(Q_h2 b a) b))
 (((P(Q_k3 t) t(Q_k3 t)))))";

            Lsx lsxA = res.lsxParse(st1);
            Lsx lsxB = res.lsxParse(st2);
            Sus susBase = Sus.susRoot;
            Assert.IsTrue(Scm.scmSequent.fSame(lsxA, lsxB, ref susBase));
        }
#endif

        void CheckPrenex(string stL, string stR, string stB)
        {
            LParse res = new LParse();
            Sko.AddSyms(res, new ExpressionEvaluatorGrammar());

            Lsx lsxL = res.lsxParse("(list " + stL + ")");
            Lsx lsxR = res.lsxParse("(list " + stR + ")");
            Lsx lsxB = res.lsxParse(stB);
            Sus susVarList;
            int nNumNotNegated;
            Lsx lsxASko = new Sko().lsxClausalTransform(lsxL, lsxR, out susVarList, out nNumNotNegated);
            Assert.IsTrue(Scm.scmSequent.fSame(lsxASko, lsxB));
        }
        void CheckPrenex2(string stL, string stR, string stB)
        {
            LParse res = new LParse();
            Sko.AddSyms(res, new ExpressionEvaluatorGrammar());

            Lsx lsxL = res.lsxParse("(list " + stL + ")");
            Lsx lsxR = res.lsxParse("(list " + stR + ")");
            Lsx lsxB = res.lsxParse("(list " + stB + ")");
            Sus susVarList;
            int nNumNotNegated;
            Lsx lsxASko = new Sko().lsxClausalTransform(lsxL, lsxR, out susVarList, out nNumNotNegated);
            Sus susBase = Sus.susRoot;
            Assert.IsTrue(Scm.scmExpr.fSame(new Lpr(Lsm.lsmList, lsxASko), lsxB, ref susBase));
        }

        [Test]
        public void PrenexSamplesNew()
        {
            CheckPrenex("", "(and)", "false");
            CheckPrenex("", "(or a b false c)", "(((a)) ((b)) ((c)))");
            CheckPrenex("a", "(not b)", "((nil a)(nil b))");
            CheckPrenex("(exists x (and a b))", "", "((nil a) (nil b))");
            /*
            CheckPrenex("", "(exists x (and a b))", "(forall x (or (not a) (not b)))");
            CheckPrenex("(forall x true)", "", "(forall x true)");
            CheckPrenex2("(forall x (iff (P x) (exists y (and (R x y) (forall z (R z y))))", "", @"
(forall  x
    (forall  v
       (exists  y
          (forall  z
             (exists  w
                (and 
                   (or  (R  x y)
                      (not  (P  x)))
                   (or  (R  z y)
                      (not  (P  x)))
                   (or  (P  x)
                      (not  (R  x v))
                      (not  (R  w v)))))))))
");  // pg 154
*/
        }

        [Test]
        public void PrenexSamples()
        {

            CheckPrenex("(or a b false c)", "", "((nil a b c))");
            CheckPrenex("(and)", "", "true");
            CheckPrenex("(or)", "", "false");
            CheckPrenex("(and a b false c)", "", "false");
            CheckPrenex("(and a b true c)", "", "((nil a) (nil b)(nil c))");
            CheckPrenex("(or a true b c)", "", "true");
            CheckPrenex("(and a b true c)", "", "((nil a) (nil b) (nil c))");
            CheckPrenex("(not (not true))", "", "true");
            CheckPrenex("(not false)", "", "true");
            CheckPrenex("(not true)", "", "false");
            CheckPrenex("(or a (not a))", "", "true");
            CheckPrenex("(and a (not b))", "", "(((b)) (nil a))");
            CheckPrenex("(or a (and b (not a)))", "", "((nil a b))");
            CheckPrenex("(or (not a) (and b a))", "", "(((a) b)");
            CheckPrenex("(or a (and b c))", "", "((nil a b) (nil a c))");
            CheckPrenex("(or (and a b c) (and d e))", "", "((nil a d) (nil a e) (nil b d) (nil b e) (nil c d) (nil c e))");
            CheckPrenex("(or (and a d) (and b c))", "", "((nil b a) (nil b d) (nil c a) (nil c d))");
            CheckPrenex("(and a b a)", "", "((nil a) (nil b))");
            CheckPrenex("(or a b a)", "", "((nil a b))");
            CheckPrenex("(or a (and a b c))", "", "((nil a))");
            CheckPrenex("(not (or (and a (not b)) (and (not c) d)))", "", "(((a) b) ((d) c))");
            CheckPrenex("(and a (not a))", "", "false");
            CheckPrenex("(or (and a b) (and (not (and a b)) (not (or c d))))", "", "(((b)) ((a))  ((c)a)  ((c)b)  ((d)a) ((d)b))");
            CheckPrenex("(implies a b)", "", "(((a) b))");
            CheckPrenex("(iff a b)", "", "(((b) a) ((a) b))");
            CheckPrenex("(or (and a b) (not (and a b)) )", "", "(((b)) ((a)))");
            CheckPrenex("(or (and a b) (and (not (and a b)) c))", "", "(((b)) ((a)) (nil  c a)(nil  c b))");
            CheckPrenex("(or (and a b) (and (not (and a b)) (not (or c d))))", "", "(((b)) ((a))  ((c)a)  ((c)b)  ((d)a) ((d)b))");
            CheckPrenex("(or (and (and a b) (or c d)) (and (not (and a b)) (not (or c d))))", "", "(((b)) ((a)) ((c)a) ((c)b) (nil  d) ((d)a) ((d)b) (nil  c))");
            CheckPrenex("(iff (and a b) (or c d))", "", "(((b  a) d c) ((c)a) ((d)a) ((c)b) ((d)b))");
        }

        void CheckClausal(string stL, string stR, string stB)
        {
            LParse res = new LParse();
            Sko.AddSyms(res, new ExpressionEvaluatorGrammar());
            SatTest.UsualVariables(res);

            Lsx lsxL = res.lsxParse("(list " + stL + ")");
            Lsx lsxR = res.lsxParse("(list " + stR + ")");
            Lsx lsxB = res.lsxParse(stB);
            Sus susList;
            int nNumNotNegated;
            Lsx lsxASko = new Sko().lsxClausalTransform(lsxL, lsxR, out susList, out nNumNotNegated);
            Console.WriteLine(lsxASko);
#if false
            Sus susFull = Sus.susRoot;
            int i = 0;
            while (sqi != null)
            {
                Sus susHere = sqi.susLatest;
                while (susHere != Sus.susRoot)
                {
                    Lsm lsm = res.lsmGet("x" + i);
                    if (lsm != null)
                        susFull = new Sus(susFull, lsm, susHere.lsxReplace);
                    susHere = susHere.susPrev;
                }
                sqi = sqi.sqiPrev;
            }
#endif
            Sus susBase = Sus.susRoot;
            Assert.IsTrue(Scm.scmSequent.fSame(lsxASko, lsxB, ref susBase));
        }

        [Test]
        public void ClausalSamplesNew()
        {


            CheckClausal(@"
(forall x (forall y (P x y)))
(forall x (forall y (exists z (implies (and (P y z) (P z z)) (Q x y)))))", @"
(exists x (exists y (forall z (and (P y z) (P z z) (Q x z)(Q z z)))))
", @"
((nil  (P  x Y))
 (((P  y ($Q_z  y x))
   (P  ($Q_z  y x) ($Q_z  y x)))
  (Q  x y))
 (((P  y ($Q_z  y x))
   (P  ($Q_z  y x) ($Q_z  y x))
   (Q  x ($Q_z  y x))
   (Q  ($Q_z  y x) ($Q_z  y x)))))
");
            /*
            CheckClausal(stEx3L, stEx3R, @"
((((P  A_0 A_1 A_2) (P  A_3 A_4 A_1) (P  A_0 A_3 A_5))
  (P  A_5 A_4 A_2))
 (((P  A_6 A_7 A_8) (P  A_9 A_7 A_10) (P  A_11 A_9 A_6))
  (P  A_11 A_10 A_8))
 (nil  (P  E A_12 A_12))
 (nil  (P  A_13 E A_13))
 (nil  (P  A_14 A_14 E))
 (nil 
    (P  Q_15 Q_16 Q_17))
 (((P  Q_16 Q_15 Q_17))))
");   // ex 3
*/
        }

        public const string stEx2L = @"
(forall x (forall y (forall z (forall u (forall v (forall w (implies (and (P x y u) (P y z v) (P x v w)) (P u z w))))))))
(forall x (forall y (forall z (forall u (forall v (forall w (implies (and (P x y u) (P y z v) (P u z w)) (P x v w))))))))
(forall r (forall s (exists g (P g r s)))) 
(forall a (forall b (exists h (P a h b)))) 
";
        public const string stEx2R = @"(exists t (forall k (P k t k)))";

        public const string stEx3L = @"
(forall x (forall y (forall z (forall u (forall v (forall w (implies (and (P x y u) (P y z v) (P x v w)) (P u z w))))))))
(forall x (forall y (forall z (forall u (forall v (forall w (implies (and (P x y u) (P y z v) (P u z w)) (P x v w))))))))
(forall x (P E x x)) 
(forall x (P x E x)) 
(forall x (P x x E))";
        public const string stEx3R = @" 
(forall a (forall b (forall c (implies (P a b c) (P b a c))))) 
";

        [Test]
        public void ClausalSamples()
        {
            CheckClausal("(forall x (iff (P x) (exists y (and (R x y) (forall z (R z y))))", "",
@"((((P  x))
  (R  x ($Q_1 x)))
 (((P  x))
  (R  z ($Q_1 x)))
 (((R  ($Q_3 y) y) 
   (R  x y))
  (P  x)))");
            CheckClausal(stEx2L, stEx2R, @"
((((P  x y u)
   (P  y z v) 
   (P x v w))
  (P  u z w))
 (((P  x y u)
   (P  y z v)
   (P  u z w))
  (P  x v w))
 (nil 
    (P  ($Q_22  s r) r s))
 (nil 
    (P  X ($Q_16  Y X) Y))
 (((P  ($Q_18  Z) Z ($Q_18  Z)))))
");   // ex 2, pg 222


            CheckClausal("(exists x (and a x))", "", "((nil a) (nil y))");
            CheckClausal("(exists x (and a b))", "", "((nil a) (nil b))");
            CheckClausal(@"
(forall x (forall y (or (not (P x y )) (Q x) (not (Q y)))))
(forall x (not (P x x)))", @"
(exists x (exists y (and (not (P x y)) (Q x) (not (Q y))))",  // pg 170
                @"((((P x y) (Q y))  (Q x))    (((P x x)))     (((Q x))   (P x y) (Q y)))");
        }

        void CheckParsed(string stLatex)
        {
            REPL repl = new REPL();
            repl.Prepare();
            Lsx lsxA = repl.lsxParse(stLatex);
            Console.WriteLine("=> " + lsxA.ToString());
        }

        [Test]
        public void CheckParsedSamples()
        {
            CheckParsed("P()");
            CheckParsed("P(a)");
            CheckParsed("P(a,b)");
            CheckParsed(@"P(a,b,c) \\ \rightarrow Q(x,y)  ");
        }

        [Test]
        public void ReslabParse()
        {
            LParse res = new LParse();
            Lsx lsxAB = res.lsxParse("(1 . 2)");
            Assert.AreEqual(lsxAB, new Lpr(new Li(1), new Li(2)));
            Lsx lsx123 = res.lsxParse("(1 2 . 3)");
            Assert.AreEqual(lsx123, new Lpr(new Li(1), new Lpr(new Li(2), new Li(3))));
            Lsx lsx12L3 = res.lsxParse("((1 2) . 3)");
            Assert.AreEqual(lsx12L3, new Lpr(new Lpr(new Li(1), new Lpr(new Li(2), Lsm.lsmNil)),
                                            new Li(3)));
            Lsx lsxEx1 = res.lsxParse(@"
  ( ((P %Y (F %X %Y))
     (P (F %X %Y) (F %X %Y)))
    .
    ((Q %X %Y))
  )");
            //Console.WriteLine(lsxEx1.stPPrint());
        }
    }

    public class Eci
    {
        public string stLabel;
        public Lsx lsxClause;
        public bool fFound = false;

        public override int GetHashCode()
        {
            return lsxClause.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Eci))
                return false;
            Eci eci = (Eci)obj;
            return lsxClause.Equals(eci.lsxClause);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (stLabel !=  null)
            {
                sb.Append(stLabel);
                sb.Append(": ");
            }
            sb.Append(lsxClause);
            if (fFound)
                sb.Append(" found");
            return sb.ToString();
        }
    }


    public class CheckClauses : LogRes
    {
        HashSet<Eci> rgeciExpected = new HashSet<Eci>();

        Res res;
        public REPL repl;
        public LParse lparse;

        public CheckClauses()
        {
            lparse = new LParse();
        }

        public void Expect(string stLabel, string stClause = null)
        {
            if (stClause == null)
            {
                stClause = stLabel;
                stLabel = null;
            }
            Eci eci = new Eci();
            eci.stLabel = stLabel;
            eci.lsxClause = lparse.lsxParse(stClause);
            rgeciExpected.Add(eci);
        }

        public void SetRes(Res res)
        {
            this.res = res;
            // Sko.AddSyms(lparse, repl.ee.Grammar);
        }

        public override void AscCreated(Asc asc, Shv shv)
        {
            //  base.AscAdded(asc);
            Lsx lsxAsc = asc.lsxTo(res.asyDefault);
            int nPos = 0;
            foreach (Eci eci in rgeciExpected)
            {
                Sus susBase = Sus.susVbl; //  Root;
                if (Scm.scmQuad.fSame(eci.lsxClause, lsxAsc, ref susBase))
                {
                    if (!eci.fFound)
                    {
                        string stLine = "found ";
                        stLine += asc.nId + " < ";
                        if (eci.stLabel != null)
                            stLine += eci.stLabel + " --- ";
                        WriteLine(stLine + lsxAsc.stPPrint(Pctl.pctlPlain));
                        eci.fFound = true;
                    }
                    break;
                }
                nPos++;
            }
        }

        public void CheckAllFound()
        {
            //bool fFound = false;
            foreach (Eci eci in rgeciExpected)
            {
                if (eci.fFound)
                    continue;
                string stLine = "not found ";
                if (eci.stLabel != null)
                    stLine += eci.stLabel + " --- ";
                WriteLine(stLine + eci.lsxClause.stPPrint(Pctl.pctlPlain));
                //  fFound = true;
            }
            // Assert.IsFalse(fFound);
        }
    }

    [TestFixture]
    [Category("Proof Examples")]
    class SkoTest
    {

        void ProveClausal(string stL, string stR)
        {
            string stA = "(entails (list " + stL + ") (list " + stR + "))";
            LParse pres = new LParse();
            Sko.AddSyms(pres, new ExpressionEvaluatorGrammar());

            Lsx lsxA = pres.lsxParse(stA);
            Res res = new Res(); //  irr: new LogRes()); 
            ascProveClausal(lsxA, res);
        }

        public static Lsx lsxParse(string stLatex)
        {
            REPL repl = new REPL();
            repl.Prepare();

            return repl.lsxParse(stLatex);
        }

        public static void ProveParsed (string stLatex, Pmb imd = null, Irr irr = null, Res res = null)
        {
            Lsx lsxA = lsxParse(stLatex);
            if (irr == null)
                irr = new LogRes();
            if (res == null)
                res = new Res(imd: imd, irr: irr);
            // res.fVerifyEachStep = false;
            // res.fUseNegatedGoals = false;
            ascProveClausal(lsxA, res);
        }

        public static Asc ascProveClausal(Lsx lsxA, Res res, LParse lparse = null,
                                    bool fAlwaysTimeout = false)
        {
            Sus susHere;
            Sko sko = new Sko();
            int nNumNotNegated;
            Lsx lsxASko = sko.lsxClausalTransform(lsxA, out susHere, out nNumNotNegated, false, lparse);
            //Console.WriteLine(lsxASko);

            while (susHere != Sus.susRoot)
            {
                susHere.lsmSymbol.MakeVariable();
                susHere = susHere.susPrev;
            }

            // res.fValidateEub = false;   // need to fix
            // res.fVerifyEachStep = true;

            Asc ascProof = res.ascProve(lsxASko, nNumNotNegated);
            if (!fAlwaysTimeout)
                Assert.IsNotNull(ascProof);
            return ascProof;
        }
        [Test]
        public void ProveSkoSamples()
        {
            ProveClausal(ClauseTest.stEx3L, ClauseTest.stEx3R);

        }

        static string stReslab2 = @"
\forall x \forall y \forall z \forall u \forall v \forall w 
                (P(x, y, u) \land P(y, z, v) \land P(x, v, w) \rightarrow P(u, z, w)),
\forall x \forall y \forall z \forall u \forall v \forall w
                (P(x, y, u) \land P(y, z, v) \land P(u, z, w) \rightarrow P(x, v, w)),
\forall r \forall s \exists g P( g, r, s),
\forall a \forall b \exists h P( a, h, b)
\Rightarrow
\exists t \forall k P(k, t, k)
";

        [Test]
        public void ProveParsedSamples2()
        {
            // ideas?
            //    add to AscResolveNew, with multiple-term unification needed instead of just term pairs
            ProveParsed(stReslab2);
        }

        [Test]
        public void ProveSkoSamplesNew()
        {
            ProveClausal(@"
(forall x (forall y (P x y)))
(forall x (forall y (exists z (implies (and (P y z) (P z z)) (Q x y)))))", @"
(exists x (exists y (forall z (and (P y z) (P z z) (Q x z) (Q z z)))))");
            ProveClausal(ClauseTest.stEx2L, ClauseTest.stEx2R);
        }

#if false
        [Test]
        public void ProveParsedSamples2_Trace()
        {
            CheckClauses cc = new CheckClauses();
            cc.repl = new REPL();
            cc.repl.Prepare();
            SatTest.UsualVariables(cc.lparse);

            Res res = new Res(irr: cc); // , imd: Pss.imdSetOfSupport);
            res.fDoParamodulation = true;

            Lsx lsxA = cc.repl.lsxParse(// stEquivalence + 
                                        // stEx8Equality + 
                                        stReslab2, lparse: cc.lparse);
            cc.SetRes(res);

            cc.lparse.lsmObtain("Q_16").MakeSkolemFunction();
            cc.lparse.lsmObtain("Q_12").MakeSkolemFunction();

            // these are not applicable because they are from axioms given in a different order.
            cc.Expect("(nil  (P  @0 (Q_16  @1 @0) @1))");
            cc.Expect("(((P  @0 @1 @2) (P  @1 @3 @4) (P  @0 @4 @5)) (P  @2 @3 @5))");
            cc.Expect("(((P  (Q_16  @0 @1) @2 @0)) (P  @0 @1 @2))");
            cc.Expect("(nil  (P  @0 @1 (Q_16  @0 (Q_16  @0 @1))))");
            cc.Expect("(nil  (P  (Q_12  @0 @1) @1 @0))");
            cc.Expect("(((P  @0 @1 @2) (P  @1 @3 @4) (P  @2 @3 @5)) (P  @0 @4 @5))");
            cc.Expect("(((P  @0 @0 @1)) (P  (Q_12  @0 @1) @0 @2))");
            cc.Expect("(nil  (P  (Q_12  @0 (Q_16  @0 (Q_16  @0 @0))) @0 @0))");
            cc.Expect("(((P  @0 @0 @1) (P  (Q_12  @0 @1) @0 @2)) (P  @0 @1 @2))");
            cc.Expect("(((P  (Q_12  @0 (Q_16  @0 (Q_16  @0 @0))) @0 @0)) (P  @0 (Q_16  @0 (Q_16  @0 @0)) @0))");
            cc.Expect("(nil  (P  @0 (Q_16  @0 (Q_16  @0 @0)) @0))");
            cc.Expect("(((P  (Q_16  @0 (Q_16  @0 @0)) @0 @1)) (P  @0 @0 @1))");
            cc.Expect("(nil  (P  @0 @0 (Q_16  (Q_16  @0 (Q_16  @0 @0)) (Q_16  (Q_16  @0 (Q_16  @0 @0)) @0))))");
            cc.Expect("(nil  (P  (Q_16  (Q_16  @0 (Q_16  @0 @0)) (Q_16  (Q_16  @0 (Q_16  @0 @0)) @0)) @1 @2))");
            cc.Expect("(nil  (P  (Q_16  @0 (Q_16  @0 @0)) (Q_16  (Q_16  @0 (Q_16  @0 @0)) @0) @1))");
            cc.Expect("(nil  (P  @0 (Q_16  @0 @0) (Q_16  (Q_16  @0 (Q_16  @0 @0)) @0)))");
            cc.Expect("(((P  (Q_16  @0 @0) @1 @2)) (P  (Q_16  (Q_16  @0 (Q_16  @0 @0)) @0) @1 @2))");
            cc.Expect("(nil  (P  (Q_16  (Q_16  @0 (Q_16  @0 @0)) @0) (Q_16  @1 (Q_16  @0 @0)) @1))");
            cc.Expect("(nil  (P  (Q_16  @0 (Q_16  @0 @0)) @0 (Q_16  (Q_16  @0 (Q_16  @0 @0)) (Q_16  @0 @0))))");
            cc.Expect("(nil  (P  @0 @0 (Q_16  (Q_16  @0 (Q_16  @0 @0)) (Q_16  @0 @0))))");
            cc.Expect("(nil  (P  (Q_16  (Q_16  @0 (Q_16  @0 @0)) (Q_16  @0 @0)) @1 @2))");
            cc.Expect("(nil  (P  (Q_16  @0 (Q_16  @0 @0)) (Q_16  @0 @0) @1))");
            cc.Expect("(nil  (P  @0 (Q_16  @0 @0) (Q_16  @0 @0)))");
            cc.Expect("(((P  (Q_16  @0 @0) @1 @2)) (P  @0 @1 @2))");
            cc.Expect("(nil  (P  @0 (Q_16  @1 (Q_16  @0 @0)) @1))");
            cc.Expect("(((P  (Q_16  @0 (Q_16  @1 @1)) @2 @0)) (P  @0 @1 @2))");
            cc.Expect("(nil  (P  (Q_16  @0 (Q_16  @0 @0)) @0 @1))");
            cc.Expect("(nil  (P  @0 @0 @1))");
            cc.Expect("(nil  (P  (Q_12  @0 @1) @0 @0))");
            cc.Expect("(((P  (Q_12  @0 @1) @0 @0)) (P  @0 @1 @0))");
            cc.Expect("(nil  (P  @0 @1 @0))");
            cc.Expect("(((P  (Q_18  @0) @0 (Q_18  @0))))");


            Asc ascProof = SkoTest.ascProveClausal(lsxA, res);
            // res.irr.Proved(ascProof);
            cc.CheckAllFound();
        }
#endif

        [Test]
        public void ProveParsedSamples()
        {


            ProveParsed(@"
\forall x \forall y \forall z \forall u \forall v \forall w 
                (P(x, v, w) \land P(y, z, v) \land P(x, y, u) \rightarrow P(u, z, w)),
\forall x \forall y \forall z \forall u \forall v \forall w
                (P(u, z, w) \land P(y, z, v) \land P(x, y, u) \rightarrow P(x, v, w)),
\forall r \forall s \exists g P( g, r, s),
\forall a \forall b \exists h P( a, h, b)
\Rightarrow
\exists t \forall k P(k, t, k)
");


            ProveParsed(@"
\forall x \forall y \forall z \forall u \forall v \forall w (P(x, y, u) \land P(y, z, v) \land P(x, v, w) \rightarrow P(u, z, w)), \\
\forall x \forall y \forall z \forall u \forall v \forall w (P(x, y, u) \land P(y, z, v) \land P(u, z, w) \rightarrow P(x, v, w)), \\
\forall x P(E, x, x), \\
\forall x P(x, E, x), \\
\forall x P(x, x, E) \\
\Rightarrow
\forall a \forall b \forall c (P(a, b, c) \rightarrow P(b, a, c))
");

            ProveParsed(@"
\forall x \forall y P(x,y),  
\forall x \forall y \exists z ( P(y,z) \land P(z,z) \rightarrow Q(x,y) )  
\Rightarrow
\exists x \exists y \forall z ( P(y,z) \land P(z,z) \land Q(x,z) \land Q(z,z) )");
        }
    }
}
