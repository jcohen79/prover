using System.Text;
using System.Diagnostics;
using System;
using System.IO;
using System.Collections.Generic;
using GraphMatching;

namespace reslab
{

    public class Integer
    {
        public readonly int nValue;

        public Integer(int nValue)
        {
            this.nValue = nValue;
        }
        public override int GetHashCode()
        {
            return nValue;
        }

        public override bool Equals(object obj)
        {
            return this == obj;
        }
    }

    /// <summary>
    /// Hold an integer value that can be hashed with other Bid
    /// </summary>
    public class Nin : Bid
    {
        public readonly int nValue;

        public Nin(int nValue)
        {
            this.nValue = nValue;
        }
    }

    /// <summary>
    /// Identify the operation being logged
    /// </summary>
    public class Tcd
    {
        public readonly string stName;
        Tcd tcdParent;
        private static Dictionary<int, Nin> mpn_intCache;

        public static Nin nin2;
        public static Nin nin3;
        public static Nin nin4;

        public static void Reset()
        {
            mpn_intCache = new Dictionary<int, Nin>();
            nin2 = nCached(2);
            nin3 = nCached(3);
            nin4 = nCached(4);
        }

        private Tcd(string stName, Tcd tcdParent)
        {
            this.stName = stName;
            this.tcdParent = tcdParent;
        }
        public override int GetHashCode()
        {
            return stName.GetHashCode();
        }

        public static Nin nCached(int n)
        {
            Nin nValue;
            if (!mpn_intCache.TryGetValue(n, out nValue))
            {
                nValue = new Nin(n);
                mpn_intCache.Add(n, nValue);
            }
            return nValue;
        }

        /// <summary>
        /// Return true if tcdEvent is included in this.
        /// </summary>
        /// <param name="tcdEvent"></param>
        /// <returns></returns>
        public bool fIncludes(Tcd tcdEvent)
        {
            Tcd tcdPlace = tcdEvent;
            while (tcdPlace != null)
            {
                if (tcdPlace == this)
                    return true;
                tcdPlace = tcdPlace.tcdParent;
            }
            return false;
        }

        public override string ToString()
        {
            return "<tcd " + stName + ">";
        }

        public static Tcd tcdAny = new Tcd("tcdAny", null);

        public static Tcd tcdTransferLeft = new Tcd("tcdTransferLeft", tcdAny);
        public static Tcd tcdTransferLeftCpg = new Tcd("tcdTransferLeftCpg", tcdTransferLeft);
        public static Tcd tcdTransferLeftEqs = new Tcd("tcdTransferLeftEqs", tcdTransferLeft);
        public static Tcd tcdTransferLeftEpr = new Tcd("tcdTransferLeftEpr", tcdTransferLeft);
        public static Tcd tcdTransferRight = new Tcd("tcdTransferRight", tcdAny);
        public static Tcd tcdTransferRightCpg = new Tcd("tcdTransferRightCpg", tcdTransferRight);
        public static Tcd tcdTransferRightEqs = new Tcd("tcdTransferRightEqs", tcdTransferRight);
        public static Tcd tcdTransferRightEpr = new Tcd("tcdTransferRightEpr", tcdTransferRight);
        public static Tcd tcdRegisterEtpByOffset = new Tcd("tcdRegisterEtpByOffset", tcdAny);
        public static Tcd tcdRegisterEtpByVbv = new Tcd("tcdRegisterEtpByVbv", tcdAny);
        public static Tcd tcdLaunchEur = new Tcd("tcdLaunchEur", tcdAny);
        public static Tcd tcdNewCmr = new Tcd("tcdNewCmr", tcdAny);
        public static Tcd tcdSolnToEqs = new Tcd("tcdSolnToEqs", tcdAny);
        public static Tcd tcdEqsToEnt = new Tcd("tcdEqsToEnt", tcdAny);
        public static Tcd tcdEntNextStep = new Tcd("tcdEntNextStep", tcdAny);
        public static Tcd tcdSaveForFilter = new Tcd("tcdSaveForFilter", tcdAny);
        public static Tcd tcdStartResolve = new Tcd("tcdStartResolve", tcdAny);
        public static Tcd tcdNewAsc = new Tcd("tcdNewAsc", tcdAny);
        public static Tcd tcdNewAscResolve = new Tcd("tcdNewAscResolve", tcdAny);
        public static Tcd tcdNewAscConnect = new Tcd("tcdNewAscConnect", tcdAny);
        public static Tcd tcdAscFromNgc = new Tcd("tcdAscFromNgc", tcdAny);
        public static Tcd tcdNewAscProduct = new Tcd("tcdNewAscProduct", tcdAny);
        public static Tcd tcdApplyPtiToEqs = new Tcd("tcdApplyPtiToEqs", tcdAny);
        public static Tcd tcdNewEqs = new Tcd("tcdNewEqs", tcdAny);
        public static Tcd tcdEurToNotifyEqs = new Tcd("tcdEurToNotifyEqs", tcdAny);
        public static Tcd tcdVbvForEtp = new Tcd("tcdVbvForEtp", tcdAny);
        public static Tcd tcdVbvForEpu = new Tcd("tcdVbvForEpu", tcdAny);
        public static Tcd tcdEqsToNgc = new Tcd("tcdEqsToNgc", tcdAny);
        public static Tcd tcdNewBid = new Tcd("tcdNewBid", tcdAny);
        public static Tcd tcdMapEuhSoln = new Tcd("tcdMapEuhSoln", tcdAny);
        public static Tcd tcdStartSubproblem = new Tcd("tcdStartSubproblem", tcdAny);
        public static Tcd tcdEqsForEpu = new Tcd("tcdEqsForEpu", tcdAny);
        public static Tcd tcdMakeEpu = new Tcd("tcdMakeEpu", tcdAny);
        public static Tcd tcdImmediateVbv = new Tcd("tcdImmediateVbv", tcdAny);






    }

    public interface Irp
    {
        void Report(Tcd tcdEvent, Ibd objTarget, Ibd objInput, Object objData);
    }


    public interface Irr : Irp
    {

        void AscAdded(Asc asc);

        void Proved(Asc asc);

        void NoProof();

        void QscLog(string stText);

        void EqsLog(string stText);

        bool fLogging();

        void AscCreated(Asc ascR, Shv shv);

        void GotPair(Gnb gnb, Bel belLeft, Bel belRight, Pqi pqiFound);
    }

    public class Tlr
    {
        protected StringBuilder sb = new StringBuilder();

        public virtual void WriteLine(string stText)
        {
            Debug.WriteLine(sb.ToString() + stText);
            sb.Clear();
        }

        protected void Write(string stText)
        {
            sb.Append(stText);
        }

        protected void Indent(int n)
        {
            for (int i = 0; i < n; i++)
                sb.Append(" ");
        }

    }

    /// <summary>
    /// Base action for watching that outputs the reports
    /// </summary>
    public class Tpf : Ika
    {
        protected Tlr tlr;
        public string stActionLabel;
        static int cId = 0;
        int nId = cId++;

        public Tpf(Tlr tlr, string stLabel)
        {
            this.tlr = tlr;
            this.stActionLabel = stLabel;
        }

        public static void Restart()
        {

            // tbiWatch is not cleared, so entries can be restored
            throw new RestartException();
        }

        public override string ToString()
        {
            return GetType().Name + "#" + nId + ": " + stActionLabel; 
        }

        /// <summary>
        /// Return true to remove
        /// </summary>
        public virtual bool fPerform(object objLeft, object objRight, object objData, Tcd tcdEvent)
        {
            string stLeft = objLeft.ToString();
            string stRight = objRight.ToString();
            tlr.WriteLine(this.GetType().Name + ": " + stActionLabel + " @ " + tcdEvent.stName + ": " + stLeft
                            + ", " + objRight + " / " + objData);
            return false;
        }
    }


    public class Bpr<T> where T : class
    {
        public T belLeft;
        public T belRight;

        public override bool Equals(object obj)
        {
            if (!(obj is Bpr<T>))
                return false;
            Bpr<T> bpr = (Bpr<T>)obj;
            return belLeft == bpr.belLeft && belRight == bpr.belRight;
        }

        public override int GetHashCode()
        {
           return belLeft.GetHashCode() + belRight.GetHashCode();
        }
    }


    /// <summary>
    /// Debugging code for look for duplicate 
    /// </summary>
    public class Fdp
    {
        Dictionary<Bpr<Bel>, Bpr<Bel>> mpbpr_bprBel = new Dictionary<reslab.Bpr<Bel>, reslab.Bpr<Bel>>();
        Dictionary<Bpr<Rib>, Bpr<Rib>> mpbpr_bprRib = new Dictionary<reslab.Bpr<Rib>, reslab.Bpr<Rib>>();

        public void GotPair(Gnb gnb, Bel belLeft, Bel belRight, Pqi pqiFound)
        {
            if (gnb.nId != 0)
                return;
            Bpr<Bel> bpr = new Bpr<Bel>();
            bpr.belLeft = belLeft;
            bpr.belRight = belRight;
            Bpr<Bel> bprOld;

            Debug.WriteLine("pair: " + pqiFound.stPretty()
                + " " + pqiFound.qscLefts.cPosition(belLeft)
                + "," + pqiFound.qscRights.cPosition(belRight)
                + " | " + belLeft.nId + "," + belRight.nId);
            if (mpbpr_bprBel.TryGetValue(bpr, out bprOld))
            {
                Debug.WriteLine("found duplicate Bel_s");
            }
            mpbpr_bprBel.Add(bpr, bpr);

            Bpr<Rib> bprRib = new Bpr<Rib>();
            bprRib.belLeft = belLeft.ribVal;
            bprRib.belRight = belRight.ribVal;
            Bpr<Rib> bprRibOld;
            if (mpbpr_bprRib.TryGetValue(bprRib, out bprRibOld))
            {
                Debug.WriteLine("found duplicate Rib_s");
            }
            mpbpr_bprRib.Add(bprRib, bprRib);
        }
    }

    /// <summary>
    /// Detailed logging of basic steps
    /// </summary>
    public class LogRes : Tlr, Irr
    {
        bool fLogging = false;
        public Fdp fdp;

        void WriteProof(int nIndent, Rib rib, HashSet<Rib> rgribAlready)
        {
            if (rgribAlready.Contains(rib))
                return;
            rgribAlready.Add(rib);

            if (rib is Asc)
            {
                Asc asc = (Asc)rib;
                if (asc.ribLeft != null)
                    WriteProof(nIndent + 3, asc.ribLeft, rgribAlready);
                if (asc.ribRight != null)
                    WriteProof(nIndent + 3, asc.ribRight, rgribAlready);

                Fwt sb = new Fwt();
                sb.Append("    cc.Expect(\"");
                sb.Append(asc.nId);
                sb.Append("<-");
                if (asc.ribLeft != null)
                    sb.Append(asc.ribLeft.nGetId());
                sb.Append(",");
                if (asc.ribRight != null)
                    sb.Append(asc.ribRight.nGetId());
                sb.Append("\",\"");
                asc.stString(Pctl.pctlPlain, sb);
                sb.Append("\");");
                WriteLine(sb.stString());
            }
            else if (rib is Pti)
            {
                Pti pti = (Pti)rib;
                WriteProof(nIndent, pti.ascB, rgribAlready);
            }
            else
                throw new NotImplementedException();
        }

        public virtual void AscCreated(Asc ascR, Shv shv)
        {

        }


        void Irr.Proved(Asc asc)
        {
            HashSet<Rib> rgribAlready = new HashSet<Rib>();
            WriteProof(0, asc, rgribAlready);
            WriteLine("QED" + asc);
        }

        void Irr.NoProof()
        {
            WriteLine("No Proof");
        }


        public virtual void AscAdded(Asc asc)
        {
            if (!fLogging)
                return;

            Pctl pctl = new Pctl();
            pctl.fPretty = false;

            WriteLine("");
            if (asc.ribLeft != null)
            {
                Fwt sb = new Fwt();
                sb.Append("L{" + asc.ribLeft.iprComplexity() + "} ");
                asc.ribLeft.stString(pctl, sb);
                WriteLine(sb.stString());
            }
            if (asc.ribRight != null)
            {
                Fwt sb = new Fwt();
                sb.Append("R{" + asc.ribRight.iprComplexity() + "} ");
                asc.ribRight.stString(pctl, sb);
                WriteLine(sb.stString());
            }
            Write("> ");
            Write(asc.fRight() ? "[R" : "[L");
            Write(asc.rgnTree.Length.ToString());
            Write("] ");
            {
                Fwt sb = new Fwt();
                asc.stString(pctl, sb);
                Write(sb.stString());
            }
        }

        void Irr.QscLog(string stText)
        {
        }

        bool Irr.fLogging()
        {
            return fLogging;
        }

        void Irr.EqsLog(string stText)
        {
            if (fLogging)
              WriteLine(stText);
        }

        void Irp.Report(Tcd tcdEvent, Ibd objTarget, Ibd objInput, Object objData)
        {
        }

        public void GotPair(Gnb gnb, Bel belLeft, Bel belRight, Pqi pqiFound)
        {
            if (fdp != null)
                fdp.GotPair(gnb, belLeft, belRight, pqiFound);
        }
    }

    /// <summary>
    /// Write proof in html format
    /// </summary>
    public class HtmlRes : Tlr, Irr
    {
        StreamWriter file;
        string stFileName;
        HashSet<Asc> mpasc;
        public Res res;
        Asc ascStep;

        public HtmlRes (string stFileName)
        {
            this.stFileName = stFileName;
        }

        public override void WriteLine(string stText)
        {
            file.WriteLine(sb.ToString() + stText);
            sb.Clear();
        }

        public void AscAdded(Asc asc)
        {
        }

        public void NoProof()
        {
            throw new NotImplementedException();
        }

        void FileHeader()
        {
            WriteLine("<html>");
            WriteLine("<head>");
            WriteLine("<title>");
            WriteLine("proof");
            WriteLine("</title>");
            WriteLine("</head>");
            WriteLine("<body>");
        }
        void FileFooter()
        {
            WriteLine("</body>");
            WriteLine("</html>");
        }
        void StartTable()
        {
            WriteLine("<table>");
        }
        void EndTable()
        {
            WriteLine("</table>");
        }

        string stAvcDesc(Rib ribSource, Avc avc)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ribSource.ToString());
            sb.Append("<br>");
            Ascb ascbR = new Ascb();
            ascbR.DummySizes();
            Sps sps = new Sps();
            foreach (ushort nLitOff in avc.aic.rgnTermOffset)
                Shv.ShowTerm(ascbR, avc.aic.asc, null, nLitOff, null, null, Vbv.vbvA, 
                             null, null, sps, false);
            Asc ascR = ascbR.ascBuild(avc.aic.nNegLiterals, avc.aic.nPosLiterals);

            sb.Append(ascR.ToString());
            return sb.ToString();
        }

        void Irr.AscCreated(Asc ascR, Shv shv)
        {
            if (file == null)
                return;
            if (ascStep == null)
                return;

            if (ascR.Equals(ascStep))
            {
                string stDescA = stAvcDesc(ascStep.ribLeft, shv.avcA);
                string stDescB = stAvcDesc(ascStep.ribRight, shv.avcB);
                WriteRow(ascR.ToString(), stDescA, stDescB);
                ascStep = null;
            }
        }

        void WriteRow(string stA, string stB, string stC)
        {
            Write("<tr>");
            Write("<td>");
            WriteLine(stA);
            Write("</td>");
            Write("<td>");
            if (stB != null)
                WriteLine(stB);
            Write("</td>");
            Write("<td>");
            if (stC != null)
                WriteLine(stC);
            Write("</td>");
            Write("</tr>");
        }

        void WriteProof(Rib rib)
        {

            if (rib is Asc)
            {
                Asc asc = (Asc)rib;
                if (mpasc.Contains(asc))
                    return;
                mpasc.Add(asc);

                if (asc.ribLeft == null && asc.ribRight == null)
                {
                    WriteRow(asc.ToString(), null, null);
                }
                else
                {
                    WriteProof(asc.ribLeft);
                    WriteProof(asc.ribRight);
                    ascStep = asc;
                    if (asc.ribLeft is Asc && asc.ribRight is Asc)
                    {
                        res.gnpAscAsc.ProcessPair((Asc)asc.ribLeft, (Asc)asc.ribRight);
                    }
                    else
                        throw new NotImplementedException();
                    if (ascStep != null)
                        WriteRow(asc.ToString() + "could not replicate", asc.ribLeft.ToString(), asc.ribRight.ToString());
                }
            }
            else if (rib is Pti)
            {
                Pti pti = (Pti)rib;
                WriteProof(pti.ascB);
            }
            else
                throw new NotImplementedException();
        }

        public void Proved(Asc asc)
        {
            using (StreamWriter sr = new StreamWriter(stFileName))
            {
                file = sr;
                mpasc = new HashSet<Asc>();
                FileHeader();
                StartTable();
                WriteProof(asc);
                EndTable();
                WriteLine("QED");
                FileFooter();
                file = null;
            }
            mpasc = null;
        }

        public void QscLog(string stText)
        {
        }

        public bool fLogging()
        {
            return true;
        }

        public void EqsLog(string stText)
        {
        }

        void Irp.Report(Tcd tcdEvent, Ibd objTarget, Ibd objInput, Object objData)
        {
        }

        public void GotPair(Gnb gnb, Bel belLeft, Bel belRight, Pqi pqiFound)
        {
        }
    }

    /// <summary>
    /// Action to perform with Hkp that has been encountered.
    /// </summary>
    public interface Ika
    {
        /// <summary>
        /// Return true to remove
        /// </summary>
        bool fPerform(Object objLeft, Object objRight, Object objData, Tcd tcdEvent);
    }

    public class Afk
    {
        public Ika ikaAction;
        public Tcd tcdTag;
    }

    public abstract class Hkb
    {
        List<Afk> rgafkCases;

        public Ika ikaFindForTcd(Tcd tcdEvent)
        {
            foreach (Afk afk in rgafkCases)
            {
                if (afk.tcdTag.fIncludes(tcdEvent))
                    return afk.ikaAction;
            }
            return null;
        }

        public void Remove(Tcd tcdEvent)
        {
            Afk afkToRemove = null;
            foreach (Afk afk in rgafkCases)
            {
                if (afk.tcdTag.fIncludes(tcdEvent))
                {
                    afkToRemove = afk;
                    break;
                }
            }
            if (afkToRemove != null)
                rgafkCases.Remove(afkToRemove);
        }

        public void AddTcdIka(Dictionary<Hkb, Hkb> mphkb, Tcd tcdEvent, Ika ikaAction)
        {
            Hkb hkbOld;
            if (!mphkb.TryGetValue(this, out hkbOld))
            {
                rgafkCases = new List<Afk>();
                hkbOld = this;
                mphkb.Add(this, this);
            }
            Afk afkNew = new Afk();
            afkNew.tcdTag = tcdEvent;
            afkNew.ikaAction = ikaAction;
            hkbOld.rgafkCases.Add(afkNew);

        }

        public void LookupAndPerform(Dictionary<Hkb, Hkb> mphks,
                                     Object objTarget, Object objInput, Object objData, Tcd tcdEvent)
        {
            Hkb hkbOld;
            if (mphks.TryGetValue(this, out hkbOld))
            {
                Ika ikaAction = hkbOld.ikaFindForTcd(tcdEvent);
                if (ikaAction != null)
                {
                    bool fRemove = false;
                    try
                    {
                        fRemove = ikaAction.fPerform(objTarget, objInput, objData, tcdEvent);
                    }
                    catch (Exception e)
                    {
                        hkbOld.Remove(tcdEvent);
                        throw e;
                    }
                    if (fRemove)
                        hkbOld.Remove(tcdEvent);
                }
            }

        }

    }

    /// <summary>
    /// Hash entry to check for which events to log for pairs of objects.
    /// The target is the this for TransferLeft or TransferRight.
    /// </summary>
    public class Hkp : Hkb
    {
        public Object objTarget;
        public Object objInput;

        public override bool Equals (Object other)
        {
            if (!(other is Hkp))
                return false;
            Hkp hkpOther = (Hkp)other;
            if (!objTarget.Equals(hkpOther.objTarget))
                return false;
            if (!objInput.Equals(hkpOther.objInput))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            int nAcc = objTarget.GetHashCode() * 3;
            nAcc ^= objInput.GetHashCode() * 5;
            return nAcc;
        }
        public override string ToString()
        {
            return "{" + GetType().Name + " " + objTarget + " " + objInput + "}";
        }
    }

    /// <summary>
    /// Hash entry to check for which events to log for one object and any other
    /// objSingle is either the target or the input.
    /// </summary>
    public class Hks : Hkb
    {
        public readonly Object objSingle;

        public Hks(Object objSingle)
        {
            this.objSingle = objSingle;
        }

        public override bool Equals(Object other)
        {
            if (!(other is Hks))
                return false;
            Hks hkpOther = (Hks)other;
            if (!objSingle.Equals(hkpOther.objSingle))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            int nAcc = objSingle.GetHashCode() * 3;
            return nAcc;
        }
        public override string ToString()
        {
            return "{" + GetType().Name + " " + objSingle + "}";
        }

    }

    public class Ctr
    {
        public int nNum;
    }

    /// <summary>
    /// Register to watch specific low level events.
    /// </summary>
    public class Drt : Tlr, Irr
    {
        Dictionary<Hkb, Hkb> mphkp_ikaWatchPair = new Dictionary<Hkb, Hkb>();
        Dictionary<Hkb, Hkb> mphks_ikaWatchTarget = new Dictionary<Hkb, Hkb>();
        Dictionary<Hkb, Hkb> mphks_ikaWatchInput = new Dictionary<Hkb, Hkb>();
        Dictionary<Tcd, Ctr> mpdrt_tlrCountReports = new Dictionary<Tcd, Ctr>();

        const int nNullId = -1;

        public Drt()
        {
        }

        public void WatchPair(Ibd objTarget, Ibd objInput, Tcd tcdEvent, Ika ikaAction)
        {
            Hkp hkp = new Hkp();
            hkp.objTarget = objTarget == null ? nNullId : objTarget.nGetId();
            hkp.objInput = objInput == null ? nNullId : objInput.nGetId();
            hkp.AddTcdIka(mphkp_ikaWatchPair, tcdEvent, ikaAction);
        }



        public void WatchSingle(Ibd objSingle, bool fInput, Tcd tcdEvent, Ika ikaAction)
        {
            Hks hks = new Hks(objSingle.nGetId());
            Dictionary<Hkb, Hkb> mphks_ikaSide = fInput ? mphks_ikaWatchInput : mphks_ikaWatchTarget;
            hks.AddTcdIka(mphks_ikaSide, tcdEvent, ikaAction);
        }

        public void WatchTarget(Ibd objSingle, Tcd tcdEvent, Ika ikaAction)
        {
            WatchSingle(objSingle, false, tcdEvent, ikaAction);
        }

        public void WatchInput(Ibd objSingle, Tcd tcdEvent, Ika ikaAction)
        {
            WatchSingle(objSingle, true, tcdEvent, ikaAction);
        }

        void CheckAndPerformSingle(Ibd objTarget, Ibd objInput, Object objData,
                                   bool fCheckInput, Tcd tcdEvent)
        {
            Ibd objRef = (fCheckInput ? objInput : objTarget);
            if (objRef != null)
            {
                Hks hks = new Hks(objRef.nGetId());
                Dictionary<Hkb, Hkb> mphks_ikaSide = fCheckInput ? mphks_ikaWatchInput : mphks_ikaWatchTarget;
                hks.LookupAndPerform(mphks_ikaSide, objTarget, objInput, objData, tcdEvent);
            }
        }

        public void Report(Tcd tcdEvent, Ibd objTarget, Ibd objInput, Object objData)
        {
            ShowCounts(tcdEvent);
            CheckAndPerformSingle(objTarget, objInput, objData, false, tcdEvent);
            CheckAndPerformSingle(objTarget, objInput, objData, true, tcdEvent);
            // if (objTarget != null && objInput != null)
            {
                Hkp hkp = new Hkp();
                hkp.objTarget = objTarget == null ? nNullId : objTarget.nGetId();
                hkp.objInput = objInput == null ? nNullId : objInput.nGetId();
                hkp.LookupAndPerform(mphkp_ikaWatchPair, objTarget, objInput, objData, tcdEvent);
            }
        }

        void Irr.AscAdded(Asc asc)
        {
        }

        void Irr.AscCreated(Asc ascR, Shv shv)
        {
        }

        void Irr.EqsLog(string stText)
        {
        }

        bool Irr.fLogging()
        {
            return false;
        }

        void Irr.NoProof()
        {
        }

        void Irr.Proved(Asc asc)
        {
            WriteLine("QED" + asc);
        }

        void Irr.QscLog(string stText)
        {
        }

        void ShowCounts(Tcd tcdEvent)
        {
            Ctr ctrValue;
            if (mpdrt_tlrCountReports.TryGetValue(tcdEvent, out ctrValue))
            {
                ctrValue.nNum++;
                if (ctrValue.nNum % 1000000 == 0)
                {
                    Debug.WriteLine("\nReport: " );
                    Counts();
                }
            }
            else
            {
                ctrValue = new Ctr();
                ctrValue.nNum = 1;
                mpdrt_tlrCountReports.Add(tcdEvent, ctrValue);
            }
        }

        public void Counts()
        {
            foreach (KeyValuePair<Tcd,Ctr> kvp in mpdrt_tlrCountReports)
            {
                Debug.WriteLine(kvp.Value.nNum + "\t" + kvp.Key.stName);
            }
        }

        public void GotPair(Gnb gnb, Bel belLeft, Bel belRight, Pqi pqiFound)
        {
        }
    }
}
