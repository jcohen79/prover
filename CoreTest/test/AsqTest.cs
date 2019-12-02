using GrammarDLL;
using NUnit.Framework;
using System.Collections.Generic;

namespace reslab.test
{
   

    [TestFixture]
    [Category("Inference")]
    class AsqTest
    {
        static Res resNull = null;

        void CmiBitfieldLevel(uint nBitField, uint nNumBits, ref int nCount)
        {
            while (true)
            {
                uint nBitField3 = Rmub.nNextPowerSet(nBitField, nNumBits, true);
                if (nBitField3 != Rmub.nEndPowerSet)
                {
                    nCount++;
                    CmiBitfieldLevel(nBitField3, nNumBits, ref nCount);
                }

                uint nBitField2 = Rmub.nNextPowerSet(nBitField, nNumBits, false);
                if (nBitField2 == Rmub.nEndPowerSet)
                    break;
                nCount++;
                nBitField = nBitField2;
            }
        }

        void CheckCmiBitfield(uint nNumBits, uint nExpected)
        {
            int nCount = 1;
            uint nBitField = Rmub.nInitialPowerSet;
            CmiBitfieldLevel(nBitField, nNumBits, ref nCount);
            Assert.AreEqual(nExpected, nCount);

        }

        [Test]
        public void CmiBitfield()
        {
            CheckCmiBitfield(2, 3);
            CheckCmiBitfield(1, 1);
            CheckCmiBitfield(6, 63);
            CheckCmiBitfield(3, 7);
        }

        public void CheckUsyBit(uint nLeftFieldBits, uint nRightFieldBits, bool fResolveNegSideRight, int nExpectedCount)
        {
            Cbd cbd = new Cbd();
            Asc ascLeft = cbd.ascBuild("(( (a1) (b1) (c1) (d1)   )  (a2) (b2) (c2) (d2) (e2)  )");
            Asc ascRight = cbd.ascBuild("(( (a1) (b1) (c1) (d1) (e1)   )  (a2) (b2) (c2) (d2)   )");
            Abt abt = new Abt(new Avc(new Aic(ascLeft)), new Avc(new Aic(ascRight)));
            Prh prh = new Prh();

            Unb usy = new Usy(abt, prh, nLeftFieldBits, nRightFieldBits, fResolveNegSideRight);
            int cCount = 0;
            while (usy.fMore())
            {
                cCount++;
                if (cCount == 5)
                    usy = usy.unbCopy();
            }
            Assert.AreEqual(nExpectedCount, cCount);
        }

        [Test]
        public void UsyBitTest()
        {
            CheckUsyBit(0xA, 0x16, true, 6);
            CheckUsyBit(0x16, 0xB, true, 9);
        }

        Shv shvForShowTest(Avc avc)
        {
            Res res = new Res();
            Cmb cmb = new Rmup(res);
            Abt abt = new Abt(avc, null);
            cmb.SetAbt(abt);
            Shv shv = new Shv(cmb, null, null, abt.avcA, abt.avcB, null, null);
            return shv;
        }

        [Test]
        public void AscChainEqual()
        {
            LParse lparse = new LParse();
            Sko.AddSyms(lparse, new ExpressionEvaluatorGrammar());
            Lsm lsmx = new Lsm("x");
            lsmx.MakeVariable();
            Res res = new Res();

            Lsx lsxA = lparse.lsxParse("(  (((g b)) r)    (() a)  (((p (f (h b c)))  (p2 x) (p3 x))  )");
            LinkedList<Asc> rgasc = Asc.rgascConvert(lsxA, Asc.nNotCounted);
            Lsx lsxA2 = Asc.lsxFromChain(res, rgasc);
            Assert.IsTrue(lsxA.fEqual(lsxA2));
        }

        public void AddVar(LParse res, string stName)
        {
            Lsm lsmx = new Lsm(stName);
            lsmx.MakeVariable();
            res.AddSym(lsmx, null);
        }

        static string[] rgstVarNames = { "w", "x", "y", "z", "@3", "@4", "@5", "@6", "@7",
            "@14", "@15","@16","@17","@18","@26","@27","@28","@29","@30",
            "@39","@40","@41","@42","@43",
            "@258","@259","@260","@261","@262","@269","@270","@271","@272","@273","@281","@282","@283","@284","@285",
            "@294","@295","@296","@297","@298","@308","@309","@310","@311","@312",
            "A_3","A_4","A_6","A_7",
            "U", "V", "W", "X", "Y", "Z", "TT", "UU", "VV", "WW", "XX", "YY", "ZZ" };

        // return list of clauses in L that are not subsumed by any clause in S
        //    step through L, copy any clauses that is not subsume to output, and includes its symbols
        public static LinkedList<Asc> ascFilter(LinkedList<Asc> rgascL, LinkedList<Asc> rgascS)  // pg 256
        {
            LinkedList<Asc> rgascR = new LinkedList<Asc>();
            foreach (Asc ascL in rgascL)
            {
                bool fSubsumed = false;
                foreach (Asc ascS in rgascS)
                {
                    Abt abt = new Abt(ascS, ascL, resNull, ascNoVar: ascL);
                    if (abt.fEmbed())
                    {
                        fSubsumed = true;
                        break;
                    }
                }
                if (!fSubsumed)
                    rgascR.AddLast(ascL);
            }
            return rgascR;
        }

        public void PerformFilterEqual(string stL, string stS, string stExpected)
        {
            LParse lparse = new LParse();
            Sko.AddSyms(lparse, new ExpressionEvaluatorGrammar());

            foreach (string st in rgstVarNames)
                AddVar(lparse, st);

            Lsx lsxL = lparse.lsxParse(stL);
            LinkedList<Asc> ascL = Asc.rgascConvert(lsxL);
            Lsx lsxS = lparse.lsxParse(stS);
            LinkedList<Asc> ascS = Asc.rgascConvert(lsxS);
            Lsx lsxExpected = lparse.lsxParse(stExpected);

            //Qsc qsc = new Qsc("test");
            //qsc.FilterOrAdd();  - step through Asc_s in ascL list

            LinkedList<Asc> ascFiltered = ascFilter(ascL, ascS);   // only use of this fn

#if false
            Lsx lsxFiltered = (ascFiltered == null) ? Lsm.lsmNil : ascFiltered.lsxFromChain(res);
            Assert.IsTrue(lsxExpected.fEqual(lsxFiltered));
#else
            LinkedList<Asc> ascExpected = Asc.rgascConvert(lsxExpected);
            Assert.IsTrue(Asc.fMatchesSeq(ascExpected, ascFiltered));
#endif
        }

        [Test]
        public void AscFilterEqual()
        {
            PerformFilterEqual("( (((p (f (g a) b) (f (g a) b)))   ) )", "( (((p (f x b) (f x b))   ) )", "()");
            PerformFilterEqual("( (((p a)) (r a)  ) )", "( (((p x)) (r x)  ) )", "()");
            PerformFilterEqual("( (((p a))   ) )", "( (((p b))   ) )", "( (((p a))   ) )");
            PerformFilterEqual("( (((p x))   ) )", "( (((p x))   ) )", "()");
            PerformFilterEqual("( (((p a))  ) )", "( (() (p a)) )", "( (((p a))  ) )");
            PerformFilterEqual("( (((p a) (q b))   ) )", "( (((p a) (q b))   ) )", "()");
            PerformFilterEqual("( (((p a) (q b)) (r c)  ) )", "( (((q b) (p a)) (r c)  ) )", "()");
            PerformFilterEqual("( (((p a) (q a)) (r a)  ) )", "( (((q x) (p x)) (r x)  ) )", "()");
            PerformFilterEqual("( (() (p a) (q a) (r a)) )", "( (() (q x) (p x) (r x)) )", "()");
            PerformFilterEqual("( (((p a))) (((p b))) (((p c))) )", "( (((p b))) )", "( (((p a))) (((p c))) )");
            PerformFilterEqual("( (((p b a))) (((p a b))) (((p a a))) )", "( (((p x b))) )", "( (((p b a))) (((p a a))) )");
            PerformFilterEqual("( (((p b a))) (((p a a))) (((p a b))) )", "( (((p x x))) )", "( (((p b a))) (((p a b))) )");
            PerformFilterEqual("( (((p a (f a)))) (((p a (f b)))) )", "( (((p x (f x)))) )", "( (((p a (f b)))) )");
            PerformFilterEqual("( (((p a a) (p b a) (p c b))) )", "( (((p a x) (p y a) (p c x)) )",
                               "( (((p a a) (p b a) (p c b))) )");
            PerformFilterEqual("( (((p a a) (p b a) (p c a))) )", "( (((p a x) (p y a) (p c x)) )",
                               "(  )");
            PerformFilterEqual("( (((p a a) (p a b) (p c b))) )", "( (((p a x) (p c x)) )",
                               "(  )");
            PerformFilterEqual("( (((p a a) (p a b) (p c b))) )", "( (((p a x) (p y a) (p c x)) )",
                               "(  )");
            PerformFilterEqual("( (((p (f a b) (f a b)))   ) )", "( (((p x x))   ) )", "()");
            PerformFilterEqual("( (((p (f (g a) b) (f (g a) b)))   ) )", "( (((p x x))   ) )", "()");

            string stL = @"((((P  @3 @4 @5)   (P(G  @3 @6) @5 @7))   (P  @6 @4 @7)))";
            PerformFilterEqual(stL,
                @"((((P X Y U)(P Y Z V)(P X V W))
                      (P U Z W))
                     (((P XX YY UU)(P YY ZZ VV)(P UU ZZ WW))
                      (P XX VV WW))
                     (((P(K TT) TT (K TT)))))", stL);

            string stL2 =
                @"
((((P  (H  @323 @323) @324 @325)
   (P  @326 @324 @327))
  (P  @326 @325 @327)))
";

            PerformFilterEqual(stL2, /*
((((P  %X %Y %U) (P  %Y %Z %V) (P  %X %V %W))
  (P  %U %Z %W))
 (((P  %XX %YY %UU) (P  %YY %ZZ %VV) (P  %UU %ZZ %WW))
  (P  %XX %VV %WW))
 (((P  (K  %TT) %TT (K  %TT))))
 (((P  @258 @259 @260)
   (P  (G  @258 @261) @260 @262))
  (P  @261 @259 @262))
 (((P  (H  @269 @270) @271 @272)
   (P  @269 @272 @273))
  (P  @270 @271 @273)) */
@"(
 (((P  (H  @281 @281) @282 @283)
   (P  @284 @283 @285))
  (P  @284 @282 @285))
 (((P  @294 @295 @296) (P  @297 @295 @298))
  (P  (G  @294 @297) @296 @298))
 (((P  (H  @308 @309) @310 @311)
   (P  @309 @310 @312))
  (P  @308 @311 @312)))
", stL2);
            PerformFilterEqual("( (((p a a) (p a b) (p c b) (p a c)) )", "( (((p a x) (p y a) (p c x) (p y c)) )",
                               "(  )");
        }

        [Test]
        public void AscFilterEqualNew()
        {





            /* wrong expected
                        PerformFilterEqual(@"
            ((((P  (H  @39 @40) @41 @42)
               (P  @40 @41 @43))
              (P  @39 @42 @43)))", 
              @"(
             (((P  @26 @27 @28) (P  @29 @27 @30))
              (P  (G  @26 @29) @28 @30)))
            ", @"()");
            */
        }
    }
}