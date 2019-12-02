
namespace reslab
{
    public class BasPL : Bas
    {
        public BasPL(Gnb gnp, Bab basPrev, Ipr iprSize) : base(gnp, false, basPrev, iprSize)
        {
        }
    }
    public class BasPR : Bas
    {
        public BasPR(Gnb gnp, Bab basPrev, Ipr iprSize) : base(gnp, true, basPrev, iprSize)
        {
        }
    }

    public class OpcReduceLength : Opc
    {
        public static OpcReduceLength opcOnly = new OpcReduceLength();

        /// <summary>
        /// 
        /// </summary>
        public bool fUseOnDemand(Pti ptiNew)
        {
            Asc ascB = ptiNew.ascB;
            sbyte nDefinedId = ascB.rgnTree[ptiNew.nFromOffset];
            sbyte nValueId = ascB.rgnTree[ptiNew.nToOffset];

            // check if this Pti should be done in Equate or using Bas
            if (nDefinedId >= Asb.nVar)
            {
                return false;   // handle all x=f(x) as Bas
            }
            else if (nValueId >= Asb.nVar)
            {
                return nDefinedId < nValueId;
            }
            else
            {
                // return true if the To is certain to be smaller than From.
                // For each var in From, the number of occurences of that var in To is less equal
                Aic aic = new Aic(ascB);
                int nFromTermSize = aic.rgnTermSize[ptiNew.nFromOffset];
                int nToTermSize = aic.rgnTermSize[ptiNew.nToOffset];
                if (nToTermSize > nFromTermSize)
                    return false;  // could this be made tigher by considering vbls?

                for (int nVbl = Asc.nVar; nVbl < aic.nNumVbls; nVbl++)
                {
                    int cNumOccursVblFrom = 0;
                    for (int nFromPos = ptiNew.nFromOffset; nFromPos < ptiNew.nFromOffset + nFromTermSize; nFromPos++)
                    {
                        if (ascB.rgnTree[nFromPos] == nVbl)
                            cNumOccursVblFrom++;
                    }
                    if (cNumOccursVblFrom == 0)
                        continue;

                    int cNumOccursVblTo = 0;
                    for (int nToPos = ptiNew.nToOffset; nToPos < ptiNew.nToOffset + nToTermSize; nToPos++)
                    {
                        if (ascB.rgnTree[nToPos] == nVbl)
                            cNumOccursVblTo++;
                    }
                    if (cNumOccursVblTo > cNumOccursVblFrom)
                        return false;
                }
                return true;
            }
        }
    }
}
