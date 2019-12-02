using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace GraphMatching
{
    public enum Side
    {
        Pattern,
        Data
    }

    public class ArgumentError : Exception
    {
        public ArgumentError(string text)
            : base(text)
        {
        }
    }

    public class TraceBase : Named
    {
        protected JObject obj = new JObject();
        protected JArray values = new JArray();

        public static int DetailLevel = 1;

        public const string LOG_VALUES = "values";

        public TraceBase(string name) : base(name) { }

        public JObject Obj()
        {
            return obj;
        }
    }

    public class TraceNode : TraceBase
    {
        public static TraceNode full = new TraceNode(null);
        private Base place;
        private List<TraceBase> contents = new List<TraceBase>();

        public TraceNode(Base place)
            : base(place == null ? "top" : place.GetType().Name)
        {
            this.place = place;
        }

        public static IEnumerable<Type> Classes()
        {
            string @namespace = "GraphMatching";

            var q = from t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && t.Namespace == @namespace
                    select t;
            // q.ToList().ForEach(t => Console.WriteLine(t.Name));
            return q;
        }

        public LogEntry Log(string name, string desc = null)
        {
            LogEntry entry = new LogEntry(name, desc);
            contents.Add(entry);
            return entry;
        }

        public TraceNode MakeChild(Base place)
        {
            TraceNode child = new TraceNode(place);
            contents.Add(child);
            return child;
        }

        public override void AddToJObject(JsonLog obj)
        {
            if (obj.IsDiagram())
            {
                CreateDiagram(obj);
            }
            else
            {
                base.AddToJObject(obj);
                obj.Add("place", place);
                JArray list = new JArray();
                foreach (TraceBase b in contents)
                {
                    list.Add(b.MakeJReference(null));
                }
                obj.Add("content", list);
            }
        }
        public override void AddToJReference(JObject vobj)
        {
            vobj.Add(LogEntry.LOG_REF_TEXT, place.ToString());
        }

    }

    public class LogEntry : TraceBase
    {

        public const string LOG_NAME = "name";
        public const string LOG_TIME = "time";
        public const string LOG_VALUE = "value";
        public const string LOG_ARRAY = "array";
        public const string LOG_ID = "id";
        public const string LOG_CLASS = "class";
        public const string LOG_DESC = "desc";
        public const string LOG_REF_TEXT = "reftext";

        private string desc;

        /// <summary>
        /// This ctor used to select diagram vs table
        /// </summary>
        public LogEntry()
            : base("dummy")
        {
            obj.Add(LOG_VALUES, values);
        }

        public LogEntry(string name, string desc)
            : base(name)
        {
            obj.Add(LOG_NAME, name);
            obj.Add(LOG_TIME, Base.CurrentCounter());
            obj.Add(LOG_VALUES, values);
            this.desc = desc;
        }
        public override JObject ToJObject(JsonLog formatter)
        {
            // a LogEntry is formatted already when it is created
            return obj;
        }

        public LogEntry A(string name, int value)
        {
            var vobj = new JObject();
            vobj.Add(LOG_NAME, name);
            vobj.Add(LOG_VALUE, value);
            values.Add(vobj);
            return this;
        }

        public LogEntry A(string name, bool value)
        {
            var vobj = new JObject();
            vobj.Add(LOG_NAME, name);
            vobj.Add(LOG_VALUE, value);
            values.Add(vobj);
            return this;
        }

        public LogEntry A(string name, string value)
        {
            if (value != null)
            {
                var vobj = new JObject();
                vobj.Add(LOG_NAME, name);
                vobj.Add(LOG_VALUE, value);
                values.Add(vobj);
            }
            return this;
        }

        public LogEntry A(string name, JArray value)
        {
            if (value != null)
            {
                var vobj = new JObject();
                vobj.Add(LOG_NAME, name);
                vobj.Add(LOG_ARRAY, value);
                values.Add(vobj);
            }
            return this;
        }

        public LogEntry A(string name, JObject value)
        {
            if (value != null)
            {
                var vobj = new JObject();
                vobj.Add(LOG_NAME, name);
                vobj.Add(LOG_VALUE, value);
                values.Add(vobj);
            }
            return this;
        }

        public LogEntry A(string name, Base value)
        {
            if (value != null)
            {
                values.Add(value.MakeJReference(name));
            }
            return this;
        }
        public override void AddToJReference(JObject vobj)
        {
            if (desc != null)
                vobj.Add(LogEntry.LOG_REF_TEXT, desc);
        }
    }

    public class JsonLog
    {
        private LogEntry obj;
        private bool diagram;
        private INodePropertySetter nodePropertySetter;

        public JsonLog(bool diagram, LogEntry obj, INodePropertySetter nodePropertySetter)
        {
            this.diagram = diagram;
            this.obj = obj;
            this.nodePropertySetter = nodePropertySetter;
        }

        /// <summary>
        /// Distingish between a client request for diagram instead of table.
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsDiagram()
        {
            return diagram;
        }

        public JObject GetJObject()
        {
            return obj.Obj();
        }

        public void Add(string s, string o)
        {
            if (obj == null)
                return;
            obj.A(s, o);
        }

        public void Add(string s, int o)
        {
            if (obj == null)
                return;
            obj.A(s, o);
        }

        public void Add(string s, JArray o)
        {
            if (obj == null)
                return;
            obj.A(s, o);
        }

        public void Add(string s, JObject o)
        {
            if (obj == null)
                return;
            obj.A(s, o);
        }

        public void Add(string s, Base o)
        {
            if (obj == null)
                return;
            obj.A(s, o);
        }

        public bool SetNodeProperties(Vertex v, DirectionType direction, EdgeDescriptor edgeDesc, JObject node)
        {
            return nodePropertySetter.SetNodeProperties(v, direction, edgeDesc, node);
        }

    }

    public interface INodePropertySetter
    {
        /// <summary>
        /// Add property to the node to control how to display a vertex, based an edge with the
        /// given descriptor and direction relative to the vertex.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="direction"></param>
        /// <param name="edgeDesc"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        bool SetNodeProperties(Vertex v, DirectionType direction, EdgeDescriptor edgeDesc, JObject node);
    }

    /// <summary>
    /// Store references to snapshots of objects.
    /// </summary>
    public class SavedObject
    {
        private readonly Base _base;
        private readonly JObject _saved;
        private readonly int _prevSnapshot;

        public Base Base { get { return _base; } }
        public JObject Saved { get { return _saved; } }

        public SavedObject(Base baseObj, int prevSnapshot, JObject saved)
        {
            _base = baseObj;
            _prevSnapshot = prevSnapshot;
            _saved = saved;
        }
    }
    /* 

 */
    // </tkey,tvalue></tkey,tvalue>

    /// <summary>
    /// Base for all classes, has debug/log support.
    /// </summary>
    public abstract class Base
    {
        private readonly int _id;
        private int _latestSnapshotId;
        private Base _nextDirty;
        private static Base _lastDirty;

        public int Id { get { return _id; } }
        private static int idCounter = 0;
        public static int stopOnId = -572;

        private readonly static Dictionary<int, Base> liveTable = new Dictionary<int, Base>();
        private readonly static Dictionary<int, SavedObject> savedTable = new Dictionary<int, SavedObject>();
        /*
        public static TValue ValueOrDefault<TValue, TKey>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value = default(TValue);
            dictionary.TryGetValue(key, out value);
            return value;
        }
         * */
        public Base()
        {
            _id = idCounter++;
            _latestSnapshotId = _id;
            liveTable.Add(_id, this);
            if (stopOnId == _id)
                Console.WriteLine("trap " + _id);

        }

        /// <summary>
        /// Indicate that next time a log snapshot is taken that this object needs a new snapshot because
        /// it has changed. Call this function from anyplace, such as a setter, that modifies the state of an object
        /// that would need to be debugged.
        /// </summary>
        protected void MarkDirty()
        {
            if (_nextDirty != null)
                return;
            if (_lastDirty == null)
                _nextDirty = this;  // watch out for the loop at the end
            else
                _nextDirty = _lastDirty;
            _lastDirty = this;
        }

        public static int CurrentCounter()
        {
            return idCounter;
        }

        /*  problem: snapshot the basic or diagram form, or both?
        public void MakeSnapshot()
        {
            var _prevSnapshot = _latestSnapshotId;
            _latestSnapshotId = idCounter++;
            var saved = new SavedObject(this, _prevSnapshot, ToJObject());
            savedTable.Add(_latestSnapshotId, saved);
        }

        public static void AddSnapshots()
        {
            while (_lastDirty != null)
            {
                _lastDirty.MakeSnapshot();
                var prev = _lastDirty;
                _lastDirty = _lastDirty._nextDirty;
                prev._nextDirty = null;
                if (_lastDirty == prev) // end is marked by loop
                {
                    break;
                }
            }
        }
        */

        public int GetSnapshotId()
        {
            return _latestSnapshotId;
        }

        /// <summary>
        /// Set the text that appears in a table where the value is a reference to this. 
        /// Should be 1 line or so of text.
        /// </summary>
        /// <param name="vobj"></param>
        public virtual void AddToJReference(JObject vobj)
        {
            vobj.Add(LogEntry.LOG_REF_TEXT, ToString());
        }

        public JObject MakeJReference(string name)
        {
            var vobj = new JObject();
            if (name != null)
                vobj.Add(LogEntry.LOG_NAME, name);
            vobj.Add(LogEntry.LOG_ID, GetSnapshotId());
            vobj.Add(LogEntry.LOG_CLASS, GetType().Name);
            string desc = GetDesc();
            if (desc != null)
                vobj.Add(LogEntry.LOG_DESC, desc);
            AddToJReference(vobj);
            return vobj;
        }

        public static Base GetById(int id)
        {
            return liveTable[id];
        }

        public virtual string GetDesc()
        {
            return Id.ToString() + ": ";
        }

        public virtual void AddToJObject(JsonLog obj)
        {
            if (obj.IsDiagram())
            {
                CreateDiagram(obj);
            }
        }

        public void CreateDiagram(JsonLog obj)
        {
            var nodes = new JArray();
            var edges = new JArray();
            var nodeTable = new List<Vertex>();
            ExtendDiagram(nodes, edges, nodeTable, obj);
            obj.Add("nodes", nodes);
            obj.Add("edges", edges);
        }

        public static bool IsListType(Type type)
        {
            if (type == typeof(String))
                return false;
            Type ienum = type.GetInterface("IEnumerable`1");
            if (ienum == null)
                return false;
            return true;
        }
        public void AddValueToDiagram(JArray nodes, JArray edges, List<Vertex> nodeTable, int source, string name, Type type, object value)
        {
            if (!(value is Base))
                return;
            Base b = (Base)value;
            var nobj = new JObject();
            if (b is Named)
                nobj.Add("name", ((Named)b).Name);
            nobj.Add("nameTip", b.GetDesc());
            nobj.Add("id", b.Id);

            nodes.Add(nobj);
            nodeTable.Add(null);

            var lobj = new JObject();
            lobj.Add("id", Id.ToString() + "." + b.Id.ToString());
            lobj.Add("sourceId", Id);
            lobj.Add("targetId", b.Id);
            lobj.Add("color", EdgeDescriptor.ValueEdgeColor.RGB);
            lobj.Add("nameTip", name);
            edges.Add(lobj);

        }

        public virtual void ExtendDiagram(JArray nodes, JArray edges, List<Vertex> nodeTable, JsonLog obj)
        {
            int source = Id;
            var nobj = new JObject();
            if (this is Named)
                nobj.Add("name", ((Named)this).Name);
            nobj.Add("nameTip", GetDesc());
            nobj.Add("id", Id);

            nodes.Add(nobj);
            nodeTable.Add(null);

            foreach (var member in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                string name = member.Name;
                var value = member.GetValue(this);
                AddValueToDiagram(nodes, edges, nodeTable, source, member.Name, member.FieldType, value);
            }
            foreach (var member in GetType().GetProperties())
            {
                string name = member.Name;
                if (name.Equals("Id"))
                    continue;
                var value = member.GetValue(this);
                AddValueToDiagram(nodes, edges, nodeTable, source, member.Name, member.PropertyType, value);
            }
        }

        public virtual JObject ToJObject(JsonLog formatter)
        {
            AddToJObject(formatter);
            return formatter.GetJObject();
        }

        public void AddToJObject(JsonLog obj, string name, Base value)
        {
            if (value == null)
                return;
            obj.Add(name, value);
        }

        public void AddToJObject(JsonLog obj, string name, string value)
        {
            obj.Add(name, value);
        }

        public void AddToJObject(JsonLog obj, string name, int value)
        {
            obj.Add(name, value);
        }

        public void AddToJObject(JsonLog obj, string name, object value)
        {
            if (value == null)
                return;
            obj.Add(name, value.ToString());
        }
        public void AddToJObject(JsonLog obj, string name, Dictionary<string, object> value)
        {
            if (value == null)
                return;
            JObject data = new JObject();
            foreach (var c in value)
            {
                if (c.Value is Base)
                    data.Add(c.Key, (c.Value as Base).GetSnapshotId());
                else
                    data.Add(c.Key, c.Value.ToString());
            }
            obj.Add(name, data);
        }

        public void AddListToJObject<T>(JsonLog obj, string name, IEnumerable<T> value) where T : Base
        {
            if (value == null)
                return;
            JArray list = new JArray();
            foreach (T c in value)
            {
                list.Add(c.MakeJReference(null));
            }
            obj.Add(name, list);
        }
        public virtual void Format(PrettyFormatter sb)
        {
        }
        public virtual void FormatSubobject(PrettyFormatter sb)
        {
            sb.Push();
            sb.Left("{");
            sb.Keyword(GetType().Name);
            Format(sb);
            sb.Right("}");
            sb.Pop();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var pf = new PrettyFormatter(new Fwt(sb));
            FormatSubobject(pf);
            pf.Newline();
            return sb.ToString();
        }
    }
}
