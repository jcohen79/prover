// OWNER: GJG
// #define DEBUG_PATH

using reslab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace WBS
{
    public class Nid
    {

    }
    public class Sdv
    {
        public static string stNidFieldName;
        public object typeIn;
        public static object sdvSetType(Type type) { throw new NotImplementedException(); }
        public static Nid tFind<Nid>(Type typeFieldId, String stNidFieldName) { throw new NotImplementedException(); }
    }

    public class Trc
    {

    }

    public class Imc
    {
        public void CreateInstance(object objTypeId, Sdi sdi) { throw new NotImplementedException(); }
        public void SetPayload(object objPayload) { throw new NotImplementedException(); }
        public void SetPayload(Sdi sdi, object objPayload) { throw new NotImplementedException(); }
        public void AddValueOnAttribute(Sdi sdiParent, object objFieldId, object objChild) { throw new NotImplementedException(); }
        public void SetValueOnAttribute(Sdi sdiParent, object objFieldId, object objValue) { throw new NotImplementedException(); }
    }

    public class Sdi
    {
        public Sdv sdvDef;
        public object Payload;

        public void SetPayload(object objPayload) { throw new NotImplementedException(); }

        public void AfterBuilt() { throw new NotImplementedException(); }
        public FieldInfo fiFindField(Nid nidFieldId) { throw new NotImplementedException(); }
        public FieldInfo fiFindField(Nid nidFieldId, string stFieldName) { throw new NotImplementedException(); }

        public static void Init() { }
    }

    public interface Ivd
    {

        Imc imcGet();
    }

    public class Tpi
    {
        public int nValue;

        public Tpi(int nValue)
        {
            this.nValue = nValue;
        }
    }

    /*
     *  Define the values which we want to enumerate over
     */
    public abstract class Tqd
    {

        public readonly string stName;

        public Tpi tpiLimit;

        private static Dictionary<Type, Tqdv> mptype_SdvCache = new Dictionary<Type, Tqdv>();


        private static Dictionary<Nid, Tqdv> mpnid_SdvCache = new Dictionary<Nid, Tqdv>();


        public static Tqdv tqdvLookupSdv(Type type)
        {
            Tqdv tqdv;

            if (!mptype_SdvCache.TryGetValue(type, out tqdv))
            {
                tqdv = new Tqdv(type);
                mptype_SdvCache.Add(type, tqdv);
            }
            return tqdv;
        }

        // Not used.

        public static Tqdv tqdvLookup(Type type)
        {
            Tqdv tqdv;

            if (!mptype_SdvCache.TryGetValue(type, out tqdv))
            {
                tqdv = new Tqdv(type);
            }
            return tqdv;
        }


        public static Tqdv tqdvLookup(Nid nid)
        {
            Tqdv tqdv;

            if (!mpnid_SdvCache.TryGetValue(nid, out tqdv))
            {
                tqdv = new Tqdv(nid);
                mpnid_SdvCache.Add(nid, tqdv);
            }
            return tqdv;
        }

        protected Tqd(string stName)
        {
            this.stName = stName;
        }


        public abstract Tqs TqsMake(Tqs tqsParent);
    }

    /// <summary>
    /// Base class for helpers. They don't generate objects, but they allow others to sync with each other
    /// </summary>
    public class Tqdha
    {
        public string stName;
        public Lsm[] rgLsmSymbols;

        public Tqdha(string stName, params Lsm[] rgLsmSymbols)
        {
            this.stName = stName;
            this.rgLsmSymbols = rgLsmSymbols;
        }

        public Tqsha TqsMake(Tqs tqsParent)
        {
            return new Tqsha(this);
        }
    }

    /*
     * Defines a literal
     * Hold literal rgsdiValue
     */
    public class Tqdv : Tqd
    {

        public readonly object objValue;

        public Tqdv(string stName, object objValue) : base(stName)
        {
            this.objValue = objValue;
        }

        public Tqdv(Type tModelClass)
            : base(tModelClass.Name)
        {
            objValue = Sdv.sdvSetType(tModelClass);
        }

        public Tqdv(Nid nid)
            : base(nid.ToString())
        {
            objValue = nid;
        }

        public override Tqs TqsMake(Tqs tqsParent)
        {
            return new Tqsv(tqsParent, this);
        }
    }

    /*
     * Provide a series of alternatives to enumerate through
     */
    public class Tqda : Tqd
    {

        public Tqd[] rgtqd;

        public Tqda(string stName, params Tqd[] rgtqd) : base(stName)
        {
            this.rgtqd = rgtqd;
        }

        public void SetRg(params Tqd[] rgtqd)
        {
            this.rgtqd = rgtqd;
        }

        public override Tqs TqsMake(Tqs tqsParent)
        {
            return new Tqsa(tqsParent, this);
        }

        public void SetTpiLimitChildren(Tpi tpiChild)
        {
            foreach (Tqd tqdChild in rgtqd)
            {
                tqdChild.tpiLimit = tpiChild;
            }
        }

    }

    /*
     * Return an enumerable sequence of items
     */
    public class Tqde : Tqd
    {

        public readonly Tqd[] rgtqd;

        public Tqde(string stName, params Tqd[] rgtqd)
            : base(stName)
        {
            this.rgtqd = rgtqd;
        }

        public override Tqs TqsMake(Tqs tqsParent)
        {
            return new Tqse(tqsParent, this);
        }
    }

    /// <summary>
    /// Handle choosing a quantified variable that is currenty in scope.
    /// </summary>
    public class Tqdq : Tqd
    {
        // Return the next Tqs that defines a scope
        public delegate bool FnfScopeFn(Tqs tqsPrev);

        public readonly FnfScopeFn fnfScopeFn;


        public readonly int nChildPos;

        public Tqdq(string stName, FnfScopeFn fnfScopeFn, int nChildPos) : base(stName)
        {
            this.fnfScopeFn = fnfScopeFn;
            this.nChildPos = nChildPos;
        }

        public override Tqs TqsMake(Tqs tqsParent)
        {
            return new Tqsq(tqsParent, this);
        }
    }

    /// <summary>
    /// Construct an Lpr expression with op and other children
    /// </summary>
    public class Tqdl : Tqd
    {
        private Tqd _tqdOp;

        private readonly Tqd[] _rgtqd; // values for child objects


        public virtual Tqd[] rgtqd() { return _rgtqd; }


        public Tqdl(string stName, Tqd tqdOp, params Tqd[] rgtqd) : base(stName)
        {
            _tqdOp = tqdOp;
            if (rgtqd != null)
            {
                _rgtqd = new Tqd[rgtqd.Length];
                int i = 0;
                foreach (Tqd tqd in rgtqd)
                    _rgtqd[i++] = tqd;
            }
        }


        public virtual Tqd tqdOp() { return _tqdOp; }
        public override Tqs TqsMake(Tqs tqsParent)
        {
            if (tpiLimit != null && tqsParent is Tqsa)
            {
                if (tqsParent.nDepth >= tpiLimit.nValue)
                    return null;
            }
            return tqslMake(tqsParent);
        }

        protected virtual Tqsl tqslMake(Tqs tqsParent)
        {
            return new Tqsl(tqsParent, this);
        }

    }
    public class Tqdlr : Tqdl
    {
        private Tqdha[] rgtqdha;

        public Tqdlr(string stName, Tqd tqdOp, Tqd[] rgtqd, Tqdha[] rgtqdha)
            : base(stName, tqdOp, rgtqd)
        {
            this.rgtqdha = rgtqdha;
        }
        protected override Tqsl tqslMake(Tqs tqsParent)
        {
            List<Tqsha> rgtqsha = new List<Tqsha>();
            foreach (Tqdha tqdha in rgtqdha)
            {
                Tqsha tqsha = new Tqsha(tqdha);
                rgtqsha.Add(tqsha);
            }

            return new Tqslr(tqsParent, this, rgtqsha);
        }
    }

    public class Tqdr : Tqd
    {
        public Tqdha tqdha;

        public Tqdr(string stName, Tqdha tqdha)
            : base(stName)
        {
            this.tqdha = tqdha;
        }

        public override Tqs TqsMake(Tqs tqsParent)
        {
            return new Tqsr(tqsParent, this);
        }
    }

    /*
     * Base class for visitor for traversing Tqs tree.
     */
    public abstract class Tqv
    {
        /*
         * Call specific function on Tqs. 
         * Caller stops when Visit returns true (ie. the work is done at the current level)
         */

        public abstract KVisitResult Visit(Tqs tqs, Ivd ivdData);

        /*
         * If Visit returns true, there may be stuff to do the previously visited object.
         * return true if failed (same as fInitialize).
         */

        public virtual KInitializeResult Cleanup(Tqs tqs, Ivd ivdData) { return KInitializeResult.Succeeded; }
    }

    public class TqvLock : Tqv
    {

        public static readonly TqvLock tqvOnly = new TqvLock();

        public override KVisitResult Visit(Tqs tqs, Ivd ivdData)
        {
            tqs.DoLock();
            return KVisitResult.Continue;
        }
    }

    public class TqvUnlock : Tqv
    {

        public static readonly TqvUnlock tqvOnly = new TqvUnlock();

        public override KVisitResult Visit(Tqs tqs, Ivd ivdData)
        {
            tqs.DoUnlock();
            return KVisitResult.Continue;
        }
    }

    public class TqvMoveNext : Tqv
    {

        public static readonly TqvMoveNext tqvOnly = new TqvMoveNext();

        public override KVisitResult Visit(Tqs tqs, Ivd ivdData)
        {
            return tqs.fDoMoveNext(ivdData);
        }


        public override KInitializeResult Cleanup(Tqs tqs, Ivd ivdData)
        {
            return tqs.fInitialize(ivdData);
        }
    }

    public class TqvInitialize : Tqv
    {

        public static readonly TqvInitialize tqvOnly = new TqvInitialize();

        public override KVisitResult Visit(Tqs tqs, Ivd ivdData)
        {
            return (tqs.fDoInitialize(ivdData) == KInitializeResult.Failed) ? KVisitResult.Stop : KVisitResult.Continue;
        }
    }

    public class TqvRandomize : Tqv
    {

        public readonly Tqr tqr;

        // Not used.

        public TqvRandomize(Tqr tqr) { this.tqr = tqr; }


        public override KVisitResult Visit(Tqs tqs, Ivd ivdData)
        {
            tqs.DoRandomize(this);
            return KVisitResult.Continue;
        }
    }
    
}
