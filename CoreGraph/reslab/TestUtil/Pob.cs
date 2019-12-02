
using GraphMatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace reslab.TestUtil
{
    /// <summary>
    /// Base class shared by steps and actions in proof outline 
    /// </summary>
    public class Pob
    {
        private static int cId = 0;
        public readonly int nId = cId++;
        private int nGenIdV = -1;

        public int nGetId()
        {
            return nGenIdV;
        }

        public void SetGenId(int nGenId)
        {
            this.nGenIdV = nGenId;
        }
    }

    /// <summary>
    /// Base class for actions in building proof outlines
    /// </summary>
    public abstract class Pst : Pob
    {
        public Pst pobNext; // next action in same stmt



        public abstract string stTypeName();

        public abstract bool fPerform(Iss iss, Tfc tfc, Psb psb);

        public abstract void PerformStep(Tfc tfc);
    }

    /// <summary>
    /// Information about the type of usage of an argument
    /// </summary>
    public class Uty
    {
        public readonly string stSetterName;

        public static Uty utyTarget = new Uty("Target");
        public static Uty utyInput = new Uty("Input");
        public static Uty utyLeft = new Uty("Left");
        public static Uty utyRight = new Uty("Right");
        public static Uty utyToWatch = new Uty("ToWatch");
        public static Uty utyKValue = new Uty("KValue");
        public static Uty utyValue = new Uty("Value");
        public static Uty utyResult = new Uty("Result");
        public static Uty utyArgs = new Uty("Args");
        public static Uty utyName = new Uty("Name");
        public static Uty utyTinName = new Uty("TinName");

        public Uty(string stName)
        {
            this.stSetterName = "Set" + stName;
        }
    }

    /// <summary>
    /// Base class for top level steps of proof outline
    /// </summary>
    public abstract class Psb : Pob, Isb
    {
        // TODO: the following should go into Tst (so not in Min)
        public Pst pobHead;
        public Pst pobTail;

        static int nMaxAction = -1;

        public void Add(Pst pobChild)
        {
            if (pobHead == null)
                pobHead = pobChild;
            else
                pobTail.pobNext = pobChild;
            pobTail = pobChild;
        }

        public bool fPerform(Iss iss, Tfc tfc)
        {
            for (Pst pob = pobHead; pob != null; pob = pob.pobNext)
            {


                Debug.WriteLine("perform " + pob.GetType().Name + "#" + pob.nId + " (" + nMaxAction + ")");
                pob.fPerform(iss, tfc, this);
                if (nMaxAction < pob.nId)
                    nMaxAction = pob.nId;
            }
            return true;
        }
    }

    public interface Ivb
    {
        string stGetName();
        void SetV(Ibd ibd);
    }

    public interface Ivh<T> : Ivb, Vrf
    {
        T tGet();
    }

    public interface Inr
    {
        void SetRes(Res res);
    }

    public interface Ips
    {
        void AddInr(Inr inr);
    }

    /// <summary>
    /// Refer to an asc that will be created later
    /// </summary>
    public class Vasc : Ivh<Asc>, Inr
    {
        int nAxNum;
        Res res;
        string stName;

        public Vasc(Ips ips, string stName, int nAxNum)
        {
            this.stName = stName;
            ips.AddInr(this);
            this.nAxNum = nAxNum;
        }

        public Asc tGet()
        {
            return res.rgascAxioms[nAxNum];
        }

        public void SetRes(Res res)
        {
            this.res = res;
        }

        public void SetV(Ibd ibd)
        {
            res.rgascAxioms[nAxNum] = (Asc)ibd;
        }

        public string stGetName()
        {
            return stName;
        }

        public Bid bidGetValue()
        {
            return tGet();
        }

        public override string ToString()
        {
            return "Vasc(\"" + stName + "\")";
        }
    }

    /// <summary>
    /// Proof outline: holds list of steps (Psb)
    /// </summary>
    public class Pou
    {
        public Psb psbFirst;
        public Psb psbLast;
        public Dictionary<string, Ivb> mpst_ivbValues = new Dictionary<string, Ivb>();
        public readonly string stClassName;

 

        public Pou(string stClassName)
        {
            this.stClassName = stClassName;
        }

        public T AddVhd<T>(T ivh) where T : Ivb
        {
            mpst_ivbValues.Add(ivh.stGetName(), ivh);
            return ivh;
        }

        public Ivh<T> ivbGet<T>(string stName) where T : class
        {
            Ivh<T> tVal = (Ivh<T>) mpst_ivbValues[stName];
            if (tVal == null)
                throw new InvalidProgramException();
            return tVal;
        }

        public Vrf vrfGet<T>(string stName) where T : class
        {
            Ivh<T> tVal = (Ivh<T>) mpst_ivbValues[stName];
            if (tVal == null)
                throw new InvalidProgramException();
            return (Vrf) tVal;
        }

        public void Add (Psb psbStep)
        {
            if (psbFirst == null)
                psbFirst = psbStep;
            /*  only psbFirst is used, for first Tst in outline
            else
                psbLast.psbNext = psbStep;
            psbLast = psbStep;
            */
        }

    }

    /// <summary>
    /// Place to hold a value that is filled in later.
    /// Using this for values created in other code for now. See also Tid
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Vhd<T> : Ivh<T> where T : Ibd
    {
        private T tVal;
        public readonly string stName;

        public Vhd(string stName)
        {
            this.stName = stName;
        }

        public T tGet()
        {
            return tVal;
        }

        public Bid bidGetValue()
        {
            if (!(tVal is Bid))
                throw new InvalidCastException();
            return tVal as Bid;
        }

        public void Set(T tVal)
        {
            this.tVal = tVal;
        }

        public void SetV(Ibd ibd)
        {
            if (ibd is Hbb)
            {
                // a hack unless Tob is split?
                Hbb hbb = (Hbb)ibd;
               tVal = (T) (Object) hbb.bidValue;
            }
            else
                this.tVal = (T)ibd;
        }

        public string stGetName()
        {
            return stName;
        }

        public override string ToString()
        {
            return "Vhd<" + typeof(T).Name + ">(\"" + stName + "\")";
        }
    }
}
