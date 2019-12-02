using GrammarDLL;
using GraphMatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab
{
    public class Pctl
    {
        public bool fPretty;
        public bool fVerbose;
        public bool fContents;
        public bool fLefts;
        public bool fRights;
        public bool fIdentifier;

        public Pctl (bool fPretty = true, 
                     bool fVerbose = true,
                     bool fContents = false,
                     bool fLefts = false,
                     bool fRights = false,
                     bool fIdentifier = false)
        {
            this.fPretty = fPretty;
            this.fVerbose = fVerbose;
            this.fContents = fContents;
            this.fLefts = fLefts;
            this.fRights = fRights;
            this.fIdentifier = fIdentifier;
        }
        public static Pctl pctlPlain = new Pctl(false, false);
        public static Pctl pctlPretty = new Pctl(true, false);
        public static Pctl pctlVerbose = new Pctl(true, true);
        public static Pctl pctlLefts = new Pctl(true, true, true, true, false);
        public static Pctl pctlRights = new Pctl(true, true, true, false, true);
        public static Pctl pctlIdentifier = new Pctl(false, false, false, false, false, true);

        public static string stIdLeft = "ㄴ";
        public static string stIdRight = "ㄱ";
        public static string stIdBox = "ㅁ";
        public static string stIdCircle = "ㅇ";
        public static string stIdBar = "ㅡ";
        public static string stIdOpenRight = "ㄷ";
        public static string stIdTop = "ㅜ";
        public static string stIdBottom = "ㅗ";
        public static string stIdToLeft = "ㅌ";
        public static string stIdTickLeft = "ㅓ";
        public static string stIdTickRight = "ㅏ";
        public static string stIdTypePrefix = "ㅕ";
        public static string stIdTypeSuffix = "ㅑ";
        public static string stIdDoubleBarLeft = "ㅔ";
        public static string stIdDoubleBarTickLeft = "ㅖ";
    }

    public abstract class Lsx
    {
        public abstract void Format(Fwt sb, Pctl pctl);

        public virtual bool fAtom() { throw new NotImplementedException(); }
        public virtual int nLength() { throw new NotImplementedException(); }
        public virtual bool fEqual(Lsx lsxArg) { return lsxArg == this; }
        public virtual bool fMember(Lsx lsxMem) { throw new NotImplementedException(); }
        public virtual Lsx lsxEval() { throw new NotImplementedException(); }
        public virtual bool fOwnLine() { return false; }



        public string stPPrint(Pctl pctl)
        {
            StringBuilder sb = new StringBuilder();
            Fwt f = new Fwt(sb);
            this.Format(f, pctl);
            return sb.ToString();
        }

        public override string ToString()
        {
            Pctl pctl = new Pctl();
            return stPPrint(pctl);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Lsx))
                return false;
            return fEqual((Lsx) obj);
        }

        public override int GetHashCode()  // get rid of warning
        {
            return base.GetHashCode();
        }
    }

    public interface Ipd
    {

    }

    public class Lsm : Lsx, Ils
    {
        public readonly string stName;


        public static Lsm lsmAnd = new Lsm("and", KSymType.kPredefined);
        public static Lsm lsmEntails = new Lsm("entails", KSymType.kPredefined);
        public static Lsm lsmExists = new Lsm("exists", KSymType.kPredefined);
        public static Lsm lsmFalse = new Lsm("false", KSymType.kPredefined);
        public static Lsm lsmForall = new Lsm("forall", KSymType.kPredefined);
        public static Lsm lsmIff = new Lsm("iff", KSymType.kPredefined);
        public static Lsm lsmImplies = new Lsm("implies", KSymType.kPredefined);
        public static Lsm lsmList = new Lsm("list", KSymType.kPredefined);
        public static Lsm lsmNil = new Lsm("nil", KSymType.kPredefined);
        public static Lsm lsmNot = new Lsm("not", KSymType.kPredefined);
        public static Lsm lsmOr = new Lsm("or", KSymType.kPredefined);
        public static Lsm lsmTrue = new Lsm("true", KSymType.kPredefined);

        public static Lsm lsmEquals = new Lsm("=", KSymType.kPredefined);
        public static Lsm lsmAdd = new Lsm("+", KSymType.kPredefined);
        public static Lsm lsmSubtract = new Lsm("-", KSymType.kPredefined);
        public static Lsm lsmMultiply = new Lsm("*", KSymType.kPredefined);
        public static Lsm lsmDivide = new Lsm("/", KSymType.kPredefined);

        public static Lsm lsmSequentIsFalse = new Lsm("*Sequent Is False*", KSymType.kPredefined);
        public static Lsm lsmIdentifier = new Lsm("*identifier*", KSymType.kPredefined);
        public static Lsm lsmInvalidTerm = new Lsm("*invalidTerm*", KSymType.kPredefined);
        

        static int nCounter = 0;
        public int nId = nCounter++;

        public const int nInitLevel = 1;
        public int nLevel = nInitLevel;

        public Li liIsBound;

        public enum KSymType
        {
            kPredefined,
            kConstant,
            kVar,
            kSkolemFunction
        }

        private KSymType fVar = KSymType.kConstant;
        private Lsx lsxVal = lsmNil;
        public const int nArgsUndefined = -1;
        public int nArity = nArgsUndefined;
        // private Dictionary<int, Lsx> mpint_LsxProps;

        public const int nArityVar = 0;
        public const int nArityConst = 2;

        public bool fPredicateReflexive = false;   // e.g. x=x is true
        public bool fPredicateAntireflexive = false;   // e.g. x=x is false
        public bool fCommutative = false;   // e.g. f(x,y) = f(y,x)
        public bool fPredicateAntisymmetric = false;   // e.g. f(x,y) != f(y,x)
        public bool fPredicateTransitive = false;   // e.g. f(x,y) != f(y,x)
        public Ipd ipdData;

        public const string stVarPrefix = "@";
        public const string stSkolemPrefix = "$";

        public Lsm (string stName, KSymType kType = KSymType.kConstant)
        {
            this.stName = stName;
            this.fVar = kType;
        }

        public bool fVariable()
        {
            return fVar == KSymType.kVar;
        }
        public bool fPredefined()
        {
            return fVar == KSymType.kPredefined;
        }

        public void MakeVariable()
        {
            fVar = KSymType.kVar;
        }
        public void UnmakeVariable()
        {
            fVar = KSymType.kConstant;
        }
        public void MakeSkolemFunction()
        {
            fVar = KSymType.kSkolemFunction;
          //  MakeVariable();
        }
        public bool fSkolemFunction()
        {
            return fVar == KSymType.kSkolemFunction;
        }

        public override Lsx lsxEval() { return lsxVal; }
        public void SetValue(Lsx lsxVal)
        {
            this.lsxVal = lsxVal;
        }
        /*
        public void SetProp(Lsx lsxVal, int iTag)
        {
            if (mpint_LsxProps == null)
                mpint_LsxProps = new Dictionary<int, Lsx>();
            mpint_LsxProps.Add(iTag, lsxVal);
        }
        public Lsx GetProp(int iTag)
        {
            if (mpint_LsxProps == null)
                return lsmNil;
            Lsx lsxVal;
            if (!mpint_LsxProps.TryGetValue(iTag, out lsxVal))
                return lsmNil;
            return lsxVal;
        }
*/
        public override void Format(Fwt sb, Pctl pctl)
        {
            sb.Append(stName);
        }


        public override bool fAtom() { return true; }
        public override int nLength() { return 0; }
        public override bool fMember(Lsx lsxMem) { return false; }
        public override bool fEqual(Lsx lsxArg) { return lsxArg == this; }

        public override int GetHashCode()
        {
            return nId;
        }
    }

    public class Lbv
    {
        public Lsm lsmSym;
        public Lsx lsxVal;
        public Lbv lbvNext;

        public Lbv(Lsm lsmSym, Lsx lsxVal, Lbv livNext)
        {
            this.lsmSym = lsmSym;
            this.lsxVal = lsxVal;
            this.lbvNext = livNext;
        }
    }

    public class Li : Lsx
    {
        public readonly int iV;
        public readonly Li liRest;
        public readonly int cLen;
        public Lbv livBound;   // latest Lsm bound in this


        public Li (int iV)
        {
            this.iV = iV;
            this.cLen = 1;
        }
        public Li(int iV, Li liRest)
        {
            this.iV = iV;
            this.liRest = liRest;
            cLen = liRest.cLen + 1;
        }
        public override void Format(Fwt sb, Pctl pctl)
        {
            sb.Append("[");
            Li liNext = this;
            bool fFirst = true;
            while (liNext != null)
            {
                if (fFirst)
                    fFirst = false;
                else
                    sb.Append(" ");
                sb.Append(liNext.iV.ToString());
                Lbv livNext = liNext.livBound;
                while (livNext != null)
                {
                    if (livNext == liNext.livBound)
                        sb.Append(":");
                    else
                        sb.Append(",");
                    sb.Append(livNext.lsmSym.stName);
                    sb.Append("=");
                    sb.Append(livNext.lsxVal.ToString());
                    livNext = livNext.lbvNext;
                }
                liNext = liNext.liRest;
            }
            sb.Append("]");
        }
        public override bool fAtom() { return true; }
        public override int nLength()
        {
            return cLen;
        }
        public override bool fMember(Lsx lsxMem) { return false; }
        public override bool fEqual(Lsx lsxArg)
        {
            if (!(lsxArg is Li))
                return false;
            Li liArg = (Li) lsxArg;
            if (liArg.iV != iV)
                return false;
            if (liArg.cLen != cLen)
                return false;
            if (liRest != null && liRest != liArg.liRest)
                return false;
            return true;
        }
        // return true if this has a tail that matches liArg
        public bool fExtends (Li liArg)
        {
            if (liArg == null)
                return true;
            if (cLen < liArg.cLen)
                return false;
            Li liThis = this;
            while (liThis.cLen > liArg.cLen)
                liThis = liThis.liRest;
            while (liThis != null)
            {
                if (liThis.iV != liArg.iV)
                    return false;
                liThis = liThis.liRest;
                liArg = liArg.liRest;
            }
            return true;
        }
        public override Lsx lsxEval() { return this; }
    }

    public class Lpr : Lsx
    {
        public Lsx lsxCar;
        public Lsx lsxCdr;

        private static int cCounter = 0;
        public int nId = cCounter++;
        private static bool fDebug = false;

        public Lpr()
        {
            this.lsxCar = Lsm.lsmNil;
            this.lsxCdr = Lsm.lsmNil;
        }

        public Lpr(Lsx lsxCar, Lsx lsxCdr)
        {
            this.lsxCar = lsxCar;
            this.lsxCdr = lsxCdr;
        }

        public static Lsx lprList (params Lsx[] rglsx)
        {
            if (rglsx.Length == 0)
                return Lsm.lsmNil;
            Lsx lsxHead = Lsm.lsmNil;
            Lpr lprTail = null;

            foreach (var lsx in rglsx)
            {
                Lpr lpr = new Lpr(lsx, Lsm.lsmNil);
                if (lprTail == null)
                    lsxHead = lpr;
                else
                    lprTail.lsxCdr = lpr;
                lprTail = lpr;
            }

            return lsxHead;
        }

        public virtual void ExtraInfo(Fwt sb, Pctl pctl) { }

        public override void Format(Fwt sb, Pctl pctl)
        {
            string stSpace;
            if (pctl.fIdentifier)
                stSpace = Pctl.stIdBar;
            else
                stSpace = " ";

            if (sb.rgAlready.Contains(this))
            {
                sb.Append("#{" + nId.ToString() + "}");
            }
            sb.rgAlready.Add(this);

            if (fDebug)
            {
                sb.Append(nId.ToString());
                sb.Append("#");
            }

            if (pctl.fIdentifier)
                sb.Append(Pctl.stIdLeft);
            else
                sb.Append("(");
            bool fFirst = true;
            int iIndented = 0;
            int iToIndent = 3;
            Lsx lsx = this;
            bool fNewlinePending = false;
            bool fNewlineAfter = false;
            while (lsx is Lpr)
            {
                Lpr lpr = (Lpr)lsx;
                if (fFirst)
                {
                    fFirst = false;
                    if (lpr.lsxCar is Lpr)
                    {
                        iToIndent = 1;
                        iIndented = iToIndent;
                        sb.Indent(iToIndent);
                    }
                    if (lpr.lsxCar != null && lpr.lsxCar.fOwnLine())
                    {
                        fNewlineAfter = true;
                    }
                }
                else
                {
                    if (iIndented == 0)
                    {
                        iIndented = iToIndent;
                        sb.Indent(iToIndent);
                        sb.Append(stSpace);
                    }
                    if (lpr.lsxCar != null && lpr.lsxCar.fOwnLine())
                    {
                        if (pctl.fPretty)
                            sb.Newline();
                        else
                            sb.Append(stSpace);
                        fNewlineAfter = true;
                    }
                    else if (fNewlinePending)
                    {
                        if (pctl.fPretty)
                            sb.Newline();
                        else
                            sb.Append(stSpace);
                    }
                    else
                        sb.Append(stSpace);
                }
                if (lpr.lsxCar != null)
                    lpr.lsxCar.Format(sb, pctl);
                else
                    sb.Append("null");
                fNewlinePending = fNewlineAfter;
                fNewlineAfter = false;

                lsx = lpr.lsxCdr;
            }
            if (lsx == null)
            {
                if (fNewlinePending)
                {
                    if (pctl.fPretty)
                        sb.Newline();
                    else
                        sb.Append(stSpace);
                }
                else
                    sb.Append(stSpace);
                sb.Append("null");
            }
            else if (lsx != Lsm.lsmNil)
            {
                if (fNewlinePending)
                {
                    if (pctl.fPretty)
                        sb.Newline();
                    else
                        sb.Append(stSpace);
                }
                else
                    sb.Append(stSpace);
                sb.Append(". ");
                lsx.Format(sb, pctl);
            }
            if (fDebug)
                ExtraInfo(sb, pctl);
            if (iIndented != 0)
                sb.Unindent(iIndented);
            if (pctl.fIdentifier)
                sb.Append(Pctl.stIdRight);
            else
                sb.Append(")");
            sb.rgAlready.RemoveAt(sb.rgAlready.Count-1);
        }
        public override bool fOwnLine()
        {
            Lsx lsx = this;
            while (lsx is Lpr)
            {
                Lpr lpr = (Lpr) lsx;
                if (lpr.lsxCar is Lpr)
                    return true;
                lsx = lpr.lsxCdr;
            }
            return false;
        }

        public override bool fAtom() { return false; }
        public override int nLength()
        {
            return 1 + lsxCdr.nLength();
        }
        public override bool fMember(Lsx lsxMem)
        {
            if (lsxCar.fEqual(lsxMem))
                return true;
            return lsxCdr.fMember(lsxMem); 
        }
        public Lsx lsxFind(Lsx lsxMem)
        {
            Lsx lsxNext = this;
            while (lsxNext != Lsm.lsmNil)
            {
                Lpr lprNext = (Lpr)lsxNext;
                if (lprNext.lsxCar.fEqual(lsxMem))
                    return lprNext.lsxCar;
                lsxNext = lprNext.lsxCdr;
            }
            return Lsm.lsmNil;
        }
        public override bool fEqual(Lsx lsxArg)
        {
            if (!(lsxArg is Lpr))
                return false;
            if (lsxArg == this)
                return true;
            Lpr lprArg = (Lpr)lsxArg;
            if (!lprArg.lsxCar.fEqual(lsxCar))
                return false;
            if (!lprArg.lsxCdr.fEqual(lsxCdr))
                return false;
            return true;
        }
        public override Lsx lsxEval() { throw new NotImplementedException(); }
        public override int GetHashCode()
        {
            return lsxCar.GetHashCode() + lsxCdr.GetHashCode() + 23;
        }

    }

    // Hold extra information related to resultion steps
    public class Lrs : Lpr
    {
        public Lsx lsxLC;
        public Lsx lsxRC;
        public Lsx lsxK;
        public Li liV;   // substitution for the resolution
        public Li liNew;   //  substitution for new variables in this clause

        public Lrs(Lsx lsxCar, Lsx lsxCdr, Lsx lsxLC, Lsx lsxRC, Lsx lsxK, Li liV, Li liNew)
            : base (lsxCar, lsxCdr)
        {
            this.lsxLC = lsxLC;
            this.lsxRC = lsxRC;
            this.lsxK = lsxK;
            this.liV = liV;
            this.liNew = liNew;
        }

        void AddInfo(Fwt sb, string stLabel, Lsx lsxTerm, Pctl pctl)
        {
            sb.Newline();
            sb.Append(stLabel);
            sb.Newline();
            sb.Indent();
            lsxTerm.Format(sb, pctl);
            sb.Unindent();
        }

        public override void ExtraInfo(Fwt sb, Pctl pctl) 
        {
            AddInfo(sb, "LC", lsxLC, pctl);
            AddInfo(sb, "RC", lsxRC, pctl);
            AddInfo(sb, "K", lsxK, pctl);
            AddInfo(sb, "V", liV, pctl);
            AddInfo(sb, "New", liNew, pctl);
        }

    }

    public class Gpl
    {
        public Gpl gplPrev;
        public Lpr lprLast;

        public Gpl (Gpl gplPrev, Lpr lprLast)
        {
            this.gplPrev = gplPrev;
            this.lprLast = lprLast;
        }
    }

    /// <summary>
    /// Construct a tree from Lsx objects
    /// </summary>
    public class GenLsx : IStackOut
    {
        public Gpl gplTop;
        private Lpr lprLast;
        private Lsx lsxRoot = Lsm.lsmNil;
        private LParse lparse;
        private Boolean fPushPending = false;

        public GenLsx (LParse lparse)
        {
            this.lparse = lparse;
        }

        public Lsx lsxGetRoot()
        {
            return lsxRoot;
        }

        public void Push()
        {
            Lpr lprNew = new Lpr(Lsm.lsmNil, Lsm.lsmNil);  // car is where list content will go, or nil if empty
            if (lprLast == null)
            {
            }
            else if (fPushPending)
            {
                lprLast.lsxCar = lprNew;
                gplTop = new Gpl(gplTop, lprLast);
            }
            else
                lprLast.lsxCdr = lprNew;
            lprLast = lprNew;
            fPushPending = true;
        }

        public void Pop()
        {
            if (!fPushPending)
            {
                lprLast = gplTop.lprLast;
                gplTop = gplTop.gplPrev;
            }
            fPushPending = false;
        }

        public void Add(string stText, TokenInfo info)
        {
            Lsm lsmAdd;
            if (info.ilsLiteral == Lsm.lsmIdentifier)
                lsmAdd = lparse.lsmObtain(stText);
            else if (info.ilsLiteral != null)
                lsmAdd = (Lsm) info.ilsLiteral;
            else
                // add to Sko.AddSyms to define grammar defined tokens to be recognized
                lsmAdd = new Lsm("*missing*");

            if (fPushPending)
            {
                fPushPending = false;
                Lpr lprNew = new Lpr(lsmAdd, Lsm.lsmNil);
                lprLast.lsxCar = lprNew;
                gplTop = new Gpl(gplTop, lprLast);
                lprLast = lprNew;
                if (lsxRoot == Lsm.lsmNil)
                    lsxRoot = lprNew;
            }
            else if (lsxRoot == Lsm.lsmNil)
                lsxRoot = lsmAdd;
            else
            {
                Lpr lprNew = new Lpr(lsmAdd, Lsm.lsmNil);
                lprLast.lsxCdr = lprNew;
                lprLast = lprNew;
            }
        }
        public void Newline()
        {

        }

    }

}
