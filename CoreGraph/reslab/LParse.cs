using GrammarDLL;
using System;
using System.Collections.Generic;

namespace reslab
{
    public class LParse
    {

        private Dictionary<string, Lsm> mpst_lsmSymbols;

        public void AddSym(Lsm lsm, TokenInfo token)
        {
            if (mpst_lsmSymbols == null)
                mpst_lsmSymbols = new Dictionary<string, Lsm>();
            mpst_lsmSymbols.Add(lsm.stName, lsm);

            if (token != null)
                token.ilsLiteral = lsm;
        }

        public Lsm lsmGet(string stText)
        {
            Lsm lsm = null;
            mpst_lsmSymbols.TryGetValue(stText, out lsm);
            return lsm;
        }

        public Lsm lsmObtain(string stText)
        {
            if (stText.Equals("nil"))
                return Lsm.lsmNil;
            else
            {
                Lsm lsm;
                if (mpst_lsmSymbols == null)
                    mpst_lsmSymbols = new Dictionary<string, Lsm>();
                if (!mpst_lsmSymbols.TryGetValue(stText, out lsm))
                {
                    lsm = new Lsm(stText);
                    if (stText.StartsWith(Lsm.stVarPrefix))
                        lsm.MakeVariable();
                    else if (stText.StartsWith(Lsm.stSkolemPrefix))
                        lsm.MakeSkolemFunction();
                    mpst_lsmSymbols.Add(stText, lsm);
                }
                return lsm;
            }
        }

        public Lsx lsxParse(string stText, bool fInList, int iPos, out int iNext)
        {
            Lsx lsxResult = null;
            iNext = iPos;
            Lpr lprHead = null;
            Lpr lprTail = null;
            bool fDotted = false;
            while (iNext < stText.Length)
            {
                char c = stText[iNext++];
                if (Char.IsWhiteSpace(c))
                    continue;
                if (c == '(')
                {
                    int iSub;
                    lsxResult = lsxParse(stText, true, iNext, out iSub);
                    iNext = iSub;
                }
                else if (c == ')')
                {
                    if (lprHead == null)
                        return Lsm.lsmNil;
                    return lprHead;
                }
                else if (c == '.')
                {
                    if (!fInList)
                        throw new NotSupportedException();
                    fDotted = true;
                    continue;
                }
                else if (Char.IsDigit(c))
                {
                    int nAcc = c - '0';
                    while (iNext < stText.Length)
                    {
                        char cd = stText[iNext++];
                        if (Char.IsDigit(cd))
                        {
                            nAcc = 10 * nAcc + (cd - '0');
                        }
                        else
                        {
                            iNext--;
                            break;
                        }
                    }
                    lsxResult = new Li(nAcc);
                    if (!fInList)
                        return lsxResult;
                }
                else
                {
                    int iStart = iNext - 1;
                    while (iNext < stText.Length)
                    {
                        char cw = stText[iNext++];
                        if (cw == ')' || cw == '(' || cw == '.' || Char.IsWhiteSpace(cw))
                        {
                            iNext--;
                            break;
                        }
                    }
                    string stSym = stText.Substring(iStart, iNext - iStart);
                    lsxResult = lsmObtain(stSym);
                    if (!fInList)
                        return lsxResult;
                }

                if (fDotted)
                {
                    lprTail.lsxCdr = lsxResult;
                }
                else if (fInList)
                {
                    Lpr lprNew = new Lpr(lsxResult, Lsm.lsmNil);
                    if (lprHead == null)
                        lprHead = lprNew;
                    else
                        lprTail.lsxCdr = lprNew;
                    lprTail = lprNew;
                    lsxResult = lprHead;
                }
                else
                    break;
            }
            return lsxResult;
        }
        public Lsx lsxParse(string stText)
        {
            int iPos = 0;
            int iNext;
            return lsxParse(stText, false, iPos, out iNext);
        }
    }
}
