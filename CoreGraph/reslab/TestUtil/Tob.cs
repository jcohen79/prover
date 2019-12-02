using GraphMatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab.TestUtil
{
    /// <summary>
    /// Hold info to identify and describe a Bid
    /// </summary>
    public class Bod : Ibd
    {
        public readonly int nId;
        public readonly long nBidId;
        public Type typeOfSrc;
        public readonly string stDescSrc;
        public KUsage kUsed = KUsage.kUnused;
        public KCapture kSourceChannel;
        public string stVarName;

        public enum KUsage
        {
            kUnused,
            kNeeded,
            kObtained
        }

        public static Bod bodMake(Tsc tsc, Ibd bid)
        {
            return bodMake(tsc, bid, KCapture.kUsage);
        }
        
        public static Bod bodMake(Tsc tsc, Ibd bid, KCapture kSource)
        {
            if (bid == null || tsc == null)
                return null;
            Bod bod = null;
            if (!tsc.mpn_bodBidUsage.TryGetValue(bid.nGetId(), out bod))
            {
                if (kSource == KCapture.kUsage)
                    throw new InvalidProgramException("bid used without being received");
                bod = new Bod(bid);
                tsc.mpn_bodBidUsage[bid.nGetId()] = bod;
                bod.kSourceChannel = kSource;
                bod.typeOfSrc = bid.GetType();
            }
            else if (kSource == KCapture.kUsage)
            {
                if (bod.kUsed == Bod.KUsage.kUnused)
                {
                    bod.kUsed = Bod.KUsage.kNeeded;
                    bod.AssignName();
                }
            }
            return bod;
        }

        protected Bod (Ibd bid)
        {
            this.nBidId = bid.nGetId();
            this.typeOfSrc = bid.typeOf();
            this.stDescSrc = bid.ToString(); // bid.stDesc(tsc);
        }

        /// <summary>
        /// This is going to be used, so assign name
        /// </summary>
        public void AssignName()
        {
            stVarName = "bodHbv" + nBidId;
        }

        public long nGetId()
        {
            return nBidId;
        }

        public Type typeOf()
        {
            return typeOfSrc;
        }

        public string stDesc(Tsc tsc)
        {
            return stDescSrc;
        }

        public override string ToString()
        {
            return stDescSrc;
        }
    }


    /// <summary>
    /// Outline element for a step. Each step has a sequence of actions (Tob)
    /// </summary>
    public class Tst : Psb
    {

        public Tst()
        {
        }

        public Tst(Pou puoOutline)
        {
            // only need to capture first step for outline
            puoOutline.Add(this); 
        }
    }


    /// <summary>
    /// Base class for actions that are built in the generated outline.
    /// Each makes calls to test harness to watch prover.
    /// </summary>
    public abstract class Tob : Pst
    {
        public Psb psbNext;  // next stmt
 
        protected Tob (Psb psbStep)
        {
            if (psbStep != null)
                psbStep.Add(this);
        }

        public void SetNextTst(Psb psbNext)
        {
            this.psbNext = psbNext;
        }

        public abstract void GenerateActionStmt(Fwt fwt, Tsc tsc);

        /// <summary>
        /// Make calls to test harness to watch steps in prover
        /// </summary>
        /// <param name="tfc"></param>
        public override void PerformStep(Tfc tfc)
        {
            // move to subclasses: or just call the tif object?
            throw new NotImplementedException();
        }

        public override string stTypeName()
        {
            return GetType().Name;
        }

    }

    /// <summary>
    /// Place to hold value saved and used according to Bod, Tod, Tid
    /// Allows passing location into functions.
    /// </summary>
    public class Hbb : Vrf
    {
        public Bid bidValue;
        public Bid bidGetValue()
        {
            return bidValue;
        }

        public override string ToString()
        {
            return "hbv_" + bidValue;
        }
    }

    public class Hbv<T> : Hbb where T:Bid
    {
        public T tGet() { return (T)bidValue;  }
        public void Set(T tV) { bidValue = tV; }
    }

    /// <summary>
    /// Save a result found during proof to a variable
    /// </summary>
    public class Tod: Tob
    {
        public KCapture kKind;
        public Hbb hbvPlace;   // for runtime (use setter)

        public Tod(Psb psbStep) : base(psbStep)
        {
        }

        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Nothing is done here, the Hbv are set by scan through actions when value is obtained
        /// This object is a placeholder to the hbb (which is also a placeholder)
        /// </summary>
        public override bool fPerform(Iss iss, Tfc tfc, Psb psb)
        { 
            //  hbvPlace.bidValue = 
            return true;
        }
    }

    /// <summary>
    /// represents WatchPair, constructed by generated proof outline code
    /// </summary>
    public class Top: Tob
    {
        public Vrf ibdTarget;
        public Vrf ibdInput;
        public Tcd tcd;
        public Tif tifNextStep;
        public string stLabel;

        public Top(Psb psbStep) : base(psbStep) { }

        public void SetTcd(Tcd tcd) { this.tcd = tcd; }
        public void SetTarget(Vrf ibdTarget) { this.ibdTarget = ibdTarget; }
        public void SetInput(Vrf ibdInput) { this.ibdInput = ibdInput; }
        public void SetLabel(string stLabel) { this.stLabel = stLabel; }



        public override bool fPerform(Iss iss, Tfc tfc, Psb psb)
        {
            Bid bidTarget = iss.bidGet(ibdTarget);
            Bid bidInput = iss.bidGet(ibdInput);
            tfc.WatchPair(bidTarget, bidInput, tcd, stLabel, psbNext);
            return true;
        }

        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// WatchTarget
    /// </summary>
    public class Tog: Tob
    {
        public Vrf ibdTarget;
        public Tcd tcd;

        public Tog(Psb psbStep) : base(psbStep) { }

        public void SetTcd(Tcd tcd) { this.tcd = tcd; }
        public void SetTarget(Vrf ibdTarget) { this.ibdTarget = ibdTarget; }

        public override bool fPerform(Iss iss, Tfc tfc, Psb psb)
        {
            Bid bidTarget = iss.bidGet(ibdTarget);
            tfc.WatchTarget(bidTarget, tcd, "", psbNext);
            return true;
        }



        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        { /*
            fwt.AppendLine("// " + tsc.stIdentifier(bidTarget) + ": " + bidTarget);
            fwt.AppendLine("WatchTarget ("
                + tsc.stIdentifier(bidTarget) + ", "
                + "Tcd." + tcd.stName + ", "
                + "\"\", "
                + "(int) " + stKStepPrefix + tifNextStep.stStep + ");");*/
        }
    }


    // Start processing a pair of Asc
    public class Tos: Tob
    {
        public Ivh<Asc> hascLeft;
        public Ivh<Asc> hascRight;
        public Tsp tspPhase;
        
        public Tos(Psb psbStep) : base(psbStep)
        {
        }

        public void SetLeft(Ivh<Asc> hascLeft) { this.hascLeft = hascLeft; }
        public void SetRight(Ivh<Asc> hascRight) { this.hascRight = hascRight; }

        public override bool fPerform(Iss iss, Tfc tfc, Psb psb)
        {
            Tfm tfm = tfc.tfm;
            if (tfm.tscOutput != null)
            {
                tfm.tscOutput.StartPhase(hascLeft, hascRight, tfc, psbNext);
            }

            Asc ascLeft = hascLeft.tGet();
            Asc ascRight = hascRight.tGet();
            Tsc tscSave = tfc.tfm.tscOutput;
            tfc.tfm.tscOutput = null;  // turn off generation of new actions, they are registered elsewhere
            if (tspPhase == null)
            {
                tspPhase = new Tsp(tfc, ascLeft, ascRight, psbNext);
                Bid.AddIsp(tspPhase);
            }
            else
            {
                tspPhase.ascLeft = ascLeft;
                tspPhase.ascRight = ascRight;
                if (tspPhase.fGetImmediate())
                    tspPhase.Perform();
            }
            tfc.tfm.tscOutput = tscSave;
            return true;
        }

 

        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        { /*
            fwt.AppendLine("// " + tsc.stIdentifier(bidLeft) + ": " + bidLeft);
            fwt.AppendLine("// " + tsc.stIdentifier(bidRight) + ": " + bidRight);
            fwt.AppendLine("StartPair (ref "
                + stTspName + ", "
                + tsc.stIdentifier(bidLeft) + ", "
                + tsc.stIdentifier(bidRight) + ", "
                + "(int) " + stKStepPrefix + tifNextStep.stStep + ");");
            Vdi vdi = new Vdi(typeof(Tsp), stTspName, true);
            tsc.AddVdi(vdi);*/
        }
    }


    /// <summary>
    /// Watch Input
    /// </summary>
    public class Toi: Tob
    {
        public Vrf ibdInput;
        public Tcd tcd;

        public Toi(Psb psbStep) : base(psbStep) { }

        public void SetTcd(Tcd tcd) { this.tcd = tcd; }
        public void SetInput(Vrf ibdInput) { this.ibdInput = ibdInput; }

        public override bool fPerform(Iss iss, Tfc tfc, Psb psb)
        {
            Bid bidInput = iss.bidGet(ibdInput);
            tfc.tfm.WatchInput(tfc, bidInput, tcd, "", psbNext);
            return true;
        }



        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        { /*
            fwt.AppendLine("// " + tsc.stIdentifier(bidInput) + ": " + bidInput);
            fwt.AppendLine("WatchInput ("
                + tsc.stIdentifier(bidInput) + ", "
                + "Tcd." + tcd.stName + ", "
                + "\"\", "
                + "(int) " + stKStepPrefix + tifNextStep.stStep + ");"); */
        }
    }

    /// <summary>
    /// Reset and watch variable, if not already
    /// </summary>
    public class Tow: Tob
    {
        public Vrf ibdToWatch;

        public Tow(Psb psbStep) : base(psbStep) { }
        public void SetToWatch(Vrf ibdToWatch) { this.ibdToWatch = ibdToWatch; }

        public override bool fPerform(Iss iss, Tfc tfc, Psb psb)
        {
            Bid bidToWatch = iss.bidGet(ibdToWatch);
            if (tfc.fWatch(bidToWatch, psbNext, ""))
                tfc.Restart();
            return true;
        }



        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        { /*
            fwt.AppendLine("// " + tsc.stIdentifier(bidToWatch) + ": " + bidToWatch);
            fwt.AppendLine("if (fWatch ("
                + tsc.stIdentifier(bidToWatch) + ", "
                + "(int) " + stKStepPrefix + tifNextStep.stStep + ", "
                + "\"\"))");
            fwt.Indent();
            fwt.AppendLine("Restart();");
            fwt.Unindent();*/
        }
    }

    public class Tov: Tob
    {
        public Vrf ibdValue;
        public KTis kValue;

        public Tov(Psb psbStep) : base(psbStep) { }
        public void SetValue(Vrf ibdValue) { this.ibdValue = ibdValue; }
        public void SetKValue(KTis kValue) { this.kValue = kValue;  }

        public override bool fPerform(Iss iss, Tfc tfc, Psb psb)
        {
            Bid bidValue = iss.bidGet(ibdValue);
            switch (kValue)
            {
                case KTis.kPtiAllowEprNonVbl:
                    ((Itt)tfc).SetPtiAllowEprNonVbl(bidValue);
                    break;
                case KTis.kEqsAllowEprNonVbl:
                    ((Itt)tfc).SetEqsAllowEprNonVbl(bidValue);
                    break;
                default:
                    throw new InvalidProgramException();  // uninitialized KValue
            }
            return true;
        }



        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        { /*
            fwt.AppendLine("// " + tsc.stIdentifier(bidValue) + ": " + bidValue);
            switch (kValue)
            {
                case KTis.kPtiAllowEprNonVbl:
                    fwt.AppendLine("SetPtiAllowEprNonVbl(" + tsc.stIdentifier(bidValue) + ");");
                    break;
                case KTis.kEqsAllowEprNonVbl:
                    fwt.AppendLine("SetEqsAllowEprNonVbl(" + tsc.stIdentifier(bidValue) + ");");
                    break;
            } */
        }
    }

    /// <summary>
    /// Action to call a function defined on a Tff
    /// </summary>
    public class Toa : Tob
    {
        public Tff tff;
        public Vrf ibdResult;
        public List<Vrf> rgibdfArgs;

        public Toa(Psb psbStep) : base(psbStep) { }

        public void SetResult(Vrf ibdResult) { this.ibdResult = ibdResult; }
        public void SetArgs(Vrf ibdArg)
        {
            if (rgibdfArgs == null)
                rgibdfArgs = new List<Vrf>();
            rgibdfArgs.Add(ibdArg);
        }
        public void SetTff(Tff tff)
        {
            this.tff = tff;
        }

        public override bool fPerform(Iss iss, Tfc tfc, Psb psb)
        {
            // Bid bidResult = iss.bidGet(ibdResult);
            List<Bid> rgbidArgs = new List<Bid>();
            if (rgibdfArgs != null)
            {
                for (int i = 0; i < rgibdfArgs.Count; i++)
                    rgbidArgs.Add(iss.bidGet(rgibdfArgs[i]));
            }

            Bid bidResult = tff.bidInvoke(tfc, rgbidArgs.ToArray());
            iss.Register(KCapture.kResult, bidResult, psb);
            if (ibdResult is Hbb)
            {
                Hbb hbb = (Hbb)ibdResult;
                hbb.bidValue = bidResult;
            }
            return true;
        }



        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        { /*
                      tff.GenerateActionStmt(fwt, tsc, bidResult, rgbifArgs);
            */
        }
    }

    /// <summary>
    /// Set a value for use in another step
    /// </summary>
    public class Toh<T> : Tob where T : Ibd
    {
        public string stVhdName;
        public Vrf ibdValue;
        public Tin tin;

        public Toh(Psb psbStep) : base(psbStep) { }

        public void SetName(string stVhdName)
        {
            this.stVhdName = stVhdName;
        }

        public void SetTinName(string stTinName)
        {
            if (stTinName == Tin.tinAsc.stName)
                tin = Tin.tinAsc;
            else if (stTinName == Tin.tinPti.stName)
                tin = Tin.tinPti;
            else if (stTinName == Tin.tinNgc.stName)
                tin = Tin.tinNgc;
            else
                throw new NotImplementedException();
        }

        public void SetValue(Vrf ibdValue)
        {
            this.ibdValue = ibdValue;
        }

        public override bool fPerform(Iss iss, Tfc tfc, Psb psb)
        {
            TfcEx2Top tfcTop = (TfcEx2Top)tfc; 
            Dictionary<string,Ivb> mpst_ivbValues = tfcTop.pou.mpst_ivbValues;
            Ivb ivbPlace;
            if (!mpst_ivbValues.TryGetValue(stVhdName, out ivbPlace))
            {
                if (ivbPlace == null)
                {
                    if (tin == Tin.tinAsc)
                        ivbPlace = tfcTop.pou.AddVhd (new Vhd<Asc>(stVhdName));
                    else if (tin == Tin.tinPti)
                        ivbPlace = tfcTop.pou.AddVhd (new Vhd<Pti>(stVhdName));
                    else if (tin == Tin.tinNgc)
                        ivbPlace = tfcTop.pou.AddVhd (new Vhd<Ngc>(stVhdName));
                    else
                        throw new NotImplementedException();
                    mpst_ivbValues[stVhdName] = ivbPlace;
                }
            }

            Bid ibdCurrent = ibdValue.bidGetValue();

            ivbPlace.SetV(ibdCurrent);

            if (tfc.tfm.tscOutput != null)
            {
                tfc.tfm.tscOutput.SetVhd(tin, stVhdName, ibdCurrent);
            }
            return true;
        }


        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)

        { 
        }

        public override string stTypeName()
        {
            return "Toh<" + tin.stName + ">";
        }

    }
}
