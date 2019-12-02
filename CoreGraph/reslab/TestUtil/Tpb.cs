using System;
using System.Collections.Generic;
using System.Text;

namespace reslab.TestUtil
{
    /// <summary>
    /// Base class to test proof structure: build a tree that is used to build linear sequence of Tst.
    /// Derived classes create test step and action sequences based on function that generates that based on parameters
    /// </summary>
    public abstract class Tpb
    {
        static int cId = 0;
        int nId = cId++;
        public Tst tstStart;  // set by InsertXXX on parent to tell this were to attach actions (Tob) to
        public Tst tstExit; // set by InsertXXX on parent to tell this where to transition to at the end of this part of proof

        public Pou pouOutline;
        public Tpb tpbParent;

        protected Tpb()
        {
        }

        /// <summary>
        /// Add a Tst to the outline
        /// </summary>
        protected Tst tstAdd()
        {
            return new Tst(pouOutline);
        }

        /// <summary>
        /// Make the child Tpb a part of the Tpb tree. Called by the specific methods for each defined gap.
        /// </summary>
        protected void Insert(Tpb tpbChild, Tst tstStart, Tst tstExit)
        {
            tpbChild.pouOutline = pouOutline;
            tpbChild.tstStart = tstStart;
            tpbChild.tstExit = tstExit;
            tpbChild.tpbParent = this;
            tpbChild.WhenInserted();
        }

        /// <summary>
        /// Called by parent when it adds this to tpb tree
        /// </summary>
        protected virtual void WhenInserted()
        {
            Build();
        }

        /// <summary>
        /// Construct the test sequence based on the parameters that have been set up.
        /// Will leave gaps where lower level Tpb_s can be build as needed.
        /// for each gap, the derived class provides an InsertXXX method to pass in the parameters from the parent
        /// </summary>
        public abstract void Build();
    }
    /// <summary>
    /// Root of proof tree. Contains a series of resolutions.
    /// </summary>
    public class TpProof : Tpb
    {
        public Tst tstCurrent;   // maintained by this, to indicate where actions should be attached

        public TpProof(Pou pou)
        {
            pouOutline = pou;
            tstCurrent = tstAdd();
        }

        public void NotePti()
        {
            TpProof tp = this;

            // TODO: move to proof specific, to be added to first Tst
            Toh<Pti> tibStmt4 = new Toh<Pti>(tp.tstCurrent);
            tibStmt4.SetName("pti2LtR");
            tibStmt4.SetTinName("Pti");
            tibStmt4.SetValue(tp.pouOutline.vrfGet<Pti>("pti2LtR"));
            Toh<Pti> tibStmt5 = new Toh<Pti>(tp.tstCurrent);
            tibStmt5.SetName("pti4LtR");
            tibStmt5.SetTinName("Pti");
            tibStmt5.SetValue(tp.pouOutline.vrfGet<Pti>("pti4LtR"));
            Toh<Pti> tibStmt6 = new Toh<Pti>(tp.tstCurrent);
            tibStmt6.SetName("pti5RtL");
            tibStmt6.SetTinName("Pti");
            tibStmt6.SetValue(tp.pouOutline.vrfGet<Pti>("pti5RtL"));
        }

        /// <summary>
        /// Insert at end of list
        /// </summary>
        public TpResolve tpbInsertMember(TpResolve tpbStep)
        {
            tpbStep.tstStart = tstCurrent;
            Insert(tpbStep, tstCurrent, tstExit);
            tstCurrent = tpbStep.tstExit;

            return tpbStep;
        }

        public override void Build()
        {
            // nothing left since each child is built on each insert
        }
    }

    /// <summary>
    /// Root of subtree of Tpb for resolving two clauses
    /// </summary>
    public class TpResolve : Tpb
    {
        string stLeft;
        string stRight;

        Hbv<Epu> bodHbv154 = new Hbv<Epu>();
        Hbv<Atp> bodHbv155 = new Hbv<Atp>();
        Hbv<Eqs> bodHbv156 = new Hbv<Eqs>();
        public TpEqs tpEqs;

        public bool fNeedWatch156 = false;

        public TpResolve(string stLeft, string stRight)
        {
            this.stLeft = stLeft;
            this.stRight = stRight;
        }

        public override void Build()
        {

            Tos tibStmt3 = new Tos(tstStart);
            tibStmt3.SetLeft(pouOutline.ivbGet<Asc>(stLeft));
            tibStmt3.SetRight(pouOutline.ivbGet<Asc>(stRight));
            Tst tstNext = tstAdd();
            tibStmt3.SetNextTst(tstNext);

            Tod tibStmt6 = new Tod(tstNext);
            tibStmt6.kKind = KCapture.kData;
            tibStmt6.hbvPlace = bodHbv154;
            Top tibStmt7 = new Top(tstNext);
            tibStmt7.SetTarget(bodHbv154);
            tibStmt7.SetInput(null);
            tibStmt7.SetTcd(Tcd.tcdEqsForEpu);
            tstNext = tstAdd();
            tibStmt7.SetNextTst(tstNext);
            tibStmt7.SetLabel("make Epu from two asc");
            Tod tibStmt10 = new Tod(tstNext);
            tibStmt10.kKind = KCapture.kData;
            tibStmt10.hbvPlace = bodHbv155;
            Toa tibStmt11 = new Toa(tstNext);
            tibStmt11.SetTff(TAtpToEqs.tff);
            tibStmt11.SetResult(bodHbv156);
            tibStmt11.SetArgs(bodHbv155);
            Tod tibStmt12 = new Tod(tstNext);
            tibStmt12.kKind = KCapture.kResult;
            tibStmt12.hbvPlace = bodHbv156;

            if (fNeedWatch156)
            {
                Tst tifStep3 = tstAdd();
                Tow tibStmt13 = new Tow(tstNext);
                tibStmt13.SetToWatch(bodHbv156);
                tstNext = tstAdd();
                tibStmt13.SetNextTst(tstNext);
            }

            // gap for eqs: is already inserted here, set parms and then build it
            tpEqs = new TpEqs();
            Tst tstExitEqs = tstAdd();
            Insert(tpEqs, tstNext, tstExitEqs);
            tpEqs.hbvEqsInput = bodHbv156;
            tstNext = tstExitEqs;

            // footer
            tstExit = tstExitEqs;   // TODO where to add next resolve step - really should be where ngc needs to go
        }
    }

    /// <summary>
    /// Root of subtree for solving Eqs
    /// </summary>
    public class TpEqs : Tpb
    {
        public TpEtp etp;
        public Hbv<Eqs> hbvEqsInput;
        public Hbv<Eqs> hbvEqsParent;
        Tst tstHaveEqs;

        public TpEqs()
        {
        }

        public override void Build()
        {
            tstHaveEqs = tstAdd();
        }

        void InsertTpb(Tpb tpb)
        {
            Insert(tpb, tstStart, tstExit);
        }


        /// <summary>
        /// says Eqs will solve using an Etp
        /// </summary>
        public TpEtp InsertEtp(TpEtp tpEtp)
        {
            tpEtp.hbvEqsInput = hbvEqsInput;
            InsertTpb(tpEtp);
            return tpEtp;
        }

        /// <summary>
        /// says Eqs will solve using an Eul (for Pti)
        /// </summary>
        public TpEul InsertEul(TpEul tpEul)
        {
            tpEul.hbvEqsInput = hbvEqsInput;
            tpEul.hbvEqsParent = hbvEqsParent;
            InsertTpb(tpEul);
            return tpEul;
        }
    }

    /// <summary>
    /// Root of subtree for solving Eul/Eur for Pti
    /// </summary>
    public class TpEul : Tpb
    {
        public Hbv<Eqs> hbvEqsInput;
        public Hbv<Eqs> hbvEqsParent;
        public string stPtiName;
        Hbv<Eul> bodHbv2027 = new Hbv<Eul>();
        Hbv<Vbv> bodHbv2262 = new Hbv<Vbv>();
        Hbv<Eur> bodHbv2268 = new Hbv<Eur>();
        Hbv<Eqs> bodHbv2029 = new Hbv<Eqs>();
        Tst tstToGetEqs;

        public TpEul() { }

        public override void Build()
        {
            tstToGetEqs = tstAdd();
            Tst tstNext = tstStart;

            Top tibStmt30 = new Top(tstNext);
            tibStmt30.SetTarget(hbvEqsInput);
            tibStmt30.SetInput(pouOutline.vrfGet<Pti>(stPtiName));
            tibStmt30.SetTcd(Tcd.tcdApplyPtiToEqs);
            tstNext = tstAdd();
            tibStmt30.SetNextTst(tstNext);
            tibStmt30.SetLabel("find eul for " + stPtiName);
            Tod tibStmt107 = new Tod(tstNext);
            tibStmt107.kKind = KCapture.kData;
            tibStmt107.hbvPlace = bodHbv2027;
            Tog tibStmt108 = new Tog(tstNext);
            tibStmt108.SetTarget(bodHbv2027);
            tibStmt108.SetTcd(Tcd.tcdLaunchEur);
            tstNext = tstToGetEqs;
            tibStmt108.SetNextTst(tstNext);
            Tod tibStmt117 = new Tod(tstNext);
            tibStmt117.kKind = KCapture.kRight;
            tibStmt117.hbvPlace = bodHbv2262;
            Tod tibStmt118 = new Tod(tstNext);
            tibStmt118.kKind = KCapture.kData;
            tibStmt118.hbvPlace = bodHbv2268;

            //  TpGetEqsPhase1Pti2 removed from here

            // did the above skip a trivial match of C to @0 ?

            // TODO where is left child?

            // right child ?

            // finish up using the variable defined above
        }

        public void InsertGetEqs(TpGetEqsB tpGetEqs)
        {
            tpGetEqs.hbvEulInput = bodHbv2027;
            tpGetEqs.hbvEurInput = bodHbv2268;
            tpGetEqs.hbvEqsOutput = hbvEqsParent;
            Insert(tpGetEqs, tstToGetEqs, tstExit);
        }

        public void InsertGetEqs(TpEtp tpEtp)
        {
         /*   tpEtp.hbvEulInput = bodHbv2027;
            tpEtp.hbvEqsOutput = hbvEqsParent; */
            tpEtp.hbvEurInput = bodHbv2268;
            tpEtp.hbvEqsParent = hbvEqsParent; 
            tpEtp.hbvEqsInput = hbvEqsInput;
            tpEtp.hbvEqsParent = hbvEqsParent;
         //   tpEtp.hbvEtp = 
            Insert(tpEtp, tstToGetEqs, tstExit);
        }
    }

    /// <summary>
    /// Hold info for each child of TpEtp
    /// </summary>
    public class HlpEtp
    {
        public Hbv<Etp> bodHbv158 = new Hbv<Etp>();
        public bool fNeedWatch160 = false;
        public bool fSkipEqs = false;
        public bool fSkipTerm = false;
        public TpEqs tpeqsChild = new TpEqs();
    }

    /// <summary>
    /// Root of subtree for solving Eqs
    /// </summary>
    public class TpEtp : Tpb
    {
        public Hbv<Eqs> hbvEqsInput;
        public Hbv<Eur> hbvEurInput;
        public Hbv<Eqs> hbvEqsParent;
        int nNumSubterms;
        HlpEtp[] rghlpEtpSubterms;
        Tst tstToUp;
        Tst tstNext;

        public TpEtp(int nNumSubterms)
        {
            SetNumSubterms(nNumSubterms);
        }

        public void SetNumSubterms(int nNumSubterms)
        {
            this.nNumSubterms = nNumSubterms;
            rghlpEtpSubterms = new HlpEtp[nNumSubterms];
            for (int i = 0; i < nNumSubterms; i++)
            {
                HlpEtp hlpEtp = new HlpEtp();
                rghlpEtpSubterms[i] = hlpEtp;
            }
        }

        public HlpEtp hlpChild(int n)
        {
            return rghlpEtpSubterms[n];

        }

        void BuildTerm(int nPos, HlpEtp hlpEtp)
        {
                Top tibStmt23 = new Top(tstNext);
                tibStmt23.SetTarget(hbvEqsInput);
                string stNin;
                switch (nPos)
                {
                    case 0:
                        stNin = "nin2";
                        break;
                    case 1:
                        stNin = "nin3";
                        break;
                    default:
                        throw new InvalidProgramException();
                }
                tibStmt23.SetInput(pouOutline.vrfGet<Nin>(stNin)); // TODO generalize this
                tibStmt23.SetTcd(Tcd.tcdRegisterEtpByOffset);

                tstNext = tstAdd();
                TpEqs tpEqs = hlpEtp.tpeqsChild;
                tpEqs.pouOutline = pouOutline;
                if (hlpEtp.fSkipEqs)
                {
                    tibStmt23.SetNextTst(tstNext);
                }
                else
                {
                    Hbv<Eqs> bodHbv160 = new Hbv<Eqs>();

                    tibStmt23.SetNextTst(tstNext);
                    tibStmt23.SetLabel("find child num " + nPos + " eqs for eqsToSolve");
                    Tod tibStmt27 = new Tod(tstNext);
                    tibStmt27.kKind = KCapture.kData;
                    tibStmt27.hbvPlace = hlpEtp.bodHbv158;

                    Toa tibStmt28 = new Toa(tstNext);
                    tibStmt28.SetTff(TfEtpToEqs.tff);
                    tibStmt28.SetResult(bodHbv160);
                    tibStmt28.SetArgs(hlpEtp.bodHbv158);
                    Tod tibStmt29 = new Tod(tstNext);
                    tibStmt29.kKind = KCapture.kResult;
                    tibStmt29.hbvPlace = bodHbv160;
                    if (hlpEtp.fNeedWatch160)
                    {
                        Tow tibStmt226 = new Tow(tstNext);
                        tibStmt226.SetToWatch(bodHbv160);
                        tstNext = tstAdd();
                        tibStmt226.SetNextTst(tstNext);
                    }

                    // next: make gap to apply the pti for this child. see old outline PouEx2.cs
                    Tst tstAfter;
                    if (nPos == rghlpEtpSubterms.Length - 1)
                        tstAfter = tstToUp;
                    else
                        tstAfter = tstAdd();
                    tpEqs.hbvEqsInput = bodHbv160;
                    tpEqs.hbvEqsParent = hbvEqsInput;
                    Insert(tpEqs, tstNext, tstAfter);
                    tstNext = tstAfter;
                }


        }

        public override void Build()
        {
            tstNext = tstStart;
            int nPos = 0;
            foreach (HlpEtp hlpEtp in rghlpEtpSubterms)
            {
                if (!hlpEtp.fSkipTerm)
                {
                    if (tstToUp == null)
                        tstToUp = tstAdd();
                    BuildTerm(nPos, hlpEtp);
                }
                nPos++;
            }
            if (tstToUp == null)
                tstToUp = tstStart;
            // TODO: exit
        }

        /// <summary>
        /// Insert Tp that will handle processing solution at end of Etp
        /// </summary>
        public void InsertWrapUp (TpEtpUpB tpEtpUpB)
        {
            tpEtpUpB.hbvEurInput = hbvEurInput;  // Eur#2147 3->6 in (nil  (=  (F  @0 @0) E))
            tpEtpUpB.hbvEqsInput = hbvEqsInput; // eqsㅕ2038 (((F  @0 @1)) (E))ㅑ
            // tpEtpUpB.hbvEqsOutput = ;
         //   tpEtpUpB.hbvEtp = ; 	// Etp#2036 3,8 (((F  @0 (F  @1 @2))) ((F  @3 E)))
            tpEtpUpB.bodHbv2029 = hbvEqsParent; // eqsㅕ2030 (((F  @0 (F  @1 @2))) ((F  @3 E)))ㅑ
            
            Insert(tpEtpUpB, tstToUp, tstExit);
        }
    }

    /// <summary>
    /// Base class for getting the eqs for Eub
    /// </summary>
    public abstract class TpGetEqsB : Tpb
    {
        public Hbv<Eul> hbvEulInput;
        public Hbv<Eur> hbvEurInput;
        public Hbv<Eqs> hbvEqsOutput;

        public TpEqs tpEqs;

        protected void AddEqs(Tst tstNext)
        {
            tpEqs = new TpEqs();
            tpEqs.hbvEqsInput = hbvEqsOutput;
            Insert(tpEqs, tstNext, tstExit);
        }
    }

    /// <summary>
    /// Ad hoc for now to get eqs for pti2 in phase 1
    /// </summary>
    public class TpGetEqsPhase1Pti2 : TpGetEqsB
    {

        public override void Build()
        {
            hbvEqsOutput = new Hbv<Eqs>();

            Tst tstNext = tstStart;
            Toa tibStmt119 = new Toa(tstNext);
            tibStmt119.SetTff(TEqsSub.tff);
            tibStmt119.SetResult(hbvEqsOutput);
            tibStmt119.SetArgs(hbvEulInput);
            Tod tibStmt120 = new Tod(tstNext);
            tibStmt120.kKind = KCapture.kResult;
            tibStmt120.hbvPlace = hbvEqsOutput;    // is this needed?
            Tow tibStmt121 = new Tow(tstNext);     // TODO: check flag
            tibStmt121.SetToWatch(hbvEqsOutput);
            tstNext = tstAdd();
            tibStmt121.SetNextTst(tstNext);

            AddEqs(tstNext);

        }
    }

    /// <summary>
    /// Base class for return result to Etp for Eur
    /// </summary>
    public abstract class TpEtpUpB : Tpb
    {
        public Hbv<Eur> hbvEurInput;   // Eur#2147 3->6 in (nil  (=  (F  @0 @0) E))
        public Hbv<Eqs> hbvEqsInput;  // from TpEul.hbvEqsInput
        public Hbv<Eqs> hbvEqsOutput;
        public Hbv<Eqs> bodHbv2029;  // what is this?
        public Hbv<Etp> hbvEtp;   // bodHbv2036 Etp#2036 3,8 (((F  @0 (F  @1 @2))) ((F  @3 E))), set in tibStmt223

        public TpEqs tpEqs;

        protected void AddEqs(Tst tstNext)
        {
            tpEqs = new TpEqs();
            tpEqs.hbvEqsInput = hbvEqsOutput;
            Insert(tpEqs, tstNext, tstExit);
        }
    }


    /// <summary>
    /// Apply trivial Vbv from (F @0 @0) vs (F @1 @2) to etp that was used
    /// </summary>
    public class TpGetEqsPhase1Pti4Up : TpEtpUpB
    {
        Hbv<Eqs> bodHbv491 = new Hbv<Eqs>();
        Hbv<Vbv> bodHbv2154 = new Hbv<Vbv>();
        Hbv<Vbv> bodHbv2183 = new Hbv<Vbv>();
 

        public override void Build()
        {
            Tst tstNext = tstStart;
            Toi tibStmt342 = new Toi(tstNext);
            tibStmt342.SetInput(hbvEurInput); // Eur#2147 3->6 in (nil  (=  (F  @0 @0) E))
            tibStmt342.SetTcd(Tcd.tcdTransferLeftEqs);
            tstNext = tstAdd();
            tibStmt342.SetNextTst(tstNext);
            Tod tibStmt343 = new Tod(tstNext);
            tibStmt343.kKind = KCapture.kLeft;
            tibStmt343.hbvPlace = bodHbv491;
            Toi tibStmt345 = new Toi(tstNext);
            tibStmt345.SetInput(bodHbv491);
            tibStmt345.SetTcd(Tcd.tcdEqsToEnt);
            Tst tstNext2 = tstAdd();
            tibStmt345.SetNextTst(tstNext2);   // junk?
            Top tibStmt346 = new Top(tstNext);
            tibStmt346.SetTarget(hbvEurInput);
            tibStmt346.SetInput(bodHbv491);
            tibStmt346.SetTcd(Tcd.tcdEqsToEnt);
            tstNext = tstAdd();
            tibStmt346.SetNextTst(tstNext);
            tibStmt346.SetLabel("solns coming from eqsEE to pti4");
            Top tibStmt353 = new Top(tstNext);
            tibStmt353.SetTarget(hbvEqsInput);// eqsㅕ2038 (((F  @0 @1)) (E))ㅑ
            tibStmt353.SetInput(hbvEurInput);
            tibStmt353.SetTcd(Tcd.tcdEurToNotifyEqs);
            tstNext = tstAdd();
            tibStmt353.SetNextTst(tstNext);
            tibStmt353.SetLabel("soln from eqsEE using pti4 to {{eqs#8103 (((F  @0 @1)) (E))}");

            // move this to higher level - on caller?
            // hard code hack for now
            TpEtp tpP1 = (TpEtp)tpbParent;
            TpEul eul1 = (TpEul)tpP1.tpbParent;
            TpEqs eqs1 = (TpEqs)eul1.tpbParent;
            TpEtp etp2 = (TpEtp)eqs1.tpbParent;
            hbvEtp = etp2.hlpChild(1).bodHbv158; 

            Tod tibStmt356 = new Tod(tstNext);
            tibStmt356.kKind = KCapture.kData;
            tibStmt356.hbvPlace = bodHbv2154;
            Top tibStmt357 = new Top(tstNext);
            tibStmt357.SetTarget(hbvEtp);   // Etp#2036 3,8 (((F  @0 (F  @1 @2))) ((F  @3 E)))
            tibStmt357.SetInput(bodHbv2154);
            tibStmt357.SetTcd(Tcd.tcdSolnToEqs);
            tstNext = tstAdd();
            tibStmt357.SetNextTst(tstNext);
            tibStmt357.SetLabel("process soln from eqsEE using pti4 to {{eqs#8103 (((F  @0 @1)) (E))}");
            Top tibStmt362 = new Top(tstNext);
            tibStmt362.SetTarget(hbvEtp);	// Etp#2036 3,8 (((F  @0 (F  @1 @2))) ((F  @3 E)))
            tibStmt362.SetInput(bodHbv2154);
            tibStmt362.SetTcd(Tcd.tcdVbvForEtp);
            tstNext = tstAdd();
            tibStmt362.SetNextTst(tstNext);
            tibStmt362.SetLabel("get merged pti4/EE vbv from etp of ax3 and ax1");
            Tod tibStmt365 = new Tod(tstNext);
            tibStmt365.kKind = KCapture.kData;
            tibStmt365.hbvPlace = bodHbv2183;
            Toa tibStmt366 = new Toa(tstNext);
            tibStmt366.SetTff(TfFindDup.tff);
            tibStmt366.SetResult(bodHbv2183);
            tibStmt366.SetArgs(bodHbv2029);
            tibStmt366.SetArgs(bodHbv2183);
            Tow tibStmt368 = new Tow(tstNext);
            tibStmt368.SetToWatch(bodHbv2183);
            tstNext = tstAdd();
            tibStmt368.SetNextTst(tstNext);
        }
    }
}
