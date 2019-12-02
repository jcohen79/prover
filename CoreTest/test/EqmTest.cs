using NUnit.Framework;
using GrammarDLL;

namespace reslab.test
{
    class EqdTestHandler : Epu
    {
        EsdTestFactory etf;

        public EqdTestHandler(Abt abtToEquate, Unb unb, Res res, EsdTestFactory etf, Epu epuPrev, Vbv vbvPrev, Vbv vbvPrevPti)
                : base(res, abtToEquate, unb, epuPrev, vbvPrev, vbvPrevPti)
        {
            this.etf = etf;
        }
        protected override void ReportResult(Esn esnSolution)
        {
            Ascb ascbMerged = new Ascb();
            ascbMerged.DummySizes();

            Vbv.StartGlobalHack(atpToEquate);
            Sps sps = new Sps();
            Vbv vbvLeft = (Vbv)esnSolution;
            Vbv vbvRight = vbvLeft.vbvBSide();
            Shv.ShowTerm(ascbMerged, abtToEquate.avcA.aic.asc, abtToEquate.avcB.aic.asc,
                    Asc.nClauseLeadingSizeNumbers, vbvLeft, vbvRight, Vbv.vbvA,
                    null, null, sps, true);
//            mvbMapToVblIdForChild, mobToOffsetForChild, sps);

            Vbv.EndGlobalHack();


            ascbMerged.SetSizes(etf.nNegLiterals, etf.nPosLiterals);
            Asc ascMerged = ascbMerged.ascBuild();
            Assert.IsTrue(etf.ascExpected.Equals(ascMerged));
            etf.fFound = true;
        }
        protected override void MakeNextEpu(Unb unbLocal, Vbv vbvOutput, Vbv vbvPti)
        {
            Epu epuNext = new EqdTestHandler(abtToEquate, unbLocal, res, etf, this, vbvOutput, vbvPti);
            epuNext.Start();
        }

    }

    class EsdTestFactory 
    {
        public Prh prh;
        public Asc ascExpected;
        public int nNegLiterals;
        public int nPosLiterals;
        public Shv shv;
        public bool fFound = false;

        public EsdTestFactory (Prh prh, Asc ascExpected, int nNegLiterals, int nPosLiterals) 
        {
            this.prh = prh;
            this.ascExpected = ascExpected;
            this.nNegLiterals = nNegLiterals;
            this.nPosLiterals = nPosLiterals;
        }

        /// <summary>
        /// Like Esu.EquateUnify, but uses already existing object (one off, for testing only)
        /// </summary>
        public void SubEquateUnify(Abt abtToEquate, Unb unb, Res res) // pg 271
        {
            if (!unb.fMore())  // first one is here, the rest are in fProcessSolution
                return;
            EqdTestHandler eth = new EqdTestHandler(abtToEquate, unb, res, this, null, null, null);
            eth.Start();
        }
    }

    /// <summary>
    /// Perform unification from source (use EqrTest for lower level that takes positions discovered by EquateUnify)
    /// </summary>
    [TestFixture]
    [Category("Inference")]
    class EqmTest
    {
        public void CheckUnify(string stLeft, string stRight, string stExpected,
            bool fP1 = true, uint nLeftFieldBits = 1, uint nRightFieldBits = 1,
            bool fResolveNegSideRight = true, bool fNullNeg = true, int nNegLiterals = 0, int nPosLiterals = 1)
        {
            Cbd cbd = new Cbd();


            string stDefNeg = fNullNeg ? "(() " : "(";
            Asc ascLeft = cbd.ascBuild(stDefNeg + stLeft + ")");
            Asc ascRight = cbd.ascBuild(fNullNeg ? ("((" + stRight + "))") : ("(" + stRight + ")"));

            Prh prh = new Prh();
            Res res = new Res();
            Asc ascExpected = cbd.ascBuild(stDefNeg + stExpected + ")");
            EsdTestFactory etf = new EsdTestFactory(prh, ascExpected, nNegLiterals, nPosLiterals);
            res.iehEquateHandler = etf.SubEquateUnify;
            res.Init();
            Abt abt = new Abt(ascLeft, ascRight, res);

            prh.rmu = new Rmup(res);
            prh.rmu.SetAbt(abt);
            etf.shv = new Shv(prh.rmu, null, null, abt.avcA, abt.avcB, null, null);

            Unb unb;
            if (fP1)
                unb = new Up1(abt, prh, nLeftFieldBits, 0, fResolveNegSideRight); //  abt.P1Unify(nLeftFieldBits, fResolveNegSideRight, prh);
            else
                unb = new Usy(abt, prh, nLeftFieldBits, nRightFieldBits, fResolveNegSideRight); //    abt.SymmetricUnify(nLeftFieldBits, nRightFieldBits, fResolveNegSideRight, prh);
            res.iehEquateHandler(abt, unb, res);
            res.prs.Run();
            Assert.IsTrue(etf.fFound == (stExpected.Length != 0));
        }

        [Test]
        public void AscUnify()
        {
            CheckUnify("(a b x)", "(a y y)", "(a b b)");
            CheckUnify("(a x b)", "(a c y)", "(a c b)");
            CheckUnify("(a y y)", "(a x b)", "(a b b)");
            CheckUnify("(a x (g x))", "(a (f y) y)", "");
        }

        [Test]
        public void AscUnifyNew()
        {
            CheckUnify("(a w (f w))", "(a (f z) (f (f z)))", "(a (f z) (f (f z)))");
            CheckUnify("(a y y)", "(a x b)", "(a b b)");
            CheckUnify("(a x b)", "(a y y)", "(a b b)");
            CheckUnify("(a x b)", "(a (f y) y)", "(a (f b) b)");
            CheckUnify("(a (f x) w (f w))", "(a y (f z) (f (f z)))", "(a (f x) (f z) (f (f z)))");
            CheckUnify("(a x (g b))", "(a (f c) y)", "(a (f c) (g b))");
            CheckUnify("(a (f x (g b)) (g b))", "(a (f c y) y)", "(a (f c (g b)) (g b))");
            CheckUnify("(a x (g z) z)", "(a (f y) (g y) b)", "(a (f b) (g b) b)");
            CheckUnify("(a (f x) w)", "(a y (f z))", "(a (f x) (f z))");
            CheckUnify("(p (g b y (f3 y) x (f1 x)))", "(p (g Z (f4 Z) Y (f2 Y) W))",
                @"(p (g b (f4  b) (f3 (f4  b)) (f2 (f3 (f4  b))) (f1 (f2 (f3 (f4  b))))))");
            // 0 - (K 2), 2 - (H 1 1), 
            CheckUnify("(P  @0 (H  @1 @1) @0)", "(P (K  @2) @2 (K  @2))", "(P (K (H @1 @1)) (H @1 @1) (K (H @1 @1))   )");
            CheckUnify("(a b y z) (foo) (a x c z)", "(a x y d)", "(a b c d)",
                nLeftFieldBits: 5);
            CheckUnify("(P (G  @0 @1) @0 @1)", "(P (G  @0 @1) @2 @3))", "(P (G @0 @1) @0 @1)");

#if false
            // check unify multiple terms
            CheckUnify("((foo) (a x b))", "() (bar) (a c y)", "((a c b))",
                fP1: false, fNullNeg: false, nLeftFieldBits: 2, nRightFieldBits: 2, fResolveNegSideRight: false,
                nNegLiterals: 1, nPosLiterals: 0);
            CheckUnify("(p (g b y (f3 y) x (f1 x)))", "(p (g Z (f4 Z) Y (f2 Y) W))",
                @"(p (g b (f4  b) (f3 (f4  b)) (f2 (f3 (f4  b))) (f1 (f2 (f3 (f4  b))))))",
                    fP1: false);
            CheckUnify("((a x w e) (foo) (a x b z))", "() (bar) (a c y w)", "((a c b e))",
                fP1: true, fNullNeg: false,
                nLeftFieldBits: 5,
                nRightFieldBits: 2,
                fResolveNegSideRight: false,
                nNegLiterals: 1, nPosLiterals: 0);
#endif

        }

        /// <summary>
        /// testing only,
        /// </summary>
        public class OpcAlwaysDefn : Opc
        {
            public static OpcAlwaysDefn opcOnly = new OpcAlwaysDefn();
            public bool fUseOnDemand(Pti ptiNew)
            {
                return true;
            }
        }

        public void PerformResolve(string stLeft, string stRight, string stExpected, string stAxioms = null, bool fValidateEub = true)
        {
            Cbd cbd = new Cbd();

            Asc ascLeft = cbd.ascBuild(stLeft);
            ascLeft.gfbSource = Gfc.gfcAxiom;
            ascLeft.nResolveTerm = Asc.nNoResolveTerm;
            Asc ascRight = cbd.ascBuild(stRight);
            ascRight.gfbSource = Gfc.gfcAxiom;
            ascRight.nResolveTerm = Asc.nHasResolveTerm;
            Lsx lsxExpected = cbd.lparse.lsxParse("(" + stExpected + ")");

            Res res = new Res();
            res.fUseNegatedGoals = false;
            res.fBuildClauseFromHyp = false;
            if (!fValidateEub)
                res.fValidateEub = false;

            if (stAxioms != null)
                res.fDoParamodulation = true;
            res.Init();

            if (stAxioms != null)
            {
                Lsx lsxAxioms = cbd.lsxParse("(" + stAxioms + ")");
                res.AddAxioms(lsxAxioms, Asc.nNotCounted);
            }

            Rmup cmi = new Rmup(res);
            cmi.SetAbt(new Abt(ascLeft, ascRight, res));
            cmi.fInit();
            res.prs.Add(cmi);
            res.prs.Add(res);   // perform filter on pti_s saved as unfiltered when created by Cgs
            res.prs.Run();

            Lsx lsxPlace = lsxExpected;
            while (lsxPlace is Lpr)
            {
                Lpr lprPlace = (Lpr)lsxPlace;
                Lsx lsxTerm = lprPlace.lsxCar;
                Asc ascTerm = Asc.ascFromLsx(lsxTerm);
                ascTerm.nResolveTerm = (ascTerm.rgnTree[Asc.nPosnNumNegTerms] == 0) ? Asc.nNoResolveTerm : Asc.nHasResolveTerm;
                Assert.IsTrue(res.fFind(ascTerm));
                lsxPlace = lprPlace.lsxCdr;
            }
        }

        [Test]
        public void AscResolve()
        {
            PerformResolve("(() (p x a))", "(((p b y)) (q y))", "(() (q a))");



            PerformResolve("(() (p a))", "(((p a)) (p b))", "(() (p b))");
            PerformResolve("(() (p a))", "(((p a) (q a)) (p b))", "(((q a)) (p b))");
            PerformResolve("(() (p b) (p a))", "(((p b)) )", "(() (p a))");
            PerformResolve("(() (p b) (p a))", "(((p b)) (p c))", "(() (p a) (p c))");
            PerformResolve("(() (p a) (p b))", "(((p b)) )", "(() (p a))");
            PerformResolve("(nil (P (G @0 @1) @0 @1))", "(((P (G @0 @1) @2 @3)) (P  @1 (H @0 @2) @3))",
                " (nil  (P  @0 (H  @1 @1) @0))");
            PerformResolve("(() (p a) (p b))", "(((p b)) )", "(() (p a))");

        }

        [Test]
        public void AscResolveNew()
        {
            PerformResolve("(() (p a))", "(((p c)) (p b))", "(() (p b))", "(nil (= a c))");
            PerformResolve("(() (p a x))", "(((p c d)) (q b))", "(() (q b))", "(nil (= a c))");

            PerformResolve("(() (p a) (p b))", "(((p a)) (p b))", "(() (p b))");
            PerformResolve("(nil (q x) (p x a))", "(((p (f b) y)) (q y))", "(nil (q (f b)) (q a))");
            PerformResolve("(nil  (E  s2 s4) (Uk  s2 s4) (B  s2 s4) (Uk  s4 s2))",
                            "(((B  X Y) (E  X Y)))",
                            "");  // tautology (E  s2 s4)
            // having two pti needs gnpR to performFIlter

        }

    }

    [TestFixture]
    [Category("Inference")]
    class NgcTest
    {
        int nStopCount = 20;

        class CheckNgc : Irp
        {
            public Eqs eqsToShow;
            public Asc[] rgascExpected;
            public bool[] rgfFoundExpected;
            public Res res;
            int nStopAfter;

            public CheckNgc(int nStopAfter)
            {
                this.nStopAfter = nStopAfter;
            }

            public void Report(Tcd tcdEvent, Ibd objTarget, Ibd objInput, object objData)
            {
                if (tcdEvent == Tcd.tcdAscFromNgc)
                {
                    Asc ascFromNgc = (Asc)objData;
                    if (ascFromNgc.gfbSource is Gfi)
                    {
                        Gfi gfi = (Gfi)ascFromNgc.gfbSource;
                        if (gfi.gfhFrom is Ngc)
                        {
                            Ngc ngc = (Ngc)gfi.gfhFrom;
                            if (ngc.eqsGoal == eqsToShow)
                            {
                                bool fFound = false;
                                for(int nEx = 0; nEx < rgascExpected.Length; nEx++)
                                {
                                    Asc ascExpected = rgascExpected[nEx];
                                    if (ascExpected.Equals(ascFromNgc))
                                    {
                                        fFound = true;
                                        rgfFoundExpected[nEx] = true;
                                    }
                                }
                                Assert.IsTrue(fFound);
                                if (--nStopAfter <= 0)
                                    res.prs.Done();
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Get the vbv to solve stEqsForSoln, apply to Ngc from stEqsForNgc, then resolve asc from Ngc with stAscToResolve
        /// </summary>
        void PerformNgcRefute (string stNgcL, string stNgcR, string stAxioms, params string[] rgstExpectedAsc)
        {
            Cbd cbd = new Cbd();

            Res res = new Res();
            CheckNgc chk = new CheckNgc(nStopCount);
            chk.res = res;
            res.irp = chk;
            res.fVerifyEachStep = false;
            res.Init();

            Atp atpToRefute = Atp.atpMake(cbd.lparse, " (() " + stNgcL + ")", " (() " + stNgcR + ")");
            Eqs eqsToShow = new Eqs(res, atpToRefute);
            eqsToShow.FirstStep();
            chk.eqsToShow = eqsToShow;

            Lsx lsxAxioms = cbd.lparse.lsxParse("(" + stAxioms + ")");
            chk.rgascExpected = new Asc[rgstExpectedAsc.Length];
            chk.rgfFoundExpected = new bool[rgstExpectedAsc.Length];
            for (int nEx = 0; nEx < rgstExpectedAsc.Length; nEx++)
            {
                chk.rgascExpected[nEx] = cbd.ascBuild(rgstExpectedAsc[nEx]);
                chk.rgfFoundExpected[nEx] = false;
            }
            Asc ascProof = res.ascProve(lsxAxioms, 0);

            // no need for proof, just check that correct Asc is inferred
            for (int nEx = 0; nEx < rgstExpectedAsc.Length; nEx++)
            {
                Assert.IsTrue(chk.rgfFoundExpected[nEx]);
            }
        }

        [Test]
        public void NgcRefuteNew()
        {
            nStopCount = 1;
            PerformNgcRefute("(F A)", "(G @0)",
                             @"(( (= (F C) (G @0)) ) (= (F A) (G @0)) ) 
                               (() (= (F C) (G B) )) )",
                             "(() (= (F A) (G B))) )");
            // more cases:
            //    multiple steps in proof
            //    multiple uses of ngc in refutation
            //    replacement (Pti) in soln

        }

        [Test]
        public void NgcRefute()
        {
            PerformNgcRefute("A", "(F B C)",   // eqs to prove
                             "(() (= A (F B C)) )",      // axioms to prove negation of ngc
                             "(() (= A (F B C)))");   // expected axiom to infer
            PerformNgcRefute("A", "(F B @0)",
                             "(() (= A (F B C)))",
                             "(() (= A (F B C)) )");
            PerformNgcRefute("A", "(F @0 @1 @2)",
                             "(() (= A (F B C D)))",
                             "(() (= A (F B C D)) )");
            PerformNgcRefute("@0", "(F @1 @2 @3)",
                             "(() (= A (F B C D)) )",
                             "(() (= A (F B C D)))",
                             "(() (= (F B C D) (F B C D)))");
            PerformNgcRefute("(F @0 (G @0))", "(F @1 @2)",
                             "(() (= (F A (G @0)) (F @0 A)) )",
                             "(() (= (F A (G A)) (F A A)))",
                             "(() (= (F A A) (F A (G A))))",
                             "(() (= (F A A) (F A A)))",
                             "(() (= (F A (G A)) (F A (G A))))");
            PerformNgcRefute("(F A)", "(G @0)",
                             @"(( (= (F C) (G @0)) ) (= (F A) (G @0)) ) 
                               (() (= (F C) (G @0) )) )",
                             "(() (= (F A) (G @0))) )");
        }
    }
}