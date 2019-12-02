// #define LOG
using GraphMatching;
using System.Diagnostics;
using System.Text;

namespace reslab
{

    /// <summary>
    /// Save status for backtracking during fEmbed
    /// </summary>
    public class Ams
    {
        public ushort nTermB;
        public Tde tdeLatestSbst;

        public void Reset(Tde tdeSaved = null)
        {
            Tde tdeSbst = tdeLatestSbst;
            while (tdeSbst != tdeSaved)
            {
                tdeSbst.nValue = Tde.nNoValue;
                tdeSbst = tdeSbst.tdePrevSbst;
            }
            tdeLatestSbst = tdeSaved;
        }

    }

#if false
    /// <summary>
    /// Hold new vbl ids used in producing new clause from terms in an old one.
    /// Is stored separate from Aac so those are not modified when building new clauses.
    /// </summary>
    public class Avc
    {
        public sbyte[] rgnOutputVarId;   // new vbl ids. entries correspond to indices used for Aac.rgtdeData
        public Aac aac;

        public Avc (Aac aac)
        {
            this.aac = aac;
            rgnOutputVarId = new sbyte[aac.rgtdeData.Length];
            for (int i = 0; i < rgnOutputVarId.Length; i++)
                rgnOutputVarId[i] = Tde.nNoVblId;
        }
    }
#endif

    /// <summary>
    /// Hold the values assigned to variables in a clause, and all other info for clause.
    /// </summary>
    public class Avc : Bid
    {
        public Tde[] rgtdeData;   // variable values. indexed by the non-negative entries in aic.asc.rgnTree
        public Aic aic;

        public Avc(Aic aic)
        {
            this.aic = aic;
            if (aic != null)
            {
                Asc asc = aic.asc;
                rgtdeData = new Tde[aic.nNumVbls];
                for (int i = 0; i < aic.nNumVbls; i++)
                {
                    rgtdeData[i] = new Tde();
                }
            }
        }

        public override int GetHashCode()
        {
            int nAcc = rgtdeData.Length;
            foreach (Tde tde in rgtdeData)
            {
                nAcc += tde.GetHashCode();
            }
            nAcc += aic.GetHashCode();
            return nAcc;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Avc))
                return false;
            Avc avc = (Avc)obj;
            if (rgtdeData.Length != avc.rgtdeData.Length)
            {
                for (int i = 0; i < rgtdeData.Length; i++)
                {
                    if (!rgtdeData[i].Equals(avc.rgtdeData[i]))
                        return false;
                }
            }
            if (!aic.asc.Equals(avc.aic.asc))
                return false;
            return true;
        }

        public void ClearValues()
        {
            foreach (Tde tde in rgtdeData)
                tde.nValue = Tde.nNoValue;
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append("[");
            bool fFirst = true;
            foreach (Tde tde in rgtdeData)
            {
                if (fFirst)
                    fFirst = false;
                else
                    sb.Append(",");
                if (tde.avcValue == this)
                    sb.Append("#");
                sb.Append(tde.nValue);
            }
            sb.Append(" ");
            sb.Append(aic.ToString());
            sb.Append("]");
        }
    }

    /// <summary>
    /// Hold information that can be computed directly from an Asc. These could be cached.
    /// </summary>
    public class Aic : Bid
    {
        public readonly Asc asc;


        public ushort[] rgnTermOffset;   // location of start of each term in clause
        public byte[] rgnTermSize;             // num bytes in each term and subterm

        public byte nLiterals;
        public byte nNegLiterals;
        public byte nPosLiterals;
        public int nNumVbls = -1;

        public Aic (Asc asc)
        {
            this.asc = asc;
            GetTermPosns();
        }

        void GetTermPosns()
        {
            rgnTermSize = new byte[asc.rgnTree.Length];
            nNegLiterals = (byte) asc.rgnTree[Asc.nPosnNumNegTerms];
            nPosLiterals = (byte) asc.rgnTree[Asc.nPosnNumPosTerms];
            nLiterals = (byte) (nNegLiterals + nPosLiterals);
            rgnTermOffset = new ushort[nLiterals];
            ushort nOffset = Asb.nClauseLeadingSizeNumbers;
            byte nTermNum = 0;
            asc.GetTermOffsets(this, ref nTermNum, nNegLiterals, ref nOffset);
            asc.GetTermOffsets(this, ref nTermNum, nPosLiterals, ref nOffset);

            for (int i = Asb.nClauseLeadingSizeNumbers; i < asc.rgnTree.Length; i++)
            {
                sbyte nV = asc.rgnTree[i];
                if (nV > nNumVbls)
                    nNumVbls = nV;
            }
            nNumVbls = nNumVbls + 1;    // first vbl is 0
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            asc.stString(pctl, sb);
        }
    }


    /// <summary>
    /// Hold position information for one of the clauses being equated. Forms a stack for handling substitutions
    /// </summary>
    public class Aqs
#if DEBUG
        : Bid
#endif
    {
        public Aqs aqsPrev;
        public ushort nLimit;
        public Avc avc;
        public ushort nOffset;
        public const sbyte EOS = 127;
        public sbyte nSavedId = EOS;   // EOS indicate not used
        public bool fEmpty = false;

        public Aqs(Aqs aqsPrev, ushort nLimit, Avc avc, int nOffset)
        {
            this.aqsPrev = aqsPrev;
            this.avc = avc;
            this.nOffset = (ushort)nOffset;
            this.nLimit = nLimit;
        }

        public Aqs(Avc avc, int nOffset)
        {
            this.avc = avc;
            this.nOffset = (ushort)nOffset;
            nLimit = (ushort) (nOffset + avc.aic.rgnTermSize[nOffset]);
        }

        public Aqs aqsCopy()
        {
            Aqs aqsNew = new Aqs(null, nLimit, avc, nOffset);
            aqsNew.fEmpty = fEmpty;
            return aqsNew;
        }

        public static sbyte nIdNext(ref Aqs aqs)
        {
            while (aqs.nOffset >= aqs.nLimit)  // end of sbst value
            {
                if (aqs.aqsPrev == null)
                {
                    aqs.fEmpty = true;
                    return EOS;
                }
                aqs = aqs.aqsPrev;
            }

            Asb asb = aqs.avc.aic.asc;
            sbyte nId = asb.rgnTree[aqs.nOffset++];
            return nId;
        }

#if DEBUG
        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append("{");
            sb.Append(nOffset);
            sb.Append("/");
            sb.Append(nLimit);
            sb.Append(" ");
            if (fEmpty)
            {
                sb.Append("empty ");
            }
            avc.aic.asc.stString(pctl, sb);
            sb.Append("}");
        }
#endif
        /// <summary>
        /// Move this forward by distance traveled by child
        /// </summary>
        public void Advance(Aqs aqsChild)
        {
            Debug.Assert(avc == aqsChild.avc);
            nOffset = aqsChild.nOffset;
        }

        public static void UseVblValue(ref Aqs aqsVbl, Tde tdeWhereSaved) // , Aqs aqsOther)
        {
            // Debug.Assert(!fOffsetAlreadyPushed(aqsTop, aic, (ushort)(nOffset + 1)));

            byte nNewTermSize = tdeWhereSaved.avcValue.aic.rgnTermSize[tdeWhereSaved.nValue];

            aqsVbl = new Aqs(aqsVbl, (ushort) (tdeWhereSaved.nValue + nNewTermSize), tdeWhereSaved.avcValue, tdeWhereSaved.nValue);
        }

        public static void BindVblValue(Aqs aqsValue, Tde tdeWhereToSave, Ams amsTop)
        {
            ushort nOffsetValue = (ushort)(aqsValue.nOffset - 1);  // already incremented
            tdeWhereToSave.avcValue = aqsValue.avc;
            tdeWhereToSave.nValue = nOffsetValue;
            tdeWhereToSave.tdePrevSbst = amsTop.tdeLatestSbst;
            amsTop.tdeLatestSbst = tdeWhereToSave;

            byte nTermSize = aqsValue.avc.aic.rgnTermSize[nOffsetValue];
#if LOG
            Eqm.Log("BindVblValue" + aqsValue + " + " + nTermSize);
#endif
            aqsValue.nOffset = (ushort)(nOffsetValue + nTermSize);

        }
    }



    public class Abt
    {
        public readonly Avc avcA;
        public readonly Avc avcB;
        readonly Asc ascNoVar;  // set if one of the Aic is considered to value no variables (e.g. embed)

        public int nMaxOffsetA;   // offset where mistmatch occurred, with best selected of terms to match

        public Abt(Asc ascLeft, Asc ascRight, Res res, Asc ascNoVar = null)
            : this (new Avc(new Aic(ascLeft)),
                    ascRight == null ? null : new Avc(new Aic(ascRight)), ascNoVar)
        {
        }

        public Abt(Avc avcLeft, Avc avcRight, Asc ascNoVar = null)
        {
            this.avcA = avcLeft;
            this.avcB = avcRight;
            this.ascNoVar = ascNoVar;
        }

        /// <summary>
        /// Avoid having to start from the beginning of next try of embed
        /// </summary>
        void UpdateHighwater(Aqs aqsA)
        {
            while (aqsA.aqsPrev != null)
                aqsA = aqsA.aqsPrev;   // want offset where variable was first substituted
            if (nMaxOffsetA < aqsA.nOffset - 1)
                nMaxOffsetA = aqsA.nOffset - 1;
        }


        public bool fEquateEmbed(ushort nOffsetA, ushort nOffsetB, Ams amsTop)  // pg 271
        {
            Aqs aqsA = new Aqs(avcA, nOffsetA);
            Aqs aqsB = new Aqs(avcB, nOffsetB);

            sbyte nIdA = 0;
            sbyte nIdB = 0;
            bool fKeepB = false;
            while (true)
            {
                nIdA = Aqs.nIdNext(ref aqsA);

                if (fKeepB)
                    fKeepB = false;
                else
                    nIdB = Aqs.nIdNext(ref aqsB);

                if (aqsA.fEmpty)
                {
                    if (aqsB.fEmpty)
                        return true;
                    UpdateHighwater(aqsA);
                    return false;
                }
                else if (aqsB.fEmpty)
                {
                    UpdateHighwater(aqsA);
                    return false;
                }

                if (aqsA.avc == aqsB.avc && nIdA == nIdB)
                { }   // vbl can match itself after sbst
                else if (nIdA >= Asb.nVar)
                {
                    if (aqsA.avc.aic.asc != ascNoVar)
                    {
                        Tde tdeA = aqsA.avc.rgtdeData[nIdA];

                        if (tdeA.fIsBound())
                        {
                            Aqs.UseVblValue(ref aqsA, tdeA);
                            fKeepB = true;
                        }
                        else
                        {
                            Aqs.BindVblValue(aqsB, tdeA, amsTop);
                        }
                    }
                    else
                    {
                        // a B vbl is on the A side, treat it as a non-vbl, but there is no Lsm for it. Can only match itself
                        if (nIdA != nIdB)
                        {
                            UpdateHighwater(aqsA);
                            return false;
                        }
                    }
                }
                else if (nIdB >= Asb.nVar)
                {
                    UpdateHighwater(aqsA);
                    return false;
                }
                else
                {
                    int nLsmA = Asb.nLsmId - nIdA;
                    int nLsmB = Asb.nLsmId - nIdB;
                    if (aqsA.avc.aic.asc.rglsmData[nLsmA] != aqsB.avc.aic.asc.rglsmData[nLsmB])
                    {
                        UpdateHighwater(aqsA);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Determine the number of terms that appear in the first nPrefixLen bytes (rounded up)
        /// </summary>
        int nNumPrefixTermsA (int nPrefixLen)
        {
            int nNumTerms = 0;
            while (nNumTerms < avcA.aic.nLiterals && avcA.aic.rgnTermOffset[nNumTerms] < nPrefixLen)
                nNumTerms++;
            return nNumTerms;
        }

        public const int nNoPrefix = -1;

        /// <summary>
        /// Look for permutation of A that equates with a subset of B.
        /// </summary>
        /// <returns></returns>
        public bool fEmbed()  // pg 277
        {
            int nNumATerms = avcA.aic.nLiterals;
            int nNumANegTerms = avcA.aic.nNegLiterals;
            nMaxOffsetA = 0;

            if (nNumATerms == 0)
                return true;
            ushort nOffsetA = 2;
            ushort nOffsetB = 2;
            Ams[] rgamsStackA = new Ams[nNumATerms];
            int nANum = 0;

            for (int i = 0; i < nNumATerms; i++)
                rgamsStackA[i] = new Ams();

            avcA.ClearValues();


            /*
                iterate over negs first,
                when end reached, iterate over pos
                when popping back, switch back to negs
            */

            Ams amsTop = rgamsStackA[0];
            amsTop.nTermB = (nNumANegTerms == 0) ? (ushort)avcB.aic.nNegLiterals : (ushort)0;
            while (true)
            {
                ushort nNextB = amsTop.nTermB;
                int nStopB = (nANum < nNumANegTerms) ? avcB.aic.nNegLiterals : avcB.aic.nLiterals; // is current term in A on pos or neg side?
                if (nNextB >= nStopB)
                {
                    nOffsetA = avcA.aic.rgnTermOffset[nANum];
                    if (nMaxOffsetA < nOffsetA)
                        nMaxOffsetA = nOffsetA;

                    if (nANum == 0)
                        break;
                    nANum--;
                    amsTop = rgamsStackA[nANum];
                    amsTop.Reset();
                    amsTop.nTermB++;
                }
                else
                {
                    amsTop.nTermB = nNextB;

                    nOffsetA = avcA.aic.rgnTermOffset[nANum];
                    nOffsetB = avcB.aic.rgnTermOffset[amsTop.nTermB];
                    if (fEquateEmbed(nOffsetA, nOffsetB, amsTop))
                    {
                        nANum++;
                        if (nANum == nNumATerms)
                        {
                            while (nANum > 0)
                                rgamsStackA[--nANum].Reset();
                            return true;
                        }
                        amsTop = rgamsStackA[nANum];
                        if (nANum >= nNumANegTerms)
                            amsTop.nTermB = (ushort)avcB.aic.nNegLiterals;
                        else
                            amsTop.nTermB = 0;
                    }
                    else
                    {
                        amsTop.Reset();
                        amsTop.nTermB++;
                    }
                }
            }
            return false;
        }

    }
}
