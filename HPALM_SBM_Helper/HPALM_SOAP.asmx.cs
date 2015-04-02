using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using HPALM_SBM_Helper.Objects;
using System.Web.Script.Serialization;

namespace HPALM_SBM_Helper
{
    /// <summary>
    /// Summary description for HPALM_SOAP
    /// </summary>
    [WebService(Namespace = "http://serena.com/HPALM_Helper")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class HPALM_SOAP : System.Web.Services.WebService
    {

        [WebMethod()]
        public HPALM_GET_Response HP_ALM_Get(string domain, string project, string entityType, string fields, string query, string orderBy)
        {
            //qcbin/rest/domains/DEFAULT/projects/Sample/defects?fields=id,name,status,severity,detected-by&query={id[>1 AND <10]}
            string urlAction = "qcbin/rest/domains/" + domain + "/projects/" + project + "/" + entityType;
            string queryStr = "";

            if (!string.IsNullOrEmpty(fields))  
                queryStr = queryStr + "fields=" + fields;

            if (!string.IsNullOrEmpty(query))  {
                if (string.IsNullOrEmpty(queryStr))  
                    queryStr = queryStr + "query=" + query;
                else
                    queryStr = queryStr + "&query=" + query;
            }

            if (!string.IsNullOrEmpty(orderBy))  {
                if (string.IsNullOrEmpty(queryStr))
                    queryStr = queryStr + "order-by=" + orderBy;

                else
                    queryStr = queryStr + "&order-by=" + orderBy;
            }

            if (!string.IsNullOrEmpty(queryStr))
                urlAction = urlAction + "?" + queryStr;

            HPALM_Helper HPHelper = new HPALM_Helper();
            HPHelper.getAuthToken();
            HPHelper.getSessionToken();
            HPALM_GET_Response output = new HPALM_GET_Response();
            HPALM_Entities tmpOut = new HPALM_Entities();

            string responseJSON = HPHelper.getFromALM(urlAction);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            tmpOut = serializer.Deserialize<HPALM_Entities>(responseJSON);
            output.HPALMEntities = tmpOut;
            output.AuthToken = HPHelper.getAuth();
            output.sessionToken = HPHelper.getSession();
            return output;
        }

        [WebMethod()]
        public HPALM_Post_Response HP_ALM_Update(string Domain, string Project, string EntityType, string EntityID, 
                                                 string xmlBody, bool UpdateWithLock,bool UpdateWithVersioning,
                                                 CheckOutParameters chkOutParams, CheckInParameters chkInParams)
        {
            HPALM_Helper HPHelper = new HPALM_Helper();
            HP_Entity output = new HP_Entity();
            string urlAction = "qcbin/rest/domains/" + Domain + "/projects/" + Project + "/" + EntityType + "/" + EntityID;
            string responseJSON = "";

            HPHelper.getAuthToken();
            HPHelper.getSessionToken();

            if (UpdateWithVersioning)
            {
                //check-out
                HPHelper.postToALM(urlAction + "/versions/check-out",
                                    "<CheckOutParameters><Comment>" + chkOutParams.Comment + "</Comment><Version>" + chkOutParams.Version + "</Version></CheckOutParameters>",
                                    false, false);
                //do the update
                responseJSON = HPHelper.postToALM(urlAction, Server.UrlDecode(xmlBody), false, true);       
                //check in
                 HPHelper.postToALM(urlAction + "/versions/check-in",
                                    "<CheckInParameters><Comment>" + chkInParams.Comment + "</Comment><OverrideLastVersion>" + chkInParams.OverrideLastVersion + "</OverrideLastVersion></CheckInParameters>",
                                    false, false);    
            }
            else
            {
                if (UpdateWithLock)
                {                                                                                               //are we updating with lock?
                    HPHelper.getFromALM(urlAction + "/lock");                                                   //get the lock
                    responseJSON = HPHelper.postToALM(urlAction, Server.UrlDecode(xmlBody), false, true);       //do the update
                    HPHelper.deleteFromALM(urlAction + "/lock");                                                //unlock
                }
                else
                    responseJSON = HPHelper.postToALM(urlAction, Server.UrlDecode(xmlBody), false, true);       //do the update without lock
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            output = serializer.Deserialize < HP_Entity>(responseJSON);
            return new HPALM_Post_Response(output, HPHelper.getAuth(), HPHelper.getSession());
        }

        [WebMethod()]
        public HPALM_Post_Response HP_ALM_Create(string domain, string project, string entityType, string xmlBody) {

            HPALM_Helper HPHelper = new HPALM_Helper();
            HPHelper.getAuthToken();
            HPHelper.getSessionToken();
            HP_Entity output = new HP_Entity();
            string urlAction = "qcbin/rest/domains/" + domain + "/projects/" + project + "/" + entityType;

            string responseJSON = HPHelper.postToALM(urlAction, Server.UrlDecode(xmlBody), false, false);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            output = serializer.Deserialize < HP_Entity>(responseJSON);
            return new HPALM_Post_Response(output, HPHelper.getAuth(), HPHelper.getSession());
        }

        [WebMethod()]
        public string GetAuthTokensString()  {
            HPALM_Helper HPHelper = new HPALM_Helper();
            HPHelper.getAuthToken();
            HPHelper.getSessionToken();
            return HPHelper.getAuth() + ";" + HPHelper.getSession();
        }

        // check out parameters when using versioning
        public class CheckOutParameters
        {
            public string Comment { get; set; }
            public string Version { get; set; }
        }
        //check in parameters when using versioning
        public class CheckInParameters
        {
            public string Comment { get; set; }
            public bool OverrideLastVersion { get; set; }
        }

        public class Value {
            public string value { get; set; }
        }

        public class Field {
            public string Name { get; set; }
            public Value[] values { get; set; }
        }

        public class HP_Entity {
            public Field[] Fields { get; set; }
            public string Type { get; set; }
        }

        public class HPALM_Entities {
            public HP_Entity[] entities { get; set; }
            public int TotalResults { get; set; }
        }

        public class HPALM_GET_Response {
            public HPALM_Entities HPALMEntities { get; set; }
            public string AuthToken { get; set; }
            public string sessionToken { get; set; }
        }

        public class HPALM_Post_Response {
            public HPALM_Post_Response() {
            }

            public HPALM_Post_Response(HP_Entity ent, string aToken, string sToken) {
                HPALMEntity = ent;
                AuthToken = aToken;
                sessionToken = sToken;
            }

            public HP_Entity HPALMEntity { get; set; }
            public string AuthToken { get; set; }
            public string sessionToken { get; set; }
        }
    }
}
