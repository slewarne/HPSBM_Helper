using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;

namespace HPALM_SBM_Helper.Objects
{
    public static class Logger
    {
        static readonly TextWriter tw;
        private static readonly object _syncObject = new object();
        private static bool m_debug = false;

        static Logger()
        {
            // are we debugging
            if (ConfigurationManager.AppSettings["debug"] == "1")
                m_debug = true;
            tw = TextWriter.Synchronized(File.AppendText(SPath() + "\\Log.txt"));
        }

        public static string SPath()
        {
            return ConfigurationManager.AppSettings["logPath"];
        }

        public static void Write(string logMessage, bool debug)
        {
            // only write out debug lines if debug is turned on, or if this is a non-debug message
            if ((m_debug && debug) || (!debug)) { 
                try
                {
                    Log(logMessage, tw);
                }
                catch (IOException e)
                {
                    tw.Close();
                }
            }
        }

        public static void Log(string logMessage, TextWriter w)
        {
            lock (_syncObject)
            {
                w.WriteLine("{0} {1}: {2}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString(), logMessage);
                w.Flush();
            }
        }
    }
}