
using NUnit.Framework;
using System.Collections.Generic;
using GraphMatching;
using static reslab.test.PuzzleTest;
using System;

namespace reslab.test
{


    public class Ex2
    {
        public static string stEx2_1 = "(() (= (F E @0) @0) ) ";
        public static string stEx2_2 = "(() (= (F @0 E) @0) ) ";
        public static string stEx2_3 = "(() (= (F @0 (F @1 @2)) (F (F @0 @1) @2)) ) ";
        public static string stEx2_4 = "(() (= (F @0 @0) E) )";
        public static string stEx2_5 = "(() (= C (F A B)) ) ";
        public static string stEx2_6 = "(( (= C (F B A)) )) ";
        public static string st_R = "(( (= X Y) ) (= Y X) )";
        public static string stEx2Axioms = stEx2_1 + stEx2_2 + stEx2_3 + stEx2_4 + stEx2_5 + st_R;
        public static string stEx2_9 = "(() (= A (F C B)) ) ";
        public static string stEx2_12 = "(() (= (F C A) B) ) ";

        public static string stEx2 = "(" + stEx2Axioms + stEx2_6 + ")";
        public static string stEx2_plus9 = "(" + stEx2Axioms + stEx2_9 + stEx2_6 + ")";
        public static string stEx2_plus12 = "(" + stEx2Axioms + stEx2_12 + stEx2_6 + ")";
        public static string stEx2StepsList = "(" + stEx2Axioms + @" 
                          (() (= (F @0 E) (F (F @0 @1) @1)) ) 
                          (() (= @0 (F (F @0 @1) @1)) ) 
                          (() (= A (F C B)) )
                          (() (= (F @0 (F @0 @1)) (F E @1)) ) 
                          (() (= (F @0 (F @0 @1)) @1) ) 
                          (() (= (F C A) B) )
                          (() (= C (F B A)) )"
                      // look for intermediate steps that use negated terms
                      + @"
                          ( ( (= (F B A) (F (F C @1) @1)) ) )
                     )";
        public static string stEx2_plusPhases = "(" + stEx2Axioms + stEx2_9  + stEx2_6
            //+ stEx2_12   - adding this axiom causes pti to be used even though the asc is not used
            + ")";
    }
   
    [TestFixture]
    [Category("Proof Examples")]
    class EqExamples
    {
        // See also example pg 248 in Harrison

        public const string stReflexive = @"
\forall x x=x,";

        public const string stSymmetric = @"
\forall x \forall y (x=y \rightarrow y=x),";

        public const string stTransitive = @"
\forall x \forall y \forall z (x=y \land y=z \rightarrow x=z),";

        public const string stEquivalence = stReflexive + stSymmetric + stTransitive;

        const string stEx1Equality = stEquivalence + @"
\forall x \forall y (x=y \rightarrow (Q(x) \rightarrow Q(y))),";

        const string stEx1 = @"
a=b,
Q(a)
\Rightarrow
Q(b)
";
        const string stEx2 = @"
a=b,
\forall x Q(x)
\Rightarrow
Q(b)
";
        const string stEx3Equality = stEquivalence + @"
\forall x \forall y (x=y \rightarrow (Q(x) \rightarrow Q(y))),
\forall x \forall y (x=y \rightarrow (P(x) \rightarrow P(y))),";

        const string stEx3 = @"
a=b,
\forall x (Q(x) \lor P(x))
\Rightarrow
(Q(b) \lor P(a))
";
        const string stEx4 = @"
a=b,
\forall x (Q(x) \lor P(x))
\Rightarrow
(Q(a) \lor P(b))
";

        const string stEx5Equality = stEquivalence + @"
\forall x \forall y (x=y \rightarrow (Q(x) \rightarrow Q(y))),
\forall x \forall y (x=y \rightarrow g(x) = g(y)),
\forall x \forall y (x=y \rightarrow h(x) = h(y)),";

        const string stEx5 = stEx5Equality + @"
\forall x x = h(x),
\forall y Q(g(y))
\Rightarrow
\forall y Q(h(g(y)))
";

        const string stEx6Equality = stEquivalence + @"
\forall x \forall y (x=y \rightarrow (Q(x) \rightarrow Q(y))),
\forall x \forall y (x=y \rightarrow f(x) = f(y)),
\forall x \forall y (x=y \rightarrow g(x) = g(y)),
\forall x \forall y (x=y \rightarrow h(x) = h(y)),
\forall x \forall y (x=y \rightarrow j(x) = j(y)),";

        const string stEx6 = @"
a=b,
Q(f(g(h(j(a)))))
\Rightarrow
Q(f(g(h(j(b)))))
";

        const string stEx7Equality = stEquivalence + @"
\forall x \forall y (x=y \rightarrow g(x) = g(y)),
\forall x \forall y \forall z (x=y \rightarrow f(x,z) = f(y,z)),
\forall x \forall y \forall z (x=y \rightarrow f(z,x) = f(z,y)),
\forall x \forall y \forall z \forall w (x=y \rightarrow (P3(x,z,w) \rightarrow P3(y,z,w))),
\forall x \forall y \forall z \forall w (x=y \rightarrow (P3(z,x,w) \rightarrow P3(z,y,w))),
\forall x \forall y \forall z \forall w (x=y \rightarrow (P3(w,z,x) \rightarrow P3(w,z,y))),";

        const string stEx7 = @"
\forall x f(x,g(x)) = e,
\forall y \forall z P3(y,f(g(y),z),z)
\Rightarrow
\forall y P3(y,e,g(g(y)))
";

        public const string stEx8Equality = @"
\forall x \forall y \forall z (x=y \rightarrow f(x,z) = f(y,z)),
\forall x \forall y \forall z (x=y \rightarrow f(z,x) = f(z,y)),";

        public const string stEx8 = @"
\forall x f(e,x) = x,
\forall x f(x,e) = x,
\forall x \forall y \forall z f(x,f(y,z)) = f(f(x,y),z),
\forall x f(x,x) = e,
f(a,b) = c
\Rightarrow
c = f(b,a)
";
        public const string stEx9Equality = @"
\forall x \forall y \forall z (x=y \rightarrow f(x,z) = f(y,z)),
\forall x \forall y \forall z (x=y \rightarrow f(z,x) = f(z,y)),
\forall x \forall y (x=y \rightarrow g(x) = g(y)),
\forall x \forall y \forall z (x=y \rightarrow h(x,z) = h(y,z)),
\forall x \forall y \forall z (x=y \rightarrow h(z,x) = h(z,y)),";

        public const string stEx9 = @"
\forall x f(e,x)=x,
\forall x f(x,e)=x,
\forall x f(g(x),x) = e,
\forall x f(x,g(x)) = e,
\forall x \forall y \forall z f(x,f(y,z)) = f(f(x,y),z),
\forall x f(f(x,x),x) = e,
\forall x \forall y h(x,y) = f(f(f(x,y),g(x)),g(y))
\Rightarrow
h(h(a,b),b) = e
";

        public static string[] rgstCases1 =
        {
            stEx3Equality + stEx3,
            stEx1Equality + stEx1,
            stEx1Equality + stEx2,
            stEx3Equality + stEx4,
            stEx5Equality + stEx5,
            stEx7Equality + stEx7
        };
        public static string[] rgstCases2 =
        {
            stEx6Equality + stEx6
        };
        string[] rgstCases3 = { stEquivalence + stEx8Equality + stEx8 };
        string[] rgstCases1Para = { stEx5, stEx1, stEx2, stEx3, stEx4, stEx6, stEx7 };
        string[] rgstCases3Para = { // stEquivalence + stEx8Equality +
                                       stEx8 };

        [Test]
        public void ProveF2()
        {
            LogRes logRes = new LogRes();
            string stF2New = @"
\forall x x=x,
\forall x \forall y (x=y \rightarrow y=x),
\forall x \forall y \forall z (x=y \land y=z \rightarrow x=z),
\forall x \forall y \forall z (x=y \rightarrow f(x,z) = f(y,z)),
\forall x \forall y \forall z (x=y \rightarrow f(z,x) = f(z,y))

\Rightarrow

\forall x1 \forall y1 \forall x2 \forall y2 (x1=y1 \land x2=y2 \rightarrow f(x1,x2) = f(y1,y2))
";
            SkoTest.ProveParsed(stF2New, irr: logRes); //, imd:Pss.imdSetOfSupport);
        }

        [Test]
        public void ProveEq1Para()
        {
            // return;
            LogRes logRes = new LogRes();
            foreach (string stEx in rgstCases1Para)
            {
                Lsx lsxA = SkoTest.lsxParse(stEx);
                Res res = new Res(imd: null, irr: logRes); //, imd:Pss.imdSetOfSupport);
                res.fUseNegatedGoals = false;
                res.fDoParamodulation = true;
                SkoTest.ascProveClausal(lsxA, res);
            }
        }

        // [Test]
        public void ProveEq3()
        {
            return;  // 23 seconds 10/30/16
            LogRes logRes = new LogRes();
            foreach (string stEx in rgstCases3)
            {
                Lsx lsxA = SkoTest.lsxParse(stEx);
                Res res = new Res(imd: null, irr: logRes); //, imd:Pss.imdSetOfSupport);
                res.fDoParamodulation = false;
                SkoTest.ascProveClausal(lsxA, res);
                // SkoTest.ProveParsed(stEx, irr: logRes); // , imd: Pss.imdSetOfSupport);
            }
        }

        // [Test]
        public void ProveEq3Para_Trace()
        {
            return;

            CheckClauses cc = new CheckClauses();
            cc.repl = new REPL();
            cc.repl.Prepare();
            SatTest.UsualVariables(cc.lparse);

            Res res = new Res(irr: cc); // , imd: Pss.imdSetOfSupport);
            res.fDoParamodulation = true;
            res.msTimeLimit = 120000;

            Lsx lsxA = cc.repl.lsxParse(// stEquivalence + 
                                        // stEx8Equality + 
                                        stEx8, lparse: cc.lparse);
            cc.SetRes(res);
            cc.Expect("(nil  (=  (f  @0 (f  @1 @2)) (f  (f  @0 @1) @2)))");
            cc.Expect("(nil  (=  (f  @0 @0) e))");
            cc.Expect("(nil  (=  (f  @0 e) (f  (f  @0 @1) @1)))");
            cc.Expect("(nil  (=  (f  @0 e) @0))");
            cc.Expect("(nil  (=  @0 (f  (f  @0 @1) @1)))");
            cc.Expect("(nil  (=  (f  @0 (f  @1 @2)) (f  (f  @0 @1) @2)))");
            cc.Expect("(nil  (=  (f  @0 @0) e))");
            cc.Expect("(nil  (=  (f  @0 e) (f  (f  @0 @1) @1)))");
            cc.Expect("(nil  (=  (f  @0 e) @0))");
            cc.Expect("(nil  (=  @0 (f  (f  @0 @1) @1)))");
            cc.Expect("(nil  (=  (f  @0 (f  @1 @2)) (f  (f  @0 @1) @2)))");
            cc.Expect("(nil  (=  (f  @0 @0) e))");
            cc.Expect("(nil  (=  e (f  (f  (f  @0 @1) @0) @1)))");
            cc.Expect("(nil  (=  (f  a b) c))");
            cc.Expect("(nil  (=  e (f  (f  c a) b)))");
            cc.Expect("(nil  (=  (f  c a) (f  e b)))");
            cc.Expect("(nil  (=  (f  e @0) @0))");
            cc.Expect("(nil  (=  (f  c a) b))");
            cc.Expect("(nil  (=  c (f  b a)))");
            cc.Expect("(((=  c (f  b a))))");


            Asc ascProof = SkoTest.ascProveClausal(lsxA, res, fAlwaysTimeout: true);
            // res.irr.Proved(ascProof);
            cc.CheckAllFound();
        }

       // [Test]// , Ignore("hangs")]
        public void CheckLos()
        {
            CheckClauses cc = new CheckClauses();
            cc.repl = new REPL();
            cc.repl.Prepare();
            // SatTest.UsualVariables(cc.lparse);

            Res res = new Res(irr: cc, fAlwaysTimeout: true);
                                        // , imd: Pss.imdSetOfSupport);
            res.fDoParamodulation = false;
            res.fValidateProof = false;
            res.fVerifyEachStep = false;
            res.msTimeLimit = 3 * 60 * 1000;

            Lsx lsxA = cc.repl.lsxParse(ResExamples.stLos, lparse: cc.lparse);
            cc.SetRes(res);
            cc.Expect("3<-,", "(nil  (P  @0 @1) (Q  @0 @1))");
            cc.Expect("2<-,", "(((Q  @0 @1)) (Q  @1 @0))");
            cc.Expect("126<-3,2", "(nil  (P  @0 @1) (Q  @1 @0))");
            cc.Expect("4<-,", "(((P  Q_10 Q_11)))");
            cc.Expect("9<-3,4", "(nil  (Q  Q_10 Q_11))");
            cc.Expect("1<-,", "(((Q  @0 @1) (Q  @1 @2)) (Q  @0 @2))");
            cc.Expect("38<-9,1", "(((Q  Q_11 @0)) (Q  Q_10 @0))");
            cc.Expect("336<-126,38", "(nil  (P  @0 Q_11) (Q  Q_10 @0))");
            cc.Expect("5<-,", "(((Q  Q_12 Q_13)))");
            cc.Expect("68<-3,5", "(nil  (P  Q_12 Q_13))");
            cc.Expect("0<-,", "(((P  @0 @1) (P  @1 @2)) (P  @0 @2))");
            cc.Expect("81<-68,0", "(((P  Q_13 @0)) (P  Q_12 @0))");
            cc.Expect("352<-336,81", "(nil  (Q  Q_10 Q_13) (P  Q_12 Q_11))");
            cc.Expect("124<-3,0", "(((P  @0 @1)) (Q  @2 @0) (P  @2 @1))");
            cc.Expect("526<-126,124", "(nil  (Q  @0 @1) (Q  @2 @1) (P  @2 @0))");
            cc.Expect("532<-526,38", "(nil  (Q  @0 @1) (P  @0 Q_11) (Q  Q_10 @1))");
            cc.Expect("536<-532,2", "(nil  (P  @0 Q_11) (Q  Q_10 @1) (Q  @1 @0))");
            cc.Expect("670<-536,4", "(nil  (Q  Q_10 @0) (Q  @0 Q_10))");
            cc.Expect("801<-670,2", "(nil  (Q  @0 Q_10))");
            cc.Expect("1245<-801,1", "(((Q  Q_10 @0)) (Q  @1 @0))");
            cc.Expect("1885<-352,1245", "(nil  (P  Q_12 Q_11) (Q  @0 Q_13))");
            cc.Expect("3153<-1885,5", "(nil  (P  Q_12 Q_11))");
            cc.Expect("148<-126,5", "(nil  (P  Q_13 Q_12))");
            cc.Expect("403<-148,124", "(nil  (Q  @0 Q_13) (P  @0 Q_12))");
            cc.Expect("3028<-403,1245", "(nil  (P  Q_10 Q_12) (Q  @0 Q_13))");
            cc.Expect("3219<-3028,5", "(nil  (P  Q_10 Q_12))");
            cc.Expect("7349<-3219,0", "(((P  Q_12 @0)) (P  Q_10 @0))");
            cc.Expect("7432<-3153,7349", "(nil  (P  Q_10 Q_11))"); Asc ascProof = SkoTest.ascProveClausal(lsxA, res, cc.lparse, fAlwaysTimeout: true);
            if (ascProof == null)
                cc.CheckAllFound();
        }

        // [Test]
        public void ProveEq4_Trace()
        {
            CheckClauses cc = new CheckClauses();
            cc.repl = new REPL();
            cc.repl.Prepare();
            SatTest.UsualVariables(cc.lparse);

            Res res = new Res(irr: cc); // , imd: Pss.imdSetOfSupport);
            res.fDoParamodulation = true;

            Lsx lsxA = cc.repl.lsxParse(stEquivalence + stEx9Equality + stEx9, lparse: cc.lparse);
            cc.SetRes(res);
#if true
            cc.Expect("16. f(f(xg(x))w) =f(ew)", "(nil   (= (f (f x (g x)) w) (f e w)))");
            cc.Expect("24. x=f(xe)", "(nil   (= x (f x e)))");
            cc.Expect("25.f(xe) != z x = z", "(( (= (f x e) z) )  (= x z)  )");
            cc.Expect("44. f(f(g(x)x)u) = f(eu), 3 and 9_1", "(nil (= (f (f (g x) x) u) (f e u)) )");

#endif
            cc.Expect("17. f(f(xy)z) !=w f(xf(yz))=w, 5 and 8_1", "( ( (= (f (f x y) z) w) ) (= (f x (f y z)) w ))");
            cc.Expect("18. f(xf(g(x)z))=f(ez), 16 and 17_1", "(nil (= (f x (f (g x) z)) (f e z)))");
            cc.Expect("26. x=g(g(x))", "(nil (= x (g (g x))))");
            cc.Expect("30. f(f(xx)e)=g(x), 29 and 221 ", "(nil (= (f (f x x) e) (g x)))");
            cc.Expect("31. f(xx)=g(x), 30 and 2_1 ", "(nil (= (f x x) (g x)) )");
            cc.Expect("51. g(g(x)) = x, 26 and 7_1", "(nil (= (g (g x)) x))");
            cc.Expect("52. f(g(g(u))z) = f(uz), 51 and 9_1", "(nil (= (f (g (g u)) z)  (f u z) ))");
            cc.Expect("47. g(f(xy)) = f(g(y)g(x)), 46 and 15_1", "(nil (= (g (f x y)) (f (g y) (g x)) ))");
            cc.Expect("55. f(zg(f(xy))) = f(zf(g(y)g(x))), 47 and 9_1", "(nil (= (f z (g (f x y)))  (f z (f (g y) (g x))) ))");
            cc.Expect("58. f(yf(g(g(u))z)) = f(yf(uz)), 52 and 10_1", "(nil (= (f y (f (g (g u)) z))  (f y (f u z))) )");
            cc.Expect("60. g(h(xy)) = f(yf(xg(f(xy)))), 57 and 59_1", "(nil (= (g (h x y)) (f y (f x (g (f x y))))))");
            cc.Expect("70. f(x(f(yz))!=u f(f(xy)z)=u 69 and 8_1 ", "(((= (f x (f y z)) u)) (= (f (f x y) z) u) )");
            cc.Expect("80. u!=f(xe) u= x, 2 and 8_2 ", "(((= u (f x e))) (= u x) )");
            cc.Expect("90. g(x) =f(xx), 31 and 71", "(nil (= (g x) (= (f x x))) )");
            cc.Expect("100. f(f(xf(yz))u) =f(f(f(xy)z)u), 5 and 91", "(nil (= (f (f x (f y z)) u)) (f (f (f x y) z) u) ) )");
            cc.Expect("110. f(f(f(f(f(f(f(ab)a)a)b)a)f(g(b)g(a)))g(b))!=y  y != e, 108 and 83",
                      "(( (= (f (f (f (f (f (f (f a b) a) a) b) a) (f (g b) ( g a))) (g b)) y) (= y e)  ))");
            cc.Expect("121. f(g(x)z) = f(f(xx)z), 90 and 91 ", "(nil (= (f (g x) z)  (f (f x x) z)))");
            cc.Expect("134. f(uf(xe)) =f(ux), 2 and 101 ", "(nil (= (f u (f x e)) (f u x)) )");
            Asc ascProof = SkoTest.ascProveClausal(lsxA, res);
            res.irr.Proved(ascProof);
            cc.CheckAllFound();
        }

        // [Test]
        public void ProveEq4()
        {
            return;
            LogRes logRes = new LogRes();
            SkoTest.ProveParsed(stEquivalence + stEx9Equality + stEx9, irr: logRes); // , imd: Pss.imdSetOfSupport);
        }

        // [Test]
        public void ProveEq4ParaHtml()
        {
            return;
            HtmlRes logRes = new HtmlRes(@"C:\Users\kerika\projects\graphmatch\ex9.html");
            Lsx lsxA = SkoTest.lsxParse(/*stEx9Equality +*/ stEx9);
            Res res = new Res(irr: logRes); // , imd: Pss.imdSetOfSupport);
            logRes.res = res;
            res.fDoParamodulation = true;
            Asc ascProof = SkoTest.ascProveClausal(lsxA, res);
            res.irr.Proved(ascProof);
        }

    }
    [TestFixture]
    [Category("Proof Examples Slow")]
    class EqExamplesSlow
    {

        [Test]
        [Ignore("wip")]
        public void ProveEq3Para()
        {
            // return;   // takes over 2 minutes
            LogRes logRes = new LogRes();
            string stEx = // EqExamples.stReflexive + EqExamples.stSymmetric +
                           EqExamples.stEquivalence + // EqExamples.stEx8Equality +    // runs out of memory with these on
                          EqExamples.stEx8;
            
            Lsx lsxA = SkoTest.lsxParse(stEx);
            Res res = new Res(irr: logRes); //, imd:Pss.imdSetOfSupport);
            res.fDoParamodulation = true;
            //res.fValidateEub = true;
            //res.fValidateEpu = true;
            res.fVerifyEachStep = false;
            res.msTimeLimit = 0;

            SkoTest.ascProveClausal(lsxA, res);
            
        }

        [Test]
        [Ignore("wip")]
        public void ProveEq3_with9()
        {
            ResTest.DoProof(Ex2.stEx2_plus9); // , imd: Pss.imdSetOfSupport);
        }

        [Test]
        public void ProveEq3_with12()
        {
            ResTest.DoProof(Ex2.stEx2_plus12); // , imd: Pss.imdSetOfSupport);
        }

        
        [Test]
        [Ignore("wip")]
        public void ProveEq4Para()
        {
            // return;
            LogRes logRes = new LogRes();
            Lsx lsxA = SkoTest.lsxParse(EqExamples.stEquivalence +
                                        // EqExamples.stEx9Equality + 
                                        EqExamples.stEx9);
            Res res = new Res(irr: logRes); // , imd:Pss.imdSetOfSupport);
            res.fDoParamodulation = true;
#if false
            res.opcOnDemandControl = OpcReduceLength.opcOnly;
            res.iehEquateHandler = Eqm.EquateUnify;
#endif
            Asc ascProof = SkoTest.ascProveClausal(lsxA, res);
            res.irr.Proved(ascProof);
        }


        [Test]
        [Ignore("broken")]

        public void SemanticRes()
        {
            LogRes logRes = new LogRes();
            Psm psm = new PsmSimple();
            SkoTest.ProveParsed(@"
\forall x (S1(x) \lor S2(x))
\Rightarrow
\forall y@qy (S1(y) \lor S2(y))
", imd: psm, irr: logRes);
        }

    }
}