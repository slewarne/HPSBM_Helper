using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Diagnostics;
using System.Net;

namespace HPALM_SBM_Helper.Objects
{
    public class CookieAwareWebClient : WebClient
    {
        public CookieContainer cc;
        public Cookie authCookie;
        public Cookie sessionCookie;
        private string lastPage;

        public CookieAwareWebClient() {
            cc = new CookieContainer();
        }

        protected override System.Net.WebRequest GetWebRequest(System.Uri address)  {
            dynamic R = base.GetWebRequest(address);
            if (R is HttpWebRequest)  {
                var _with1 = (HttpWebRequest)R;
                _with1.CookieContainer = cc;
                if ((lastPage != null))
                    _with1.Referer = lastPage;
            }
            lastPage = address.ToString();
            return R;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)  {
            try
            {
                WebResponse response = base.GetWebResponse(request, result);
                ReadCookies(response);
                return response;
            }
            catch (Exception e)
            {
                Logger.Write("Error: " + e.Message + e.StackTrace, false);
                return base.GetWebResponse(request, result);
            }
            
        }

        protected override WebResponse GetWebResponse(WebRequest request)  {
            try
            {
                WebResponse response = default(WebResponse);
                response = base.GetWebResponse(request);
                ReadCookies(response);
                return response;
            }
            catch (Exception e)
            {
                Logger.Write("Error: " + e.Message + e.StackTrace, false);
                return default(WebResponse);
            }
        }

        private void ReadCookies(WebResponse r) {
		    dynamic response = r as HttpWebResponse;
		    if (response != null) {
			    foreach (Cookie m_cookie in response.Cookies) {
                   
                    if (m_cookie.Name == "LWSSO_COOKIE_KEY")  
					    authCookie = m_cookie;
				    if (m_cookie.Name == "QCSession") 
					    sessionCookie = m_cookie;
			    }
			    cc.Add(response.Cookies);
		    }
	    }
    }
}