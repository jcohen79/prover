using System.Collections.Generic;
using System.Diagnostics;

namespace reslab.TestUtil
{

    // Phases
    //	6:     (((= C(F B   A)) ) )        (= C(F B A))   
    //~12:    {(((=  B(F C @0) )))}        (= B(F C A))   swap 1st   @0~A
    // ~9        (= @0 (F @1 B))           (= A(F C B)    swap 2nd   @1~C
    // ?         (= @1 (F @0 @2))                                    @2~B
    // 5         (= C(F A    B) )          (= C(F A B) )  swap 1st

    // Swap 1st
    //  3:  (() (= (F @0 (F @1 @2)) (F (F @0 @1) @2) ))                  right of epu
    //                   +-------+
    //  4:               (F @0 @0)                      @0 ~ @1,@2 
    //                   => E
    //             +--------------+
    //  2:         (F  @0 E       )
    //             => @0                              @0 ~C
    //                                 +------+
    // Hyp 12:                         (F C @0)     @0~A
    //                                => B
    //Rhs of the eqs, from resolution:
    //    6: (((=    C              (F   B       A)) ) )                        left of epu




    // Swap 2st
    //  3:  (() (= (F @0 (F @1 @2)) (F (F @0 @1) @2) ))
    //                    +------+                                  @0~C
    // Hyp 9:             (F  C B)        C   C  B                @1~C, @2~B
    //                    =>  A
    //                                 +-------+
    //  4:                             (F @0 @0)                   @0 ~ @1,@2 
    //                                 => E
    //                              +--------------+
    //  2:                          (F E         @0)
    //                              => @0                          @0 ~B
    //Rhs of the eqs, from resolution:
    //    12: (((= (F C   A)          B) ) )


    // Swap 3rd
    //  3:  (() (= (F @0 (F @1 @2)) (F (F @0 @1) @2) ))                  right of epu
    //                   +-------+
    //  4:               (F @0 @0)                      @0 ~ @1,@2 
    //                   => E
    //             +--------------+
    //  2:         (F  @0 E       )
    //             => @0                              @0 ~C
    //                                 +------+
    // Hyp 12:                         (F C @0)     @0~A           apply pti of Ax5: (= (F A B) C)
    //                                => B
    //Rhs of the eqs, from resolution:
    //    6: (((=    @0              (F  @1        @2)) ) )                        left of epu

    public interface Ibo
    {
        Pou pouBuildOutline(Iss iss);
    }

    /// <summary>
    /// Temporarily here until TfcEx2Phase is split up
    /// </summary>


    public abstract class TfcEx2Top : TfcTop, Ipa, Iss
    {

        public PouEx2 pou;

        public Eqs eqsSubForPti2First;

        public TfcEx2Top(Tfm tfm, PouEx2 pou) : base(tfm, pou)
        {
            this.pou = pou;

        }

        public abstract Bid bidGet(Vrf ibdInput);

        public override void Init()
        {
            tfm.res.ipaPtiAdded = this;
        }

        public void PtiAdded(Pti ptiNew)
        {
            Res res = tfm.res;
            string stLabel = null;
            Vhd<Pti> vpti = null;
            bool fDoSetAllowed = false;

            if (ptiNew.ascB == res.rgascAxioms[2] && ptiNew.nFromOffset == 3)
            {
                vpti = pou.vpti2LtR;
                stLabel = "pti2LtR";
            }
            else if (ptiNew.ascB == res.rgascAxioms[4] && ptiNew.nFromOffset == 3)
            {
                vpti = pou.vpti4LtR;
                stLabel = "pti4LtR";
            }
            else if (ptiNew.ascB == res.rgascAxioms[5] && ptiNew.nFromOffset == 4)
            {
                fDoSetAllowed = true;
                vpti = pou.vpti5RtL;
                stLabel = "pti5RtL";
            }
            else if (ptiNew.ascB == res.rgascAxioms[6] && ptiNew.nFromOffset == 3)
            {
                vpti = pou.vpti6LtR;
                stLabel = "pti6LtR";
            }
            else
                return;

            tfm.SetVhd(Tin.tinPti, vpti, ptiNew);
      //      if (tfm.tscOutput != null)
      //          tfm.tscOutput.SetIdentifier(vpti.tGet(), stLabel);
            if (fDoSetAllowed )
                SetPtiAllowEprNonVbl(vpti.tGet());
            RegisterPreset (ptiNew, stLabel);
            pou.ivbGet<Pti>(stLabel).SetV(ptiNew);    // TODO: move from  ctor to here
        }

        public override void ReadyToRun(Res res)
        {
            pou.vnin2.Set(Tcd.nin2);
            pou.vnin3.Set(Tcd.nin3);
            pou.vnin4.Set(Tcd.nin4);

            Tsc tsc = tfm.tscOutput;
            if (tsc != null)
            {
                tsc.tifCurrent = null;

             //   tsc.SetIdentifier(Tcd.nin2, "Tcd.nin2");
            //    tsc.SetIdentifier(Tcd.nin3, "Tcd.nin3");

                Bod.bodMake(tsc, Tcd.nin2, KCapture.kPreset).stVarName = "nin2";
                Bod.bodMake(tsc, Tcd.nin3, KCapture.kPreset).stVarName = "nin3";
                Bod.bodMake(tsc, Tcd.nin4, KCapture.kPreset).stVarName = "nin4";

                Bod.bodMake(tsc, pou.vasc3.tGet(), KCapture.kPreset).stVarName = "ax3";
                Bod.bodMake(tsc, pou.vasc6.tGet(), KCapture.kPreset).stVarName = "ax6";
                Bod.bodMake(tsc, pou.vascR.tGet(), KCapture.kPreset).stVarName = "axR";

                tfm.tscOutput.NextStep(this, pou.psbFirst, null, null, null, null);
            }
            pou.psbFirst.fPerform(this, this);
        }

        public override void SetRes(Res res)
        {
            pou.SetRes(res);
        }

    }

    public class PouEx2 : Pou, Ips
    {

        public const int nAx3Index = 3;
        public const int nAx5Index = 5;
        public const int nAx6Index = 8;
        public const int nAxSymIndex = 15;
        public const int nAx12Index = 9;
        public const int nAxRIndex = 6;

        public Vasc vasc3;
        public Vasc vasc6;
        public Vasc vascR;
        public Vasc vascSym;

        public Vhd<Asc> vascNgcFC0B;
        public Vhd<Asc> vascNgcF012;
        public Vhd<Asc> vasc0_F12;

        public Vhd<Pti> vpti2LtR;
        public Vhd<Pti> vpti4LtR;
        public Vhd<Pti> vpti5RtL;
        public Vhd<Pti> vpti6LtR;

        public Vhd<Nin> vnin2;
        public Vhd<Nin> vnin3;
        public Vhd<Nin> vnin4;

        List<Inr> rginr = new List<Inr>();

        public PouEx2(string stClassName) : base(stClassName) 
        {
            vascNgcFC0B = AddVhd(new Vhd<Asc>("NgcFC0B"));
            vascNgcF012 = AddVhd(new Vhd<Asc>("NgcF012"));
            vasc0_F12 = AddVhd(new Vhd<Asc>("0_F12"));

            vpti2LtR = AddVhd(new Vhd<Pti>("pti2LtR"));
            vpti4LtR = AddVhd(new Vhd<Pti>("pti4LtR"));
            vpti5RtL = AddVhd(new Vhd<Pti>("pti5RtL"));
            vpti6LtR = AddVhd(new Vhd<Pti>("pti6LtR"));

            vnin2 = AddVhd(new Vhd<Nin>("nin2"));
            vnin3 = AddVhd(new Vhd<Nin>("nin3"));
            vnin4 = AddVhd(new Vhd<Nin>("nin4"));

            vasc3 = AddVhd (new Vasc(this, "ax3", nAx3Index));
            vasc6 = AddVhd(new Vasc(this, "ax6", nAx6Index));
            vascR = AddVhd (new Vasc(this, "axR", nAxRIndex));
            
         //   vascSym = AddVhd (new Vasc(this, "axSym", nAxSymIndex));
            /*
            Rpo rpoFirst = Add(new Rpo(vasc3, vasc6));
            Rpo rpoSecond = Add(new Rpo(vasc3, vascNgcFC0B));
            Rpo rpoReverseSecond = Add(new Rpo(vascR, vascNgcF012));
            Rpo rpoThird = Add(new Rpo(vasc3, vasc0_F12));
            Rpo rpoFourth = Add(new Rpo(vasc3, vascNgcFC0B));
            */
        }

        public void SetRes(Res res)
        {
            foreach (Inr inr in rginr)
                inr.SetRes(res);
        }

        public void AddInr(Inr inr)
        {
            rginr.Add(inr);
        }
    }

    public abstract class TfcBuilt : TfcEx2Top, Iss, Irs
    {
        public Pou pouOutline;
        Dictionary<long, Bid> mpn_bidLatest = new Dictionary<long, Bid>();

        int nMaxStep = -1;

        public TfcBuilt(Tfm tfm, PouEx2 pou) : base(tfm, pou)
        {
            pouOutline = pou;
            pouBuildOutline(this);
        }

        public abstract Pou pouBuildOutline(Iss iss);

        void StepHeading(Tfc tfcPhase, Psb psbNext, object objLeft, object objRight, object objData, Tcd tcdEvent)
        {
            Debug.WriteLine("step " +
                 " " + tcdEvent.stName + " " + psbNext.nId + " (max " + nMaxStep + ")\n\tleft: " + objLeft + "\n\tright: " + objRight + "\n\tdata: " + objData);

            if (psbNext.nId > nMaxStep)
                nMaxStep = psbNext.nId;

        }


        public override bool fNextStep(Tfc tfcPhase, Psb psbNext, object objLeft, object objRight, object objData, Tcd tcdEvent)
        {
            StepHeading(tfcPhase, psbNext, objLeft, objRight, objData, tcdEvent);
            if (tfm.tscOutput != null)
                tfm.tscOutput.NextStep(tfcPhase, psbNext, objLeft, objRight, objData, tcdEvent);

            if (objLeft != null)
                Register(KCapture.kLeft, (Bid)objLeft, psbNext);
            if (objRight != null)
                Register(KCapture.kRight, (Bid)objRight, psbNext);
            if (objData != null)
                Register(KCapture.kData, (Bid)objData, psbNext);

            return psbNext.fPerform(this, tfcPhase);
        }

        private void RegisterCommon(KCapture kSource, Bid bid)
        {
            if (bid != null)
                mpn_bidLatest[bid.nId] = bid;

        }

        public override void RegisterPreset(Bid bid, string stLabel)
        {
            RegisterCommon(KCapture.kPreset, bid);
            // kPreset causes use of stLabel as nickname to do lookup
            Bod bod = Bod.bodMake(tfm.tscOutput, bid, KCapture.kPreset);
            if (bod != null)
            {
                bod.stVarName = stLabel;
                bod.kUsed = Bod.KUsage.kObtained;
            }
        }

        public override void Register(KCapture kSource, Bid bid, Isb isbNext)
        {
            RegisterCommon(kSource, bid);

            if (tfm.tscOutput != null)
            {
                Bod bod = Bod.bodMake(tfm.tscOutput, bid, kSource);
                tfm.tscOutput.SaveValue(kSource, bid, bod);
            }

            if (isbNext != null
                && kSource != KCapture.kResult)   // handled in Toa.fPeform
            {
                Psb psbNext = (Psb)isbNext;
                for (Pst pstAction = psbNext.pobHead; pstAction != null; pstAction = pstAction.pobNext)
                {
                    if (pstAction is Tod)
                    {
                        Tod todAction = (Tod)pstAction;
                        if (todAction.kKind == kSource)
                        {
                            todAction.hbvPlace.bidValue = bid;
                            break;
                        }
                    }
                }
            }
        }

        public override Bid bidGet(Vrf ibdInput)
        {
            if (ibdInput == null)
                return null;
            return ibdInput.bidGetValue();
        }

        public void Reset()
        {
            mpn_bidLatest.Clear();
            RegisterPreset(Tcd.nin2, "nin2");
            RegisterPreset (Tcd.nin3, "nin3");
        }
    }
}
