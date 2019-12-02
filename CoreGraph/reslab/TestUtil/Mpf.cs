using System;
using System.Collections.Generic;
using System.Text;

namespace reslab.TestUtil
{
    /// <summary>
    /// Pattern factory: create Mpt, group into Mtg, and associate those together using Mgp
    /// </summary>
    public class Mpf
    {
        // Static mtg_s that are referenced by each of the Mip_s in patterns
        public static Mtg mtgProof = new Mtg();
        public static Mtg mtgPhase = new Mtg();
        public static Mtg mtgEqs = new Mtg();

        public static Mds mdsExit = new Mds("exit");
        Mpt mptProof = new Mpt("proof");
        Pou pou;

        /// <summary>
        /// Create the Mpt used to match proof outlines
        /// </summary>
        public static void Build(Pou pouOutline)
        {
            Mpf mpf = new Mpf();
            mpf.pou = pouOutline;
            mpf.AddAll(pouOutline);
        }

        void AddAll(Pou pouOutline)
        {
            // build top level steps of proof
            Mdv mdvPrev = null;
            for (int i = 0; i < 5; i++)   // number of phases
            {
                Mpr mpr = new Mpr(mtgPhase);
                Mdv mdv = new Mdv();
                mdv.mdsSymbol = mdsExit;
                mpr.mdvFirstBranch = mdv;
                if (mdvPrev != null)
                {
                    mdvPrev.psbValue = mpr;
                    mdvPrev.mdvNext = mdv;
                }
                else
                    mptProof.psbFirst = mpr;
            }
            Tst tstFinal = new Tst();
            mdvPrev.psbValue = tstFinal;   // Tst for terminate proof: add action to verify

            MpResolve tpRes1 = new MpResolve(pouOutline, "ax3", "ax6");
            MpResolve tpRes2 = new MpResolve(pouOutline, "ax3", "NgcFC0B");
            MpResolve tpRes3 = new MpResolve(pouOutline, "axR", "NgcF012");
            MpResolve tpRes4 = new MpResolve(pouOutline, "ax3", "0_F12");
            MpResolve tpRes5 = new MpResolve(pouOutline, "ax3", "NgcFC0B");  // wrong

            mtgProof.AddMpt(mptProof);
            mtgPhase.AddMpt(tpRes1);
            mtgPhase.AddMpt(tpRes2);
            mtgPhase.AddMpt(tpRes3);
            mtgPhase.AddMpt(tpRes4);
            mtgPhase.AddMpt(tpRes5);

            // TODO add steps to each phase

            // used in TfcBuiltTree
            MpEtp mpEtp = new MpEtp(pou, 2);

        }
    }



    /// <summary>
    /// Base class to test proof structure: build a tree that is used to build linear sequence of Tst.
    /// Derived classes create test step and action sequences based on function that generates that based on parameters
    /// </summary>
    public abstract class Mpb : Mpt
    {
        static int cId = 0;
        int nId = cId++;

        protected Mpb(string stName) : base(stName)
        {
        }

        /// <summary>
        /// Add a Tst to the outline
        /// </summary>
        protected Tst tstAdd(Tob tobPrev)
        {
            Tst tst = new Tst();
            if (tobPrev != null)
                tobPrev.SetNextTst(tst);
            return tst;
        }

        /// <summary>
        /// Add a location to the outline for a pattern to be selected
        /// </summary>
        protected Mpr mprAdd(Mtg mtg, Tob tobPrev)
        {
            Mpr mpr = new Mpr(mtg);
            tobPrev.SetNextTst(mpr);
            return mpr;
        }

        protected Tst tstMakeFirst()
        {
            Tst tst = new Tst();
            psbFirst = tst;
            return tst;
        }

        /// <summary>
        /// Construct the test sequence based on the parameters that have been set up.
        /// Will leave gaps where lower level Tpb_s can be build as needed.
        /// for each gap, the derived class provides an InsertXXX method to pass in the parameters from the parent
        /// </summary>


        public void NotePti(Pou pouOutline, Tst tstCurrent)
        {

            // TODO: make this a Mpt, add Mpr somewhere? or just call it?
            Toh<Pti> tibStmt4 = new Toh<Pti>(tstCurrent);
            tibStmt4.SetName("pti2LtR");
            tibStmt4.SetTinName("Pti");
            tibStmt4.SetValue(pouOutline.vrfGet<Pti>("pti2LtR"));
            Toh<Pti> tibStmt5 = new Toh<Pti>(tstCurrent);
            tibStmt5.SetName("pti4LtR");
            tibStmt5.SetTinName("Pti");
            tibStmt5.SetValue(pouOutline.vrfGet<Pti>("pti4LtR"));
            Toh<Pti> tibStmt6 = new Toh<Pti>(tstCurrent);
            tibStmt6.SetName("pti5RtL");
            tibStmt6.SetTinName("Pti");
            tibStmt6.SetValue(pouOutline.vrfGet<Pti>("pti5RtL"));
        }

    }

    /// <summary>
    /// Root of subtree of Tpb for resolving two clauses
    /// </summary>
    public class MpResolve : Mpb
    {
        string stLeft;
        string stRight;

        Hbv<Epu> bodHbv154 = new Hbv<Epu>();
        Hbv<Atp> bodHbv155 = new Hbv<Atp>();
        Hbv<Eqs> bodHbv156 = new Hbv<Eqs>();

        public bool fNeedWatch156 = false;

        public MpResolve(Pou pouOutline, string stLeft, string stRight) : base(stLeft + ":" + stRight)
        {
            this.stLeft = stLeft;
            this.stRight = stRight;

            Tos tibStmt3 = new Tos(tstMakeFirst());
            tibStmt3.SetLeft(pouOutline.ivbGet<Asc>(stLeft));
            tibStmt3.SetRight(pouOutline.ivbGet<Asc>(stRight));
            Tst tstNext = tstAdd(null);
            tibStmt3.SetNextTst(tstNext);

            Tod tibStmt6 = new Tod(tstNext);
            tibStmt6.kKind = KCapture.kData;
            tibStmt6.hbvPlace = bodHbv154;
            Top tibStmt7 = new Top(tstNext);
            tibStmt7.SetTarget(bodHbv154);
            tibStmt7.SetInput(null);
            tibStmt7.SetTcd(Tcd.tcdEqsForEpu);
            tstNext = tstAdd(tibStmt7);
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
            Tob tobPrev = tibStmt12;

            if (fNeedWatch156)
            {
                Tow tibStmt13 = new Tow(tstNext);
                tibStmt13.SetToWatch(bodHbv156);
                tobPrev = tibStmt13;
            }
            Mpr mprEqs = mprAdd(Mpf.mtgEqs, tobPrev);

            // TODO how to bind the exit from pattern matched to mprEqs to parent level exit?
        }
    }
    /// <summary>
    /// Root of subtree for solving Eul/Eur for Pti
    /// </summary>
    public class MpEul : Mpb
    {
        public Hbv<Eqs> hbvEqsInput;
        public Hbv<Eqs> hbvEqsParent;
        public string stPtiName;
        Hbv<Eul> bodHbv2027 = new Hbv<Eul>();
        Hbv<Vbv> bodHbv2262 = new Hbv<Vbv>();
        Hbv<Eur> bodHbv2268 = new Hbv<Eur>();
        Hbv<Eqs> bodHbv2029 = new Hbv<Eqs>();

        public MpEul(Pou pouOutline) : base("eul") 
        {
            Tst tstNext = tstMakeFirst();

            Top tibStmt30 = new Top(tstNext);
            tibStmt30.SetTarget(hbvEqsInput);
            tibStmt30.SetInput(pouOutline.vrfGet<Pti>(stPtiName));
            tibStmt30.SetTcd(Tcd.tcdApplyPtiToEqs);
            tstNext = tstAdd(tibStmt30);
            tibStmt30.SetLabel("find eul for " + stPtiName);
            Tod tibStmt107 = new Tod(tstNext);
            tibStmt107.kKind = KCapture.kData;
            tibStmt107.hbvPlace = bodHbv2027;
            Tog tibStmt108 = new Tog(tstNext);
            tibStmt108.SetTarget(bodHbv2027);
            tibStmt108.SetTcd(Tcd.tcdLaunchEur);
            tstNext = tstAdd(tibStmt108);
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
    }

    /// <summary>
    /// Hold info for each child of TpEtp
    /// </summary>
    public class MHlpEtp
    {
        public Hbv<Etp> bodHbv158 = new Hbv<Etp>();
        public bool fNeedWatch160 = false;
        public bool fSkipEqs = false;
        public bool fSkipTerm = false;
    }

    /// <summary>
    /// Root of subtree for solving Eqs
    /// </summary>
    public class MpEtp : Mpb
    {
        public Hbv<Eqs> hbvEqsInput;
        public Hbv<Eur> hbvEurInput;
        public Hbv<Eqs> hbvEqsParent;
        int nNumSubterms;
        MHlpEtp[] rghlpEtpSubterms;
        Tst tstNext;
        Pou pouOutline;

        public MpEtp(Pou pouOutline, int nNumSubterms) : base("etp")
        {
            this.pouOutline = pouOutline;
            SetNumSubterms(nNumSubterms);
            int nPos = 0;
            Mpr mprPrev = null;
            foreach (MHlpEtp hlpEtp in rghlpEtpSubterms)
            {
                if (!hlpEtp.fSkipTerm)
                {
                    mprPrev = mprBuildTerm(nPos, hlpEtp, mprPrev);
                }
                nPos++;
            }
            mprPrev.AddMdv(Mpf.mdsExit, Mpf.mdsExit);  // map exit referenced in child to exit to parent
        }

        public void SetNumSubterms(int nNumSubterms)
        {
            this.nNumSubterms = nNumSubterms;
            rghlpEtpSubterms = new MHlpEtp[nNumSubterms];
            for (int i = 0; i < nNumSubterms; i++)
            {
                MHlpEtp hlpEtp = new MHlpEtp();
                rghlpEtpSubterms[i] = hlpEtp;
            }
        }

        public MHlpEtp hlpChild(int n)
        {
            return rghlpEtpSubterms[n];

        }

        Mpr mprBuildTerm(int nPos, MHlpEtp hlpEtp, Mpr mprPrev)
        {
            if (mprPrev == null)
                tstNext = tstMakeFirst();
            else
            {
                tstNext = tstAdd(null);
                mprPrev.AddMdv(Mpf.mdsExit, tstNext);
            }
            
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

            tstNext = tstAdd(tibStmt23);
            if (hlpEtp.fSkipEqs)
            {
                return mprPrev;
            }
            else
            {
                Hbv<Eqs> bodHbv160 = new Hbv<Eqs>();

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
                Tob tobPrev = tibStmt29;
                if (hlpEtp.fNeedWatch160)
                {
                    Tow tibStmt226 = new Tow(tstNext);
                    tibStmt226.SetToWatch(bodHbv160);
                    tobPrev = tibStmt29;
                }
                Mpr mprEqs = mprAdd(Mpf.mtgEqs, tobPrev);
                return mprEqs;
                // TODO: make gap to apply the pti for this child. see old outline PouEx2.cs
                    
            }


        }
    }

    /// <summary>
    /// Base class for getting the eqs for Eub
    /// </summary>
    public abstract class MpGetEqsB : Mpb
    {
        public Hbv<Eul> hbvEulInput;
        public Hbv<Eur> hbvEurInput;
        public Hbv<Eqs> hbvEqsOutput;

        protected MpGetEqsB(string stName) : base(stName)
        {
        }

        protected void AddEqs(Tst tstNext)
        {
        }
    }

    /// <summary>
    /// Ad hoc for now to get eqs for pti2 in phase 1
    /// </summary>
    public class MpGetEqsPhase1Pti2 : MpGetEqsB
    {
        public MpGetEqsPhase1Pti2() : base("MpGetEqsPhase1Pti2") { }

        public  void Build()
        {
            hbvEqsOutput = new Hbv<Eqs>();

            Tst tstNext = tstMakeFirst();
            Toa tibStmt119 = new Toa(tstNext);
            tibStmt119.SetTff(TEqsSub.tff);
            tibStmt119.SetResult(hbvEqsOutput);
            tibStmt119.SetArgs(hbvEulInput);
            Tod tibStmt120 = new Tod(tstNext);
            tibStmt120.kKind = KCapture.kResult;
            tibStmt120.hbvPlace = hbvEqsOutput;    // is this needed?
            Tow tibStmt121 = new Tow(tstNext);     // TODO: check flag
            tibStmt121.SetToWatch(hbvEqsOutput);
            tstNext = tstAdd(tibStmt121);

            AddEqs(tstNext);

        }
    }

    /// <summary>
    /// Base class for return result to Etp for Eur
    /// </summary>
    public abstract class MpEtpUpB : Mpb
    {
        public Hbv<Eur> hbvEurInput;   // Eur#2147 3->6 in (nil  (=  (F  @0 @0) E))
        public Hbv<Eqs> hbvEqsInput;  // from TpEul.hbvEqsInput
        public Hbv<Eqs> hbvEqsOutput;
        public Hbv<Eqs> bodHbv2029;  // what is this?
        public Hbv<Etp> hbvEtp;   // bodHbv2036 Etp#2036 3,8 (((F  @0 (F  @1 @2))) ((F  @3 E))), set in tibStmt223


        protected MpEtpUpB(string stName) : base(stName)
        {
        }

        protected void AddEqs(Tst tstNext)
        {
        }
    }


    /// <summary>
    /// Apply trivial Vbv from (F @0 @0) vs (F @1 @2) to etp that was used
    /// </summary>
    public class MpGetEqsPhase1Pti4Up : MpEtpUpB
    {
        public MpGetEqsPhase1Pti4Up() : base("MpGetEqsPhase1Pti4Up") { }

        Hbv<Eqs> bodHbv491 = new Hbv<Eqs>();
        Hbv<Vbv> bodHbv2154 = new Hbv<Vbv>();
        Hbv<Vbv> bodHbv2183 = new Hbv<Vbv>();


        public  void Build()
        {
            Tst tstNext = tstMakeFirst();
            Toi tibStmt342 = new Toi(tstNext);
            tibStmt342.SetInput(hbvEurInput); // Eur#2147 3->6 in (nil  (=  (F  @0 @0) E))
            tibStmt342.SetTcd(Tcd.tcdTransferLeftEqs);
            tstNext = tstAdd(tibStmt342);
            tibStmt342.SetNextTst(tstNext);
            Tod tibStmt343 = new Tod(tstNext);
            tibStmt343.kKind = KCapture.kLeft;
            tibStmt343.hbvPlace = bodHbv491;
            Toi tibStmt345 = new Toi(tstNext);
            tibStmt345.SetInput(bodHbv491);
            tibStmt345.SetTcd(Tcd.tcdEqsToEnt);
            Tst tstNext2 = tstAdd(tibStmt345);
            Top tibStmt346 = new Top(tstNext);
            tibStmt346.SetTarget(hbvEurInput);
            tibStmt346.SetInput(bodHbv491);
            tibStmt346.SetTcd(Tcd.tcdEqsToEnt);
            tstNext = tstAdd(tibStmt346);
            tibStmt346.SetLabel("solns coming from eqsEE to pti4");
            Top tibStmt353 = new Top(tstNext);
            tibStmt353.SetTarget(hbvEqsInput);// eqsㅕ2038 (((F  @0 @1)) (E))ㅑ
            tibStmt353.SetInput(hbvEurInput);
            tibStmt353.SetTcd(Tcd.tcdEurToNotifyEqs);
            tstNext = tstAdd(tibStmt353);
            tibStmt353.SetLabel("soln from eqsEE using pti4 to {{eqs#8103 (((F  @0 @1)) (E))}");

            Tod tibStmt356 = new Tod(tstNext);
            tibStmt356.kKind = KCapture.kData;
            tibStmt356.hbvPlace = bodHbv2154;
            Top tibStmt357 = new Top(tstNext);
            tibStmt357.SetTarget(hbvEtp);   // Etp#2036 3,8 (((F  @0 (F  @1 @2))) ((F  @3 E)))
            tibStmt357.SetInput(bodHbv2154);
            tibStmt357.SetTcd(Tcd.tcdSolnToEqs);
            tstNext = tstAdd(tibStmt357);
            tibStmt357.SetLabel("process soln from eqsEE using pti4 to {{eqs#8103 (((F  @0 @1)) (E))}");
            Top tibStmt362 = new Top(tstNext);
            tibStmt362.SetTarget(hbvEtp);	// Etp#2036 3,8 (((F  @0 (F  @1 @2))) ((F  @3 E)))
            tibStmt362.SetInput(bodHbv2154);
            tibStmt362.SetTcd(Tcd.tcdVbvForEtp);
            tstNext = tstAdd(tibStmt362);
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
            tstNext = tstAdd(tibStmt368);
        }
    }
}

