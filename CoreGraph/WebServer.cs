using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace GraphMatching
{
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, HttpListenerResponse, byte[]> _responderMethod;

      //  private WebSocket websocket = new AspNetWebSocket();

        public WebServer(Func<HttpListenerRequest, HttpListenerResponse, byte[]> method, params string[] prefixes)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                    "Needs Windows XP SP2, Server 2003 or later.");
 
            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentError("prefixes");
 
            // A responder method is required
            if (method == null)
                throw new ArgumentError("method");
 
            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);
 
            _responderMethod = method;
            _listener.Start();
        }
 
 
        public void Run(string msg)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("Webserver running at " + msg);
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                byte[] buf = _responderMethod(ctx.Request, ctx.Response);
                                if (buf != null)
                                {
                                    ctx.Response.ContentLength64 = buf.Length;
                                    ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.Out.WriteLine("webserver caught " + e.ToString());
                                Console.Out.WriteLine(e.StackTrace.ToString());
                            } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
        }
 
        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }

        private const string PATH_PREFIX = "test";

        private const string DEBUG_QUERY = "q/";
        private const string DEBUG_DIAGRAM = "d/";
        private const string SETTING = "s/";
        private const string DEBUG_LOG = "log/";
        private const string POLL = "poll/";
        private const string PATH_TO_WEB = @"..\..\..\Graph\web";

        private static readonly Char[] SPLIT_CHARS = new Char[] { '/' };

        private static byte[] RespondWithErrorMessage(string msg)
        {
            JObject obj = new JObject();
            obj.Add("message", msg);
            return Encoding.UTF8.GetBytes(obj.ToString());
        }

        private static Dictionary<string, INodePropertySetter> nodePropertySetterDict = new Dictionary<string, INodePropertySetter>();
        public static string propertySetterName = "default";
        public static void AddNodePropertySetter(string name, INodePropertySetter setter)
        {
            nodePropertySetterDict.Add(name, setter);
        }

        private static byte[] PerformRequest(string reqType, string query, HttpListenerResponse response)
        {
            try
            {
                response.ContentType = "application/json";
                string[] parts = query.Split(SPLIT_CHARS);
                int objId = Convert.ToInt32(parts[0]);
                Base obj = Base.GetById(objId);
                if (parts.Length > 1)
                {
                    string methodName = parts[1];
                    int numArgs = parts.Length - 2;
                    Base[] args = new Base[numArgs];
                    Type[] types = new Type[numArgs];
                    for (int i = 0; i < numArgs; i++)
                    {
                        int argId = Convert.ToInt32(parts[i]);
                        args[i] = Base.GetById(argId);
                        types[i] = (args[i] == null) ? null : args[i].GetType();
                    }
                    MethodInfo info = obj.GetType().GetMethod(methodName, types);
                    object value = info.Invoke(obj, args);
                    if (value == null)
                        return RespondWithErrorMessage("result is null");
                    if (value is Base)
                        obj = (Base)value;
                    return RespondWithErrorMessage("result is " + value.ToString());
                }
                INodePropertySetter nodePropertySetter = nodePropertySetterDict[propertySetterName];
                string objJson = obj.ToJObject(new JsonLog(reqType == DEBUG_DIAGRAM, new LogEntry(), nodePropertySetter)).ToString();
                return Encoding.UTF8.GetBytes(objJson);
            }
            catch (Exception e)
            {
                return RespondWithErrorMessage(e.ToString());
            }
        }

        public static byte[] SendResponse(HttpListenerRequest request, HttpListenerResponse response)
        {
            Console.WriteLine("request " + request.RawUrl);
            if (request.RawUrl.Length < PATH_PREFIX.Length + 2)
            {
                response.Redirect("/" + PATH_PREFIX + "/");
                return null;
            }
            string req = request.RawUrl.Substring(2 + PATH_PREFIX.Length);
            if (req == "")
                req = "index.html";
            if (req == "request")
                return Encoding.UTF8.GetBytes(string.Format(@"<HTML><BODY>
Request info<br>
{0}<br>
RawUrl: {1}<br>
Url: {2}<br>
ServiceName: {3}<br>
CWD: {4}
</BODY></HTML>",
                    DateTime.Now, request.RawUrl, request.Url, request.ServiceName, Directory.GetCurrentDirectory()));
            if (req.StartsWith(DEBUG_QUERY))
            {
                return PerformRequest(DEBUG_QUERY, req.Substring(DEBUG_QUERY.Length), response);
            }
            if (req.StartsWith(DEBUG_DIAGRAM))
            {
                return PerformRequest(DEBUG_DIAGRAM, req.Substring(DEBUG_DIAGRAM.Length), response);
            }
            if (req.StartsWith(POLL))
            {
                return PerformRequest(POLL, req.Substring(POLL.Length), response);
            }
            if (req.StartsWith(SETTING))
            {
                try
                {
                    response.ContentType = "application/json";
                    string query = req.Substring(SETTING.Length);
                    string[] parts = query.Split(SPLIT_CHARS);
                    if (parts.Length != 2)
                    {
                        return RespondWithErrorMessage("expected setting/value " + query);
                    }
                    string setting = parts[0];
                    string value = parts[1];
                    if ("detail" == setting)
                        TraceBase.DetailLevel = Convert.ToInt32(value);
                    else
                        return RespondWithErrorMessage(setting + " not defined");
                    return RespondWithErrorMessage(setting + " set to " + value);
                }
                catch (Exception e)
                {
                    return RespondWithErrorMessage(e.ToString());
                }
            }
            if (req.StartsWith(DEBUG_LOG))
            {
                string logType = req.Substring(DEBUG_LOG.Length);
                if (logType == "all")
                {
                    var sb = new StringBuilder();
                    sb.Append("[");
                    // there is only one top now
                    INodePropertySetter nodePropertySetter = nodePropertySetterDict[propertySetterName];
                    sb.Append(TraceNode.full.ToJObject(new JsonLog(false, new LogEntry(), nodePropertySetter)).ToString());
                    sb.Append("]");
                    response.ContentType = "application/json";
                    return Encoding.UTF8.GetBytes(sb.ToString());
                }
            }
            string filename = Directory.GetCurrentDirectory() + @"\" + PATH_TO_WEB + @"\" + req;
            int queryPos = req.IndexOf('?');
            // http://stackoverflow.com/questions/2871655/proper-mime-type-for-fonts
            if (queryPos > 0)
                req = req.Substring(0, queryPos);
            if (req.EndsWith(".js"))
                response.ContentType = "application/javascript";
            else if (req.EndsWith(".css"))
                response.ContentType = "text/css";
            else if (req.EndsWith(".woff"))
                response.ContentType = "application/font-woff";
            else if (req.EndsWith(".ttf"))
                response.ContentType = "application/x-font-ttf";
            else if (req.EndsWith(".svg"))
                response.ContentType = "image/svg+xml";
            return File.ReadAllBytes(filename);
        }

#if true
        public static void _Main(string[] args)
        {
            string url = "http://localhost:8081/" + PATH_PREFIX + "/";
            WebServer ws = new WebServer(SendResponse, url);
            ws.Run(url);
#if false
            //Test<GraphMatcherTest>();
            Program.Test<Compiler>();
            if (args != null)
            {
                while (true)
                    Thread.Sleep(1000);
                // return;
            }

            Program.Test<ConsoleApplication1.test.GraphMatcherTest>();
            Program.Test<CorrespondenceExamples>();
            Console.WriteLine("Ran {0} Tests. {1} Passed, {2} Failed.", Program._tests, Program._passed, Program._failed);
#endif

            Console.ReadLine();
            ws.Stop();
        }
#endif
        }
}