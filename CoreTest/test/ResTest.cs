using GrammarDLL;
using NUnit.Framework;
using reslab.test;

namespace reslab
{
    [TestFixture]
    [Category("Proof Examples")]
    class ResTest
    {

        public static void DoProof(string stProblem, Res res = null, LParse pres = null)
        {
            if (res == null)
            {
                Irr irr = new LogRes();
                res = new Res(irr: irr);
                res.fUseNegatedGoals = false;
            }
            if (pres == null)
            {
                pres = new LParse();
                Sko.AddSyms(pres, new ExpressionEvaluatorGrammar());
                string stVars = "(%X %Y %Z %U %V %W %XX %YY %ZZ %UU %VV %WW %R %S %A %B %TT)";
                res.MakeVariables(pres.lsxParse(stVars));
            }
            Lsx lsxProblem = pres.lsxParse(stProblem);
            Asc ascProof = res.ascProve(lsxProblem, 1);
            Assert.IsNotNull(ascProof);

        }

        [Test]
        public void TinyProofNew()
        {
            DoProof(@"(
                  (((= (F A) (G @0))))
                  (( (= (F C) (G @0)) ) (= (F A) (G @0)) ) 
                  (() (= (F C) (G B) )) )
                 )");
            DoProof(@"(   
( ()            (A3))
  ( ((A3)) (B3) (C1) )
  ( ((B3))  )
  ( ((C1))  )
)");
            DoProof(@"(   
( ()            (A3 (D2 F) (E2 G H) I))
  ( ((A3 %X %Y I)) (B3 %Y %X %Z) (C1 F) )
  ( ((B3 (E2 G H) (D2 F) J)) (C1 F)  )
  ( ((C1 F))  )
)");
            DoProof(@"(   
( ()            (A3 (D2 F) (E2 G H) I))
  ( ((A3 %X %Y I)) (B3 %Y %X J) )
  ( ((B3 (E2 G H) (D2 F) J)) (C2 F G)  )
  ( ((C2 F G))  )
)");
        }

        [Test]
        public void TinyProof()
        {
            DoProof(@"( 
                          (() (= @0 @0)) 
                          (() (= (F A B) C) )
                          (( (= (F D C) (F D (F A B)) ))   )
                     )");
            DoProof(@"(   (((EQ @0 @1)) (EQ @1 @0 ) ) 
                          (() (EQ A B) )
                          (( (EQ B A) ) )
                     )");
            DoProof(@"(   (((EQ @0 @1) (EQ @1 @2 )) (EQ @0 @2) ) 
                          (() (EQ A B) )
                          (() (EQ B C) )
                          (( (EQ A C) ) )
                     )");
            DoProof(@"(   ( () (A))        ( ((A))  ) )");
            DoProof(@"(   ( () (A1 %X))    ( ((A1 B))  ) )");
            DoProof(@"(   ( () (A1 B))     ( ((A1 %X))  ) )");
            DoProof(@"(   ( () (A1 %X))    ( ((A1 %Y))  ) )");
            DoProof(@"(   ( () (A2 %X C))  ( ((A2 B %Y))  ) )");
            DoProof(@"(   ( () (A2 %X %Y)) ( ((A2 B C))  ) )");
            DoProof(@"(   ( () (A))        ( ((A)) (B)) ( ((B))) )");
            DoProof(@"(   ( () (A D E))    ( ((A %X %Y)) (B %Y %X) )  ( ((B E D)) )  )");
            DoProof(@"(   ( () (A D))      ( ((A %X)) (B %X) )  ( ((B D)) )  )");
            DoProof(@"(   ( () (A D))      ( ((A %X)) (B %X) )  ( ((B %X)) (C %X) )  ( ((C D)) )  )");
            DoProof(@"(   ( () (A (D2 F) (E2 G H)))    ( ((A %X %Y)) (B %Y %X) )  ( ((B (E2 G H) (D2 F))) )  )");
            DoProof(@"(   ( () (A2 @0 @1))  ( ( (A2 @0 (B2 @1 @0)) )  ) )");


            DoProof(@"(   ( () (P  @0 @1 (H (H  @0 @1) @1)))    ( ((P  @0 @0 @1))  ) )");

            DoProof(@"(   ( () (P  @0 (H  @0 @1) @1)  )    ( ( (P (H  @0 @1) @2 @1) )  ) )");

            DoProof(@"(   (() (= A B) (D)) 
                          (() (P B C) )
                          (( (P A C) )   )
                          (( (D) )   )
                     )");
            DoProof(@"(   (() (= (A) (B) ) ) 
                          (() (= (B) (C) ) )
                          (( (= (A) (C) ) ) )
                     )");
            DoProof(@"(   (() (D)) (( (D) )   ) )");
            DoProof(@"(   (() (= (A) (C) ) ) 
                          (( (= (A) (C) ) ) )
                     )");
            DoProof(@"(   (() (= A B) (D)) 
                          (() (= B C) )
                          (( (= A C) )   )
                          (( (D) )   )
                     )");
            DoProof(@"( 
                           (() (P A))
                           (((P C)) (P B))
                           (((P B)))
                           (() (= A E))
                           (() (= E C))
                     )");
           // is not a theorem: DoProof(@"(   (() (P A))  (((= A B)) (P B)) (((P B)))  )");
            DoProof(@"(   (() (= (F A B) C) )
                          (() (= B D))
                          (( (= (F A D) C) )   )
                     )");
            DoProof(@"(   
( ()            (A3 (D2 F) (E2 G H) I))
  ( ((A3 %X %Y I)) (B3 %Y %X J) )
  ( ((B3 (E2 G H) (D2 F) J)) )  
)");
        }

        // [Test]
        public void Para3Steps()
        {
#if false
                          (() (= @0 @0)) 
                          (() (= (F E @0) @0) )
                          (( (= @0 (F @1 @2)) ) (F (F @2 @1) @0) )
#endif
            DoProof(@"( 
                          (() (= (F @0 E) @0) )
                          (() (= (F @0 @0) E) )
                          (() (= (F (F @0 @1) @2) (F @0 (F @1 @2))) ) 
                          (() (= (F A B) C) )
                          (( (=  A (F C B)) )) 
                     )");
            DoProof(@"( 
                          (() (= (F @0 E) @0) )
                          (() (= (F @0 @0) E) )
                          (() (= (F (F @0 @1) @2) (F @0 (F @1 @2))) ) 
                          (( (=  @0 (F (F @0 @1) @1))) )) 
                     )");
            DoProof(@"( 
                          (() (= (F @0 @0) E) )
                          (() (= (F (F @0 @1) @2) (F @0 (F @1 @2))) ) 
                          (( (= (F @0 E) (F (F @0 @1) @1))) )) 
                     )");
        }

        // [Test]
        public void Para3Alt()
        {
            // Ex3Para, but with vbls instead of constants
            DoProof(@"( 
                          (() (= @0 @0)) 
                          (() (= (F @0 E) @0) )
                          (() (= (F E @0) @0) )
                          (() (= (F @0 @0) E) )
                          (() (= (F (F @0 @1) @2) ) (F @0 (F @1 @2))) ) 
                          (( (= @0 (F @1 @2)) ) (F (F @2 @1) @0) )
                     )");
        }

        [Test]
        public void Para3SubProblems()
        {
            return;
            // test cases on failed proof
            DoProof(@"( 
                          (() (= @0 @0)) 
                          (() (= (F B A) C) )
                          (() (= (F @0 E) @0) )
                          (( (= (F C E) (F B A)) ))   )
                     )");
            DoProof(@"( 
                          (() (= @0 @0)) 
                          (() (= (F B A) C) )
                          (() (= (F @0 E) @0) )
                          (() (= (F E @0) @0) )
                          (( (= (F C E) (F (F E B) A)) ))   )
                     )");
            DoProof(@"( 
                          (() (= @0 @0)) 
                          (() (= (F B A) C) )
                          (() (= (F @0 E) @0) )
                          (() (= (F E @0) @0) )
                          (() (= (F @0 @0) E) )
                          (( (= (F C E) (F (F (F @0  @0) B) A)) ))   )
                     )");
            DoProof(@"( 
                          (() (= @0 @0)) 
                          (() (= (F B A) C) )
                          (() (= (F @0 E) @0) )
                          (() (= (F E @0) @0) )
                          (() (= (F @0 @0) E) )
                          (( (F (F @0 @1) @2) ) (F @0 (F @1 @2)) ) 
                          (( (= (F C E) (F (F @0  (F @0 B)) A)) )   )
                     )");
            DoProof(@"( 
                          (() (= @0 @0)) 
                          (() (= (F B A) C) )
                          (() (= (F @0 E) @0) )
                          (() (= (F E @0) @0) )
                          (() (= (F @0 @0) E) )
                          (() (= (F (F @0 @1) @2)  (F @0 (F @1 @2))) ) 
                          (( (= (F C E) (F @0 (F (F @0 B) A))) )   )
                     )");
        }

        [Test]
        public void ReslabExample1()  // page 220
        {
            DoProof(@"
(
  ( ()
    . 
    ((P %R %S))
  )

  ( ((P %YY (F %XX %YY))
     (P (F %XX %YY) (F %XX %YY)))
    .
    ((Q %XX %YY))
  )

  ( ((P %Y (F %X %Y))
     (P (F %X %Y) (F %X %Y))
     (Q %X (F %X %Y))
     (Q (F %X %Y) (F %X %Y)))
    .
    ()
  )
)
");
        }

        [Test]
        public void ReslabExample2()  // page 220
        {
            LogRes logRes = new LogRes();
            Res res = new Res(irr: logRes); // , imd: Pss.imdSetOfSupport);
            LParse pres = new LParse();
            string stVars = "(%X %Y %Z %U %V %W %XX %YY %ZZ %UU %VV %WW %R %S %A %B %TT)";
            res.MakeVariables(pres.lsxParse(stVars));
            string stExample2 = @"
(
  ( ((P %X %Y %U)
     (P %Y %Z %V)
     (P %X %V %W))
    .
    ((P %U %Z %W))
  )
  ( ((P %XX %YY %UU)
     (P %YY %ZZ %VV)
     (P %UU %ZZ %WW))
    .
    ((P %XX %VV %WW))
  )
  ( ()
    .
    ((P (G %R %S) %R %S))
  )
  ( ()
    .
    ((P %A (H %A %B) %B))
  )
  ( ((P (K %TT) %TT (K %TT)))
    .
    ()
  )
)
";
            Lsx lsxExample2 = pres.lsxParse(stExample2);
            Asc ascProof = res.ascProve(lsxExample2, 4);
            Assert.IsNotNull(ascProof);
            
            /*
x*(y*z) = w -> (x*y)*z = w

(x*y)*z = w -> x*(y*z) = w

all r,s:  exists g: g*r = s
all a,b:  exists h: a*h = b

prove:
all t: exists k: k*t = t

------------

all x: exists k: k*x = k
             */
        }

        [Test]
        public void ReslabExample3()  // page 222
        {
            LParse pres = new LParse();
            Res res = new Res();
            string stVars = "(%X %Y %Z %U %V %W %XX %YY %ZZ %UU %VV %WW %R %S %A %B %TT)";
            res.MakeVariables(pres.lsxParse(stVars));
            string stExample3 = @"
(
  ( ((P %X %Y %U)
     (P %Y %Z %V)
     (P %X %V %W))
    .
    ((P %U %Z %W))
  )
  ( ((P %XX %YY %UU)
     (P %YY %ZZ %VV)
     (P %UU %ZZ %WW))
    .
    ((P %XX %VV %WW))
  )
  ( ()
    .
    ((P E %R %R))
  )
  ( ()
    .
    ((P %S E %S))
  )
  ( ()
    .
    ((P %TT %TT E))
  )
  ( ()
    .
    ((P A B C))
  )
  ( ((P B A C))
    .
    ()
  )
)
";
            Lsx lsxExample3 = pres.lsxParse(stExample3);
            Asc ascProof = res.ascProve(lsxExample3, 6);
            Assert.IsNotNull(ascProof);
        }


#if false
proof of Ex3
        regular order

       xyu  yzv xvw [] uzw    0
       xyu  yzv uzw [] xvw    1

[] ABC                        5
       xyu yzv xvw [] uzw     0
       ABC Bzv Avw [] Czw
           Bzv Avw [] Czw
[] xxE                         4
           BBE AEw [] CBw
               AEw [] CBw
[] xEx                         3
         [] CBA

       xyu yzv uzw [] xvw     1
[]xxE                         4
       xxE xzv Ezw [] xvw
           xzv Ezw [] xvw
[]CBA
      CBA EBw [] CAw
          EBw [] CAw
[]Exx                          2
          EBB [] CAB
              []CAB

       xyu yzv xvw [] uzw      0
[]CAB
       CAB Azv Cvw [] Bzw
           Azv Cvw [] Bzw
[]xxE                         4
       AAE CEw [] BAw
           CEw [] BAw
[]xEx                         3
           CEC [] BAC
       []BAC

which is contradiction
#endif
#if false
        xvw yzv xyu [] uzw
        uzw yzv xyu [] xvw

        xvw yzv xyu [] uzw 0
        aEa yzE ayu [] uza  <= 3,0
            bbE abu [] uba  <= 4,
                ABC [] CBA

        uzw yzv xyu [] xvw 1
        Eaa yav xyE [] xva <= 2,1
            CBA xCE [] xAB
                CCE [] CAB <=4,
                    [] CAB

                abu [] uba
                CAB [] BAC
                    [] BAC
#endif


        [Test]
        public void ReslabExample3v2()  // page 222
        {
            LParse pres = new LParse();
            Res res = new Res(); //  irr: new LogRes());
            string stVars = "(%X %Y %Z %U %V %W %XX %YY %ZZ %UU %VV %WW %R %S %A %B %TT)";
            res.MakeVariables(pres.lsxParse(stVars));
            string stExample3 = @"
(
  ( (
     (P %X %V %W)
     (P %Y %Z %V)
     (P %X %Y %U)
     )
    (P %U %Z %W))
  ( (
     (P %UU %ZZ %WW)
     (P %YY %ZZ %VV)
     (P %XX %YY %UU)
     )
     (P %XX %VV %WW))
  ( () (P E %R %R) )
  ( () (P %S E %S))
  ( () (P %TT %TT E))
  ( () (P A B C))
  ( ((P B A C)) )
)
";
            Lsx lsxExample3 = pres.lsxParse(stExample3);
            Asc ascProof = res.ascProve(lsxExample3, 6);
            Assert.IsNotNull(ascProof);
        }
    }
}
