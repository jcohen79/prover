using GrammarDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab
{
    /// <summary>
    /// Simplify creating a test case
    /// </summary>
    public class Cbd
    {
        public LParse lparse;

        public static string[] rgstVarNames = { "w", "x", "y", "z",
            "U", "V", "W", "X", "Y", "Z", "TT", "UU", "VV", "WW", "XX", "YY", "ZZ" };

        public Cbd(LParse lparse = null)
        {
            if (lparse != null)
                this.lparse = lparse;
            else
            {
                this.lparse = new LParse();
                Sko.AddSyms(this.lparse, new ExpressionEvaluatorGrammar());
                foreach (string st in rgstVarNames)
                    AddVar(this.lparse, st);
            }
        }

        public static void AddVar(LParse res, string stName)
        {
            Lsm lsmx = new Lsm(stName);
            lsmx.MakeVariable();
            res.AddSym(lsmx, null);
        }

        public Lsx lsxParse(string stText)
        {
            return lparse.lsxParse(stText);
        }

        public Asc ascBuild(string stText)
        {
            Lsx lsxA = lparse.lsxParse(stText);
            Asc ascA = Asc.ascFromLsx(lsxA);
            return ascA;
        }

        public Avc avcBuild(string stText)
        {
            Lsx lsxA = lparse.lsxParse(stText);
            Asc ascA = Asc.ascFromLsx(lsxA);
            return new Avc(new Aic(ascA));
        }
    }
}
