using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Net;
using System.Text;
using System.Configuration;


namespace HPALM_SBM_Helper.Objects
{
    public class HPALM_Helper 
    {
        Uri m_uri;
        string url;
        CookieAwareWebClient client;

        public HPALM_Helper()  {
            client = new CookieAwareWebClient();
            client.Encoding = Encoding.UTF8;
            url = ConfigurationManager.AppSettings["HP_URL"];
        }

        public string getAuth()  {
            return client.authCookie.ToString();
        }
        public string getSession()  {
            return client.sessionCookie.ToString();
        }

        public string getAuthToken() {
            return postToALM("qcbin/authentication-point/alm-authenticate", "<alm-authentication><user>" + ConfigurationManager.AppSettings["HP_UID"] + "</user><password>" + ConfigurationManager.AppSettings["HP_PWD"] + "</password></alm-authentication>", true, false);
        }

        public string getSessionToken()  {
            return postToALM("qcbin/rest/site-session", "", true, false);
        }

        public string getFromALM(string query)  {

            //run the query
            string response = null;
            m_uri = new Uri(url + query);

            client.Headers.Add("ACCEPT", "application/json");
            try {
                response = client.DownloadString(m_uri.ToString());
                return response;
            }
            catch (WebException ex)   {
                Logger.Write("Error: HPALM_Helper.cs getFromALM returned an exception. " + ex.Message, false);
                return "Error: " + ex.Message;
            }
        }

        public string deleteFromALM(string urlAction)   {

            m_uri = new Uri(url + urlAction);
            client.Headers.Add("ACCEPT", "application/xml");
            client.Headers.Add("Content-Type", "application/xml");

            try  {
                return client.UploadString(m_uri.ToString(), "DELETE", "");
            }
            catch (WebException ex)  {
                Logger.Write("Error: HPALM_Helper.cs deleteFromALM returned an exception. " + ex.Message, false);
                return "Error: " + ex.Message;
            }
        }


        public string postToALM(string urlAction, string payload, bool Auth, bool doPut)  {

            m_uri = new Uri(url + urlAction);

            client.Headers.Add("ACCEPT", "application/json");

            if (!Auth)  {
                client.Headers.Add("ACCEPT", "application/xml");
                client.Headers.Add("Content-Type", "application/xml");
            }

            try  {
                if (doPut)  
                    return client.UploadString(m_uri.ToString(), "PUT", payload);
                else
                    return client.UploadString(m_uri.ToString(), payload);
            }
            catch (WebException ex)  {
                Logger.Write("Error: HPALM_Helper.cs postToALM returned an exception. " + ex.Message, false);
                return "Error: " + ex.Message;
            }
        }
    }
}