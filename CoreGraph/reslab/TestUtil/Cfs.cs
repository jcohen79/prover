using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab.TestUtil
{


    public class WatchAsc : Ika
    {
        Cbd cbd;
        int nNumAsc;
        string stList;
        Lsx[] rglsxClause;
        Asc[] rgascExpected;
        Asc[] rgascFound;

        public WatchAsc(Cbd cbd, string stInput, string stExpected)
        {
            this.cbd = cbd;
            this.stList = stExpected;
        }

        public void Setup()
        {
            Lpr lprList = (Lpr)cbd.lparse.lsxParse(stList);
            nNumAsc = lprList.nLength();
            rglsxClause = new Lsx[nNumAsc];
            rgascExpected = new Asc[nNumAsc];
            rgascFound = new Asc[nNumAsc];
            for (int nNum = 0; nNum < nNumAsc; nNum++)
            {
                rglsxClause[nNum] = lprList.lsxCar;
                rgascExpected[nNum] = Asc.ascFromLsx(lprList.lsxCar);
                Lsx lsxNext = lprList.lsxCdr;
                if (lsxNext != Lsm.lsmNil)
                    lprList = (Lpr)lsxNext;
                Debug.WriteLine("looking for " + nNum + ": " + rgascExpected[nNum]);
            }
        }

        public void Report(Asc ascNew)
        {
            for (int nNum = 0; nNum < nNumAsc; nNum++)
            {
                if (rgascFound[nNum] != null)
                { }
                else if (rgascExpected[nNum].Equals(ascNew))
                {
                    rgascFound[nNum] = ascNew;
                    Debug.WriteLine("found " + ascNew.nId + ": " + ascNew);
                    StringBuilder sb = new StringBuilder();
                    sb.Append("have: ");
                    for (int nPrev = 0; nPrev < nNumAsc; nPrev++)
                    {
                        if (rgascFound[nPrev] != null)
                        {
                            sb.Append(nPrev);
                            sb.Append(", ");
                        }
                    }
                    Debug.WriteLine(sb.ToString());
                }
            }
        }

        public bool fPerform(object objLeft, object objRight, object objData, Tcd tcdEvent)
        {
            Report((Asc)objRight);
            return false;
        }
    }

    /// <summary>
    /// Count the objects in Gnp ahead of given object
    /// </summary>
    public class Cfs : Ffs
    {
        public bool fFoundBas = false;
        public int cBasBefore = 0;
        public int cTotalInBaseBefore = 0;
        public int cBeforeInSameBas = 0;
        public int cUnfilteredBefore = 0;
        bool fDoRight;
        string stLabel;

        public Cfs(string stLabel, bool fDoRight, Rib ribNeedle) : base(ribNeedle)
        {
            this.stLabel = stLabel;
            this.fDoRight = fDoRight;
        }

        public override bool fRight()
        {
            return fDoRight;
        }

        public override bool fCheckBas(Bas basHaystack)
        {
            bool fSameBas = base.fCheckBas(basHaystack);
            if (fSameBas)
            {
                fFoundBas = true;
                cBeforeInSameBas = basHaystack.cCountBefore(ribNeedle);
            }
            else
            {
                cBasBefore++;
                cTotalInBaseBefore += Bas.nNumClauses(basHaystack);
                cUnfilteredBefore += basHaystack.cCountUnfiltered();
            }
            return fSameBas;
        }

        public override bool fCheck(Rib ribArg)
        {
            fFound = ribNeedle.Equals(ribArg);
            return fFound;
        }

        public string stResult()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(stLabel);
            sb.Append(" foundBas: ");
            sb.Append(fFoundBas ? "true" : "false");
            sb.Append(", found: ");
            sb.Append(fFound ? "true" : "false");
            sb.Append(", cBasBefore: ");
            sb.Append(cBasBefore);
            sb.Append(", cTotalInBaseBefore: ");
            sb.Append(cTotalInBaseBefore);
            sb.Append(", cBeforeInSameBas: ");
            sb.Append(cBeforeInSameBas);
            sb.Append(", cUnfilteredBefore: ");
            sb.Append(cUnfilteredBefore);
            sb.Append(" ");
            sb.Append(ribNeedle);
            sb.Append("]");
            return sb.ToString();
        }
    }

}
