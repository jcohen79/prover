using GraphMatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab.TestUtil
{
    /// <summary>
    /// Capture actions for one step 
    /// </summary>
    public class Tif
    {
        private static int cId = 0;
        public readonly int nId = cId++;

        public Bid bidResult;
        public Tif tifNextInList;
        public Tib tibActionHead;
        public Tib tibActionTail;
        public readonly string stStep;
        public Tiw tiwSave;
        public KCapture kCapture = KCapture.kData;
        public static readonly string stStepName = "tifStep";

        public enum KTime
        {
            kNever,
            kFirst,
            kSecond
        }
        public KTime kTime = KTime.kNever;

        public Tif ()
        {
            this.stStep = "s" + nId;
        }
        public int nGetId()
        {
            return nId;
        }

        public void GenerateCase(Fwt fwt, Tsc tsc)
        {
            fwt.AppendLine("case KSteps." + stStep + ":");
            fwt.Indent();
            fwt.AppendLine("{");
            fwt.Indent();

            // todo: check if value changes to wrong id
            if (bidResult != null)
            {
                fwt.Append(tsc.stIdentifier(bidResult));
                fwt.Append(" = (");
                fwt.Append(bidResult.GetType().Name);
                fwt.Append(")");
                switch (kCapture)
                {
                    case KCapture.kData:
                        fwt.Append("objData");
                        break;
                    case KCapture.kLeft:
                        fwt.Append("objLeft");
                        break;
                    case KCapture.kRight:
                        fwt.Append("objRight");
                        break;
                    default:
                        throw new InvalidProgramException();
                }
                fwt.Append(";");
                fwt.Newline();
            }

            for (Tib tib = tibActionHead; tib != null; tib = tib.tibNextAction)
            {
                tib.GenerateActionStmt(fwt, tsc);
                fwt.Indent();
                fwt.AppendLine("   // " + tib.GetType().Name + tib.nId );
                fwt.Unindent();
            }
            fwt.AppendLine("break;");
            fwt.Unindent();
            fwt.AppendLine("}");
            fwt.Unindent();
        }

        public void PrepareOutlineStep(Tsc tsc)
        {
            // tst.SetGenId(tsc.nGenId++);
            for (Tib tib = tibActionHead; tib != null; tib = tib.tibNextAction)
            {
                tib.PrepareOutline(tsc);
            }
        }

        public void GenerateOutline(Tsc.KPass kPass, Fwt fwt, Tsc tsc)
        {
            string stClassName = "Tst";
            switch (kPass)
            {
                case Tsc.KPass.kFirst:
                    fwt.AppendLine(stClassName + " " + stStepName + nId
                           + " = new " + stClassName + "(pouOutline);");
                    break;
                case Tsc.KPass.kSecond:
                    for (Tib tib = tibActionHead; tib != null; tib = tib.tibNextAction)
                    {
                        tib.GenerateOutline(fwt, tsc, this);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }


    }

    /// <summary>
    /// Base class with info about what is performed within step
    /// </summary>
    public abstract class Tib
    {
        private static int cId = 0;
        public readonly int nId = cId++;
        public Tcd tcd;
        public Tif tifNextStep; // where to go when action is taken (e.g. WatchPair)
        public Tib tibNextAction;   // next in list to generate code
        public static string stStmtName = "tibStmt";
        protected Tib (Tcd tcd)
        {
            this.tcd = tcd;
        }

        public abstract void GenerateActionStmt(Fwt fwt, Tsc tsc);

        public abstract void PrepareOutline(Tsc tsc);

        public abstract void GenerateOutline(Fwt fwt, Tsc tsc, Tif tifParent);

        public static readonly string stKStepPrefix = "KSteps.";

        public abstract string stOutClassName();

        public int nGetId()
        {
            return nId;
        }

        protected void GenerateCallNew(Fwt fwt, Tif tifParent)
        {
            string stClassName = stOutClassName();
            string stParentName = Tif.stStepName + tifParent.nGetId();
            fwt.AppendLine(stClassName + " " + stStmtName + nGetId()
                + " = new " + stClassName + "(" + stParentName + ");");
        }

        protected void GenerateArgumentReference(Fwt fwt, Tsc tsc, string stTypeName, Ivb ivbTarget)
        {
            if (ivbTarget == null)
                fwt.Append("null");
            else
                fwt.Append("pou.ivbGet<" + stTypeName + ">(\"" + ivbTarget.stGetName() + "\")");
        }

        protected void GenerateArgumentReference(Fwt fwt, Tsc tsc, Ibd bidTarget)
        {
            if (bidTarget == null)
                fwt.Append("null");
            else
            {
                Bod bod = Bod.bodMake(tsc, bidTarget);
                if (bod.stVarName == null)
                    throw new InvalidProgramException();
                if (bod.kSourceChannel != KCapture.kPreset)
                    fwt.Append(bod.stVarName);
                else
                    fwt.Append("pou.vrfGet<"
                        + bidTarget.typeOf().Name + ">(\""
                        + bod.stVarName + "\")");
            }
        }

        protected void GenerateArgument(Fwt fwt, Tsc tsc, Uty uty, string stTypeName, Ivb ivbValue)
        {
            // pass reference of argument object to the stmt outline object that needs it
            fwt.Append(stStmtName + nGetId());
            fwt.Append(".");
            fwt.Append(uty.stSetterName);
            fwt.Append(" (");
            GenerateArgumentReference(fwt, tsc, stTypeName, ivbValue);
            fwt.Append(");");

            fwt.AppendLine("");
        }


        protected void GenerateArgument(Fwt fwt, Tsc tsc, Uty uty, Ibd bidValue)
        {
            // pass reference of argument object to the stmt outline object that needs it
            fwt.Append(stStmtName + nGetId());
            fwt.Append(".");
            fwt.Append(uty.stSetterName);
            fwt.Append(" (");
            GenerateArgumentReference(fwt, tsc, bidValue);
            fwt.Append(");");

            fwt.Append("\t// ");
            fwt.Append(bidValue);
            fwt.AppendLine("");
        }

        protected void GenerateArgument(Fwt fwt, Tsc tsc, Uty uty, string stValue)
        {
            fwt.Append(stStmtName + nGetId());
            fwt.Append(".");
            fwt.Append(uty.stSetterName);
            fwt.Append(" (\"");
            fwt.Append(stValue);
            fwt.Append("\");");

            fwt.AppendLine("");
        }

        protected void GenerateArgTcd(Fwt fwt, Tcd tcd)
        {
            // pass reference of argument object to the stmt outline object that needs it
            fwt.Append(stStmtName + nGetId());
            fwt.Append(".SetTcd(");
            fwt.Append("Tcd." + tcd.stName);
            fwt.Append(");");
            fwt.AppendLine("");
        }

        protected void GenerateNextTst(Fwt fwt, Tif tifNext)
        {
            // pass reference of argument object to the stmt outline object that needs it
            fwt.Append(stStmtName + nGetId());
            fwt.Append(".SetNextTst(");
            fwt.Append(Tif.stStepName + tifNext.nGetId());
            fwt.Append(");");
            fwt.AppendLine("");
        }

        protected void GenerateSetLabel(Fwt fwt, string stLabel)
        {
            // pass reference of argument object to the stmt outline object that needs it
            fwt.Append(stStmtName + nGetId());
            fwt.Append(".SetLabel(\"");
            fwt.Append(stLabel);
            fwt.Append("\");");
            fwt.AppendLine("");
        }

        protected void GenerateValue<T>(Fwt fwt, string stFieldName, T kValue) where T : System.Enum
        {
            // pass reference of argument object to the stmt outline object that needs it
            fwt.Append(stStmtName + nGetId());
            fwt.Append(".Set");
            fwt.Append(stFieldName);
            fwt.Append("(");
            fwt.Append(kValue.GetType().Name);
            fwt.Append(".");
            fwt.Append(kValue.ToString());
            fwt.Append(");");
            fwt.AppendLine("");
        }


    }

    /// <summary>
    /// Record the setting of a Vhd place so value can be saved for use another step.
    /// See also Tid
    /// </summary>
    public class Tih<T> : Tib where T : Ibd
    {
        public Ibd ibdVal;
        public readonly string stVhdName;
        public readonly Tin tin;

        public Tih(Tin tin, string stVhdName, Ibd bidVal) : base(null)
        {
            this.tin = tin;
            this.stVhdName = stVhdName;
            this.ibdVal = bidVal;
        }
        public override string stOutClassName()
        {
            return "Toh<" + ibdVal.GetType().Name + ">";
        }

        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        {
            fwt.AppendLine("tfcTop.pou.mpst_ivbValues.Set (\"" + stVhdName + "\", objData);");
        }

        public override void PrepareOutline(Tsc tsc)
        {
            Bod.bodMake(tsc, ibdVal);
        }

        public override void GenerateOutline(Fwt fwt, Tsc tsc, Tif tifParent)
        {
            GenerateCallNew(fwt, tifParent);
            GenerateArgument(fwt, tsc, Uty.utyName, stVhdName);
            GenerateArgument(fwt, tsc, Uty.utyTinName, tin.stName);
            GenerateArgument(fwt, tsc, Uty.utyValue, ibdVal);
        }

    }

    /// <summary>
    /// Action to save a value from the current step to a variable.
    /// See also Tih and Vhd for saving values created in other code.
    /// </summary>
    public class Tid: Tib
    {
        public KCapture kKind;
        public Ibd ibdValue;
        public readonly Bod bod;

        public Tid(Tcd tcd, KCapture kKind, Ibd ibdValue, Bod bod) : base(tcd)
        {
            this.kKind = kKind;
            this.ibdValue = ibdValue;
            this.bod = bod;
        }

        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        {
            throw new NotImplementedException();
        }

        public override void PrepareOutline(Tsc tsc)
        {
        }
        public override string stOutClassName()
        {
            return "Tod";
        }

        public override void GenerateOutline(Fwt fwt, Tsc tsc, Tif tifParent)
        {
            if (bod.kUsed != Bod.KUsage.kNeeded)
                return;
            bod.kUsed = Bod.KUsage.kObtained;

            GenerateCallNew(fwt, tifParent);
            fwt.Append(stStmtName + nGetId());
            fwt.Append(".kKind = KCapture.");
            fwt.Append(kKind.ToString());
            fwt.Append(";");
            fwt.AppendLine("");
            fwt.Append(stStmtName + nGetId());
            fwt.Append(".hbvPlace = ");
            fwt.Append(bod.stVarName);
            fwt.Append(";");
            fwt.AppendLine("");
        }

    }

    /// <summary>
    /// Built when WatchPair is called
    /// </summary>
    public class Tip: Tib
    {
        public Ibd bidTarget;
        public Ibd bidInput;
        public string stLabel;

        public Ibd ibdTarget;
        public Ibd ibdInput;

        public Tip(Tcd tcd, Ibd bidTarget, Ibd bidInput, string stLabel) : base(tcd)
        {
            this.bidTarget = bidTarget;
            this.bidInput = bidInput;
            this.stLabel = stLabel;
        }

        public override void PrepareOutline(Tsc tsc)
        {
            ibdTarget = Bod.bodMake(tsc, bidTarget);
            ibdInput = Bod.bodMake(tsc, bidInput);
        }

        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        {
            fwt.AppendLine("// " + tsc.stIdentifier(bidTarget) + ": " + bidTarget);
            fwt.AppendLine("// " + tsc.stIdentifier(bidInput) + ": " + bidInput);
            fwt.AppendLine("WatchPair ("
                + tsc.stIdentifier(bidTarget) + ", "
                + tsc.stIdentifier(bidInput) + ", "
                + "Tcd." + tcd.stName + ", "
                + "\"" + stLabel + "\", "
                + "(int) " + stKStepPrefix + tifNextStep.stStep + ");");
        }
        public override string stOutClassName()
        {
            return "Top";
        }
        public override void GenerateOutline(Fwt fwt, Tsc tsc, Tif tifParent)
        {
            //            todo: make abstract, each impl declares vars, builds Bod from discrete values, pass as args
            GenerateCallNew(fwt, tifParent);
            GenerateArgument(fwt, tsc, Uty.utyTarget, ibdTarget);
            GenerateArgument(fwt, tsc, Uty.utyInput, ibdInput);
            GenerateArgTcd(fwt, tcd);
            GenerateNextTst(fwt, tifNextStep);
            GenerateSetLabel(fwt, stLabel);
        }
    }

    /// <summary>
    /// Built when WatchTarget is called
    /// </summary>
    public class Tig: Tib
    {
        public Ibd bidTarget;
        public Ibd ibdTarget;

        public Tig(Tcd tcd, Ibd bidTarget) : base(tcd)
        {
            this.bidTarget = bidTarget;
        }

        public override void PrepareOutline(Tsc tsc)
        {
            ibdTarget = Bod.bodMake(tsc, bidTarget);
        }

        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        {
            fwt.AppendLine("// " + tsc.stIdentifier(bidTarget) + ": " + bidTarget);
            fwt.AppendLine("WatchTarget ("
                + tsc.stIdentifier(bidTarget) + ", "
                + "Tcd." + tcd.stName + ", "
                + "\"\", "
                + "(int) " + stKStepPrefix + tifNextStep.stStep + ");");
        }
        public override string stOutClassName()
        {
            return "Tog";
        }

        public override void GenerateOutline(Fwt fwt, Tsc tsc, Tif tifParent)
        {
            //            todo: make abstract, each impl declares vars, builds Bod from discrete values, pass as args
            GenerateCallNew(fwt, tifParent);
            GenerateArgument(fwt, tsc, Uty.utyTarget, ibdTarget);
            GenerateArgTcd(fwt, tcd);
            GenerateNextTst(fwt, tifNextStep);
        }

    }

    public class Tis: Tib
    {
        public readonly string stTspName;
        public Ivh<Asc> hascLeft;
        public Ivh<Asc> hascRight;
        public Tis(Ivh<Asc> hascLeft, Ivh<Asc> hascRight) : base(null)
        {
            this.stTspName = "tsp" + hascLeft.stGetName() + "_" + hascRight.stGetName();
            this.hascLeft = hascLeft;
            this.hascRight = hascRight;


        }

        public override void PrepareOutline(Tsc tsc)
        {
        }

        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        {
            fwt.AppendLine("// " + hascRight.stGetName() + ": " + hascRight.stGetName());
            fwt.AppendLine("StartPair (ref "
                + stTspName + ", "
                + hascLeft.stGetName() + ", "
                + hascRight.stGetName() + ", "
                + "(int) " + stKStepPrefix + tifNextStep.stStep + ");");
            Vdi vdi = new Vdi(typeof(Tsp), stTspName, true);
            tsc.AddVdi(vdi);
        }
        public override string stOutClassName()
        {
            return "Tos";
        }
        public override void GenerateOutline(Fwt fwt, Tsc tsc, Tif tifParent)
        {
            //            todo: make abstract, each impl declares vars, builds Bod from discrete values, pass as args
            GenerateCallNew(fwt, tifParent);
            GenerateArgument(fwt, tsc, Uty.utyLeft, typeof(Asc).Name, hascLeft);
            GenerateArgument(fwt, tsc, Uty.utyRight, typeof(Asc).Name, hascRight);
            GenerateNextTst(fwt, tifNextStep);
        }

    }

    public class Tii: Tib
    {
        public Ibd bidInput;
        public Ibd ibdInput;

        public Tii(Tcd tcd, Ibd bidInput) : base(tcd)
        {
            this.bidInput = bidInput;
        }

        public override void PrepareOutline(Tsc tsc)
        {
            ibdInput = Bod.bodMake(tsc, bidInput);
        }

        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        {
            fwt.AppendLine("// " + tsc.stIdentifier(bidInput) + ": " + bidInput);
            fwt.AppendLine("WatchInput ("
                + tsc.stIdentifier(bidInput) + ", "
                + "Tcd." + tcd.stName + ", "
                + "\"\", "
                + "(int) " + stKStepPrefix + tifNextStep.stStep + ");");
        }
        public override string stOutClassName()
        {
            return "Toi";
        }
        public override void GenerateOutline(Fwt fwt, Tsc tsc, Tif tifParent)
        {
            //            todo: make abstract, each impl declares vars, builds Bod from discrete values, pass as args
            GenerateCallNew(fwt, tifParent);
            GenerateArgument(fwt, tsc, Uty.utyInput, ibdInput);
            GenerateArgTcd(fwt, tcd);
            GenerateNextTst(fwt, tifNextStep);
        }

    }

    public class Tiw: Tib
    {
        public Ibd bidToWatch;
        public Tif.KTime kAfter = Tif.KTime.kNever;
        public Ibd ibdToWatch;

        public Tiw(Tcd tcd, Ibd bidToWatch) : base(tcd)
        {
            this.bidToWatch = bidToWatch;
        }

        public void IncrementAfter(Tif tifCurrent)
        {
            switch (kAfter)
            {
                case Tif.KTime.kNever:
                    kAfter = Tif.KTime.kFirst;
                    tifCurrent.kTime = Tif.KTime.kFirst;
                    break;
                case Tif.KTime.kFirst:
                    kAfter = Tif.KTime.kSecond;
                    break;
                case Tif.KTime.kSecond:
                    break;
                default:
                    throw new InvalidProgramException();
            }
        }


        public override void PrepareOutline(Tsc tsc)
        {
            ibdToWatch = Bod.bodMake(tsc, bidToWatch);
        }

        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        {
            fwt.AppendLine("// " + tsc.stIdentifier(bidToWatch) + ": " + bidToWatch);
            fwt.AppendLine("if (fWatch ("
                + tsc.stIdentifier(bidToWatch) + ", "
                + "(int) " + stKStepPrefix + tifNextStep.stStep + ", "
                + "\"\"))");
            fwt.Indent();
            fwt.AppendLine("Restart();");
            fwt.Unindent();
        }
        public override string stOutClassName()
        {
            return "Tow";
        }
        public override void GenerateOutline(Fwt fwt, Tsc tsc, Tif tifParent)
        {
            //            todo: make abstract, each impl declares vars, builds Bod from discrete values, pass as args
            GenerateCallNew(fwt, tifParent);
            GenerateArgument(fwt, tsc, Uty.utyToWatch, ibdToWatch);
            GenerateNextTst(fwt, tifNextStep);
        }
    }
    public enum KTis
    {
        kInvalid,
        kPtiAllowEprNonVbl,
        kEqsAllowEprNonVbl
    }

    public class Tiv: Tib
    {

        public readonly KTis kValue;
        public Ibd bidValue;
        public Ibd ibdValue;

        public Tiv(KTis kValue, Ibd bidValue) : base(null)
        {
            this.kValue = kValue;
            this.bidValue = bidValue;
        }

        public override void PrepareOutline(Tsc tsc)
        {
            ibdValue = Bod.bodMake(tsc, bidValue);
        }

        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        {
            fwt.AppendLine("// " + tsc.stIdentifier(bidValue) + ": " + bidValue);
            switch (kValue)
            {
                case KTis.kPtiAllowEprNonVbl:
                    fwt.AppendLine("SetPtiAllowEprNonVbl(" + tsc.stIdentifier(bidValue) + ");");
                    break;
                case KTis.kEqsAllowEprNonVbl:
                    fwt.AppendLine("SetEqsAllowEprNonVbl(" + tsc.stIdentifier(bidValue) + ");");
                    break;
                default:
                    throw new InvalidProgramException();
            }
        }
        public override string stOutClassName()
        {
            return "Tov";
        }
        public override void GenerateOutline(Fwt fwt, Tsc tsc, Tif tifParent)
        {
            //            todo: make abstract, each impl declares vars, builds Bod from discrete values, pass as args
            GenerateCallNew(fwt, tifParent);
            GenerateValue(fwt, "KValue", kValue);
            GenerateArgument(fwt, tsc, Uty.utyValue, ibdValue);
        }

    }

    /// <summary>
    /// Info to declare a variable
    /// </summary>
    public class Vdi
    {
        public readonly Type type;
        public readonly string stName;
        public readonly bool fDeclare;

        public Vdi (Type type, string stName, bool fDeclare)
        {
            this.type = type;
            this.stName = stName;
            this.fDeclare = fDeclare;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vdi))
                return false;
            Vdi vdiO = (Vdi)obj;
            return vdiO.type == type && vdiO.stName.Equals(stName);
        }

        public override int GetHashCode()
        {
            return type.GetHashCode() * 7 + stName.GetHashCode();
        }

    }

    /// <summary>
    /// Hold arguments and results for a function call made during a test to code can be generated
    /// </summary>
    public class Tia : Tib
    {
        public readonly Tff tff;
        public Ibd bidResult;
        public Ibd[] rgbifArgs;

        Ibd ibdResult;
        List<Ibd> rgibdfArgs;

        public Tia(Tff tff, Ibd bidResult, Ibd[] rgbifArgs) : base(null)
        {
            this.tff = tff;
            this.bidResult = bidResult;
            this.rgbifArgs = rgbifArgs;
        }

        public override void GenerateActionStmt(Fwt fwt, Tsc tsc)
        {
            tff.GenerateActionStmt(fwt, tsc, bidResult, rgbifArgs);
        }
        public override string stOutClassName()
        {
            return "Toa";
        }
        private void GenerateTff(Fwt fwt)
        {
            fwt.Append(Tib.stStmtName + nGetId());
            fwt.Append(".SetTff (");
            fwt.Append(tff.stFactoryLabel);
            fwt.Append(");");
            fwt.AppendLine("");
        }

        public override void PrepareOutline(Tsc tsc)
        {
            ibdResult = Bod.bodMake(tsc, bidResult);
            rgibdfArgs = new List<Ibd>();
            for (int i = 0; i < rgbifArgs.Length; i++)
                rgibdfArgs.Add(Bod.bodMake(tsc, rgbifArgs[i]));
        }

        public override void GenerateOutline(Fwt fwt, Tsc tsc, Tif tifParent)
        {
            //            todo: make abstract, each impl declares vars, builds Bod from discrete values, pass as args
            GenerateCallNew(fwt, tifParent);
            GenerateTff(fwt);
            GenerateArgument(fwt, tsc, Uty.utyResult, ibdResult);
            foreach (Ibd ibdArg in rgibdfArgs)
                GenerateArgument(fwt, tsc, Uty.utyArgs, ibdArg);  // need to identify each arg?
        }
    }


    /// <summary>
    /// Base class for singleton classes with a function to call in tests.
    /// This is to capture info about the return value for a call in order generate series of stmts.
    /// </summary>
    public abstract class Tff
    {
        public readonly string stFactoryLabel;

        public Tff()
        {
            this.stFactoryLabel = GetType().Name + ".tff";
        }

        // call the function that the test needs
        // Should only be called from bidInvoke, not the test itself
        public abstract Bid bidCall(Tfc tfc, Bid[] rgbidArgs);

        protected void MakeAction(Tfc tfc, Bid[] rgbidArgs, Bid bidResult)
        {
            Tia tiaCapture = new Tia(this, bidResult, rgbidArgs);
            tfc.tfm.tscOutput.fAddAction(tiaCapture);
        }


        // call using the factory so that parameters and results are captured for later code gen
        public Bid bidInvoke (Tfc tfc, params Bid[] rgbidArgs)
        {
            Bid bidResult = bidCall(tfc, rgbidArgs);

            if (tfc.tfm.tscOutput != null)
            {
                MakeAction(tfc, rgbidArgs, bidResult);
            }
            return bidResult;
        }

        public virtual void GenerateActionStmt(Fwt fwt, Tsc tsc, Ibd bidResult, Ibd[] rgbifArgs)
        {
            string[] rgstArgIds = new string[rgbifArgs.Length];
            for (int n = 0; n < rgbifArgs.Length; n++)
            {
                rgstArgIds[n] = tsc.stIdentifier(rgbifArgs[n]);
                fwt.AppendLine("// " + rgstArgIds[n] + ": " + rgbifArgs[n]);
            }
            if (bidResult != null)
            {
                string stResultId = tsc.stIdentifier(bidResult);
                fwt.AppendLine("// " + stResultId + ": " + bidResult);

                string stTypeName = bidResult.GetType().Name;
                fwt.Append(stResultId + " = (" + stTypeName + ") ");
            }

            fwt.Append(stFactoryLabel + ".bidInvoke (this");
            for (int n = 0; n < rgbifArgs.Length; n++)
            {
                fwt.Append(", ");
                fwt.Append(rgstArgIds[n]);
            }
            fwt.AppendLine(");");
        }
    }

    /// <summary>
    /// Trace scenario: has list of Tif to capture steps
    /// </summary>
    public class Tsc
    {
        public Dictionary<long, Tif> mpn_tifStepByNumber = new Dictionary<long, Tif>();
        public Dictionary<long, Vdi> mpn_vdiBidIdentifier = new Dictionary<long, Vdi>();
        HashSet<Vdi> rgvdiOther = new HashSet<Vdi>();
        Dictionary<string,Tib> mpst_tibDedup = new Dictionary<string, Tib>();
        public Dictionary<long, Bod> mpn_bodBidUsage = new Dictionary<long, Bod>();

        // https://en.wikipedia.org/wiki/Korean_language_and_computers#Hangul_in_Unicode

        Tif tifHead;
        Tif tifTail;
        public Tif tifCurrent;
        string stFilename;
        public readonly string stClassName;
        public int nGenId = 0;

        public Tsc(string stFilename, string stClassName)
        {
            this.stFilename = stFilename;
            this.stClassName = stClassName;
        }

        public void AddVdi (Vdi vdi)
        {
            rgvdiOther.Add(vdi);
        }

        public void SetIdentifier(Ibd bid, string stLabel, bool fDeclare = false)
        {
            Vdi vdi = new Vdi(bid.GetType(), stLabel, fDeclare);
            mpn_vdiBidIdentifier[bid.nGetId()] = vdi;
        }

        public string stIdentifier(Ibd bid)
        {
            if (bid == null)
                return "null";

            Vdi vdi;
            if (mpn_vdiBidIdentifier.TryGetValue(bid.nGetId(), out vdi))
                return vdi.stName;
            string stId = bid.GetType().Name.ToLower() + "_" + bid.nGetId();
            SetIdentifier(bid, stId, true);
            return stId;
        }

        /// <summary>
        /// Find the Tif for the next step
        /// </summary>
        private Tif tifObtain (Tfc tfc, Psb psb)
        {
            Tif tif;
            if (!mpn_tifStepByNumber.TryGetValue(psb.nId, out tif))
            {
                tif = new Tif();
                mpn_tifStepByNumber.Add(psb.nId, tif);

                if (tifHead == null)
                    tifHead = tif;
                else
                    tifTail.tifNextInList = tif;
                tifTail = tif;
            }
            return tif;
        }

        public bool fAddAction (Tib tibAction)
        {
            if (tifCurrent == null)  // not started steps yet
                return false;

            if (tifCurrent.kTime != Tif.KTime.kFirst)
                return false;
            if (tifCurrent.tibActionTail == null)
                tifCurrent.tibActionHead = tibAction;
            else
                tifCurrent.tibActionTail.tibNextAction = tibAction;
            tifCurrent.tibActionTail = tibAction;
            return true;
        }

        /// <summary>
        /// capture result value and save on Tif for the step
        /// </summary>
        public void NextStep(Tfc tfc, Psb psb, object objLeft, object objRight, object objData, Tcd tcdEvent)
        {
            tifCurrent = tifObtain(tfc, psb);
            switch (tifCurrent.kCapture)
            {
                case KCapture.kData:
                    tifCurrent.bidResult = (Bid)objData;
                    break;
                case KCapture.kLeft:
                    tifCurrent.bidResult = (Bid)objLeft;
                    break;
                case KCapture.kRight:
                    tifCurrent.bidResult = (Bid)objRight;
                    break;
                default:
                    throw new InvalidProgramException();
            }

            switch (tifCurrent.kTime)
            {
                case Tif.KTime.kNever:
                    tifCurrent.kTime = Tif.KTime.kFirst;
                    break;
                case Tif.KTime.kFirst:
                    tifCurrent.kTime = Tif.KTime.kSecond;
                    break;
                case Tif.KTime.kSecond:
                    break;
                default:
                    throw new InvalidOperationException();
            }
            
        }

        public void SetVhd<T>(Tin tin, string stVhdName, T tVal) where T : Ibd
        {
            if (!mpst_tibDedup.ContainsKey(stVhdName))
            {
                Tih<T> tih = new Tih<T>(tin, stVhdName, tVal);
                if (fAddAction(tih))
                    mpst_tibDedup[stVhdName] = tih;
            }
        }

        public void StartPhase (Ivh<Asc> hascLeft, Ivh<Asc> hascRight, Tfc tfc, Psb psb)
        {
            Tis tis = new Tis(hascLeft, hascRight);
            tis.tifNextStep = tifObtain(tfc, psb);
            fAddAction(tis);
        }

        public void SaveValue (KCapture kKind, Ibd ibdValue, Bod bod)
        {
            Tid tid = new Tid(null, kKind, ibdValue, bod);
            fAddAction(tid);
        }

        public void WatchPair(Ibd bidTarget, Ibd bidInput, Tcd tcdEvent, string stLabel, Tfc tfc, Psb psb)
        {
            // a hack until phases are removed. The Top for these should not appear in regened outline.
            if (tcdEvent == Tcd.tcdMakeEpu)
                return;

            Tip tip = new Tip(tcdEvent, bidTarget, bidInput, stLabel);
            tip.tifNextStep = tifObtain(tfc, psb);
            fAddAction(tip);
        }

        public void WatchTarget(Ibd bidTarget, Tcd tcdEvent, string stLabel, Tfc tfc, Psb psb)
        {
            Tig tip = new Tig(tcdEvent, bidTarget);
            tip.tifNextStep = tifObtain(tfc, psb);
            fAddAction(tip);
        }

        public void WatchInput(Ibd bidInput, Tcd tcdEvent, string stLabel, Tfc tfc, Psb psb)
        {
            Tii tip = new Tii(tcdEvent, bidInput);
            tip.tifNextStep = tifObtain(tfc, psb);
            fAddAction(tip);
        }

        // Create object to record that a called to fWatch is made in proof outline
        public void fWatch(Ibd bidToWatch,  Tfc tfc, Psb psbDest, string stLabel)
        {
            Tiw tip = new Tiw(null, bidToWatch);
            tip.tifNextStep = tifObtain(tfc, psbDest);
            if (fAddAction(tip))
                tifCurrent.tiwSave = tip;
        }
        /*
        public void StartPair(Ibd bidLeft, Ibd bidRight, Tfc tfc, int nNextStep)
        {
            Tis tip = new Tis(bidLeft, bidRight);
            tip.tifNextStep = tifObtain(tfc, nNextStep);
            fAddAction(tip);
        } */

        public void SetPtiAllowEprNonVbl(Ibd bidValue)
        {
            Tiv tip = new Tiv(KTis.kPtiAllowEprNonVbl, bidValue);
            fAddAction(tip);
        }

        public void SetEqsAllowEprNonVbl(Ibd bidValue)
        {
            Tiv tip = new Tiv(KTis.kEqsAllowEprNonVbl, bidValue);
            fAddAction(tip);
        }

        public enum KPass
        {
            kFirst,
            kSecond,
        }

        public void GenerateOutline(Fwt fwt)
        {
            fwt.AppendLine("Pou pou = pouOutline;"); //  new " + stClassName + "();");
            for (int nPass = 0; nPass <= (int)KPass.kSecond; nPass++)
            {
                fwt.Newline();
                for (Tif tif = tifHead; tif != null; tif = tif.tifNextInList)
                {
                    tif.GenerateOutline((KPass)nPass, fwt, this);
                }
            }
        }

        public void Generate(Fwt fwt, Pou pou)
        {
            fwt.AppendLine("using System;");
            fwt.AppendLine("using System.Diagnostics;");
            fwt.AppendLine("using reslab.TestUtil;");
            fwt.Newline();
            fwt.AppendLine("namespace reslab");
            fwt.AppendLine("{");
            fwt.Indent();

            string stClassName = (pou == null) ? "TfcGenEx2" : pou.stClassName;
            fwt.AppendLine("class " + stClassName + ": TfcBuilt, Ibo");
            fwt.AppendLine("{");
            fwt.Indent();

            fwt.AppendLine("public " + stClassName + "(Tfm tfm, PouEx2 pou) : base(tfm, pou) {}");

            if (pou != null)
            {
                fwt.AppendLine("public override Pou pouBuildOutline(Iss iss)");
                fwt.AppendLine("{");
                fwt.Indent();
                GenerateBodDecls(fwt);
                GenerateOutline(fwt);
                fwt.AppendLine("return pou;");
                fwt.Unindent();
                fwt.AppendLine("}");
            }
            else
            {
                GenerateStepEnum(fwt);
                fwt.AppendLine("public override bool fNextStep(Tfc tfcPhase, int nPhaseParm, int nNextStep, object objLeft, object objRight, object objData, Tcd tcdEvent)");
                fwt.AppendLine("{");
                fwt.Indent();
                if (pou != null)
                {
                    fwt.AppendLine("throw new NotImplementedException();");
                }
                {
                    fwt.AppendLine("if (tfm.tscOutput != null)");
                    fwt.Indent();
                    fwt.AppendLine("tfm.tscOutput.NextStep(tfm.nPhase, nNextStep, objLeft, objRight, objData, tcdEvent);");
                    fwt.Unindent();
                    fwt.Newline();

                    GenerateSwitch(fwt);
                }
                fwt.Unindent();
                fwt.AppendLine("}"); // fNextStep

                GenerateVariables(fwt);
            }
            fwt.Unindent();
            fwt.AppendLine("}"); // class

            fwt.Unindent();
            fwt.AppendLine("}"); // namespace
        }

        /// <summary>
        /// Generate variables to hold Bid from the time they are seen so they can be used
        /// </summary>
        void GenerateBodDecls(Fwt fwt)
        {
            foreach (KeyValuePair<long,Bod> entry in mpn_bodBidUsage)
            {
                Bod bod = entry.Value;
                long nId = entry.Key;
                if (bod.kUsed != Bod.KUsage.kUnused
                    && bod.kSourceChannel != KCapture.kPreset)
                {
                    string stTypeName = bod.typeOfSrc.Name;
                    fwt.Append("Hbv<");
                    fwt.Append(stTypeName);
                    fwt.Append("> ");
                    fwt.Append(bod.stVarName);
                    fwt.Append("= new Hbv<");
                    fwt.Append(stTypeName);
                    fwt.Append(">();");
                    fwt.Newline();
                }
            }
        }


        // obsolete?
        public void GenerateVariables(Fwt fwt)
        {
            foreach (Vdi vdi in mpn_vdiBidIdentifier.Values)
                if (vdi.fDeclare)
                    fwt.AppendLine(vdi.type.Name + " " + vdi.stName + ";");
            foreach (Vdi vdi in rgvdiOther)
                if (vdi.fDeclare)
                    fwt.AppendLine(vdi.type.Name + " " + vdi.stName + ";");
        }

        public void GenerateStepEnum(Fwt fwt)
        {
            bool fFirst = true;
            fwt.AppendLine("public enum KSteps");
            fwt.AppendLine("{");
            fwt.Indent();

            for (Tif tif = tifHead; tif != null; tif = tif.tifNextInList)
            {
                if (fFirst)
                    fFirst = false;
                else
                    fwt.AppendLine(",");
                fwt.Append(tif.stStep);
            }
            fwt.Unindent();
            fwt.Newline();
            fwt.AppendLine("}");
            fwt.Newline();
        }

        public void GenerateSwitch(Fwt fwt)
        {
            fwt.AppendLine("KSteps kStep = (KSteps) nNextStep;");
            
            fwt.AppendLine("Debug.WriteLine(\"nPhase: \" + (KPhase) tfm.nPhase  + \":\" + kStep + \"(\" + (int) kStep + \")\");");

            fwt.AppendLine("switch(kStep)");
            fwt.AppendLine("{");
            fwt.Indent();

            for (Tif tif = tifHead; tif != null; tif = tif.tifNextInList)
            {
                tif.GenerateCase(fwt, this);
            }
            fwt.AppendLine("default:");
            fwt.Indent();
            fwt.AppendLine("throw new NotImplementedException();");
            fwt.Unindent();
            fwt.Unindent();
            fwt.AppendLine("}");
            fwt.AppendLine("return true;");
        }

        public void Generate(Pou pou)
        {
            Fwt fwt = new Fwt();
            Generate(fwt, pou);

            using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(stFilename)) 
            {
                file.WriteLine(fwt.stString());
            }

        }

        public Pou pouMakeManualOutline(string stClassName)
        {
            Pou pouTop = new Pou(stClassName);  // top of outline
            for (Tif tif = tifHead; tif != null; tif = tif.tifNextInList)
            {
                tif.PrepareOutlineStep(this);
            }
            return pouTop;
        }
    }
    public class TfEpuToEqs : Tff
    {
        public static TfEpuToEqs tff = new TfEpuToEqs();
        public override Bid bidCall(Tfc tfc, Bid[] rgbidArgs)
        {
            Epu epu = (Epu)rgbidArgs[0];
            return tfc.tfm.res.eqsObtain(epu.atpToEquate);
        }
    }
    public class TfAtpChild : Tff
    {
        public static TfAtpChild tff = new TfAtpChild();
        public override Bid bidCall(Tfc tfc, Bid[] rgbidArgs)
        {
            Etp etp = (Etp)rgbidArgs[0];
            return etp.atpChild;
        }
    }
    public class TfEtpToEqs : Tff
    {
        public static TfEtpToEqs tff = new TfEtpToEqs();
        public override Bid bidCall(Tfc tfc, Bid[] rgbidArgs)
        {
            Etp etp = (Etp)rgbidArgs[0];
            return tfc.tfm.res.eqsObtain(etp.atpChild);
        }
    }
    public class TfVbvMinimalSoln : Tff
    {
        public static TfVbvMinimalSoln tff = new TfVbvMinimalSoln();
        public override Bid bidCall(Tfc tfc, Bid[] rgbidArgs)
        {
            Eqs eqs = (Eqs)rgbidArgs[0];
            return eqs.vbvMinimalSoln;
        }
    }
    public class TfFindDup : Tff
    {
        public static TfFindDup tff = new TfFindDup();
        public override Bid bidCall(Tfc tfc, Bid[] rgbidArgs)
        {
            Eqs eqs = (Eqs)rgbidArgs[0];
            Vbv vbv = (Vbv)rgbidArgs[1];
            return Tfc.vbvFindDup(eqs.rgvbvGetSolutions(), vbv);
        }
    }
    public class TAtpToEqs : Tff
    {
        public static TAtpToEqs tff = new TAtpToEqs();
        public override Bid bidCall(Tfc tfc, Bid[] rgbidArgs)
        {
            Atp atp = (Atp)rgbidArgs[0];
            return tfc.tfm.res.eqsObtain(atp);
        }
    }

    public class TEqsSub : Tff
    {
        public static TEqsSub tff = new TEqsSub();
        public override Bid bidCall(Tfc tfc, Bid[] rgbidArgs)
        {
            Eub eub = (Eub)rgbidArgs[0];
#if DEBUG
            return eub.eqsSub;
#else
            return null;
#endif
        }
    }
    public class TffSave : Tff
    {
        public static TffSave tff = new TffSave();
        public override Bid bidCall(Tfc tfc, Bid[] rgbidArgs)
        {
            Tfm tfm = tfc.tfm;
            if (tfm.tscOutput != null)
            {
                MakeAction(tfc, rgbidArgs, null);
                Pou pou = tfm.tscOutput.pouMakeManualOutline(tfm.tscOutput.stClassName);
                tfm.tscOutput.Generate(pou);
                    // C:\Users\user\gen");
            }
            return null;
        }
    }

}
