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
            try
            {
                Logger.Write("HP_ALM_GET command received.  Domain=" + domain + ", Project=" + project + ", entityType=" + entityType +
                                ", Fields=" + fields + ", Query=" + query + ", Order-By=" + orderBy, true);

                string urlAction = "qcbin/rest/domains/" + domain + "/projects/" + project + "/" + entityType;
                string queryStr = "";

                if (!string.IsNullOrEmpty(fields))
                    queryStr = queryStr + "fields=" + fields;

                if (!string.IsNullOrEmpty(query))
                {
                    if (string.IsNullOrEmpty(queryStr))
                        queryStr = queryStr + "query=" + query;
                    else
                        queryStr = queryStr + "&query=" + query;
                }

                if (!string.IsNullOrEmpty(orderBy))
                {
                    if (string.IsNullOrEmpty(queryStr))
                        queryStr = queryStr + "order-by=" + orderBy;

                    else
                        queryStr = queryStr + "&order-by=" + orderBy;
                }

                if (!string.IsNullOrEmpty(queryStr))
                    urlAction = urlAction + "?" + queryStr;

                Logger.Write("HP_ALM_GET: urlAction=" + urlAction, true);

                HPALM_Helper HPHelper = new HPALM_Helper();
                HPHelper.getAuthToken();
                HPHelper.getSessionToken();
                HPALM_GET_Response output = new HPALM_GET_Response();
                HPALM_Entities tmpOut = new HPALM_Entities();

                string responseJSON = HPHelper.getFromALM(urlAction);
                Logger.Write("HP_ALM_GET: responseJSON=" + responseJSON, true);
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                tmpOut = serializer.Deserialize<HPALM_Entities>(responseJSON);
                output.HPALMEntities = tmpOut;
                output.AuthToken = HPHelper.getAuth();
                output.sessionToken = HPHelper.getSession();
                return output;
            }
            catch (Exception e)
            {
                Logger.Write("HP_ALM_GET: exception occured. " + e.Message  + e.StackTrace, false);
                return new HPALM_GET_Response();
            }
           
        }

        [WebMethod()]
        public HPALM_Post_Response HP_ALM_Update(string Domain, string Project, string EntityType, string EntityID, 
                                                 string xmlBody, bool UpdateWithLock,bool UpdateWithVersioning,
                                                 CheckOutParameters chkOutParams, CheckInParameters chkInParams)
        {
            try
            {
                Logger.Write("HP_ALM_Update command received.  Domain=" + Domain + ", Project=" + Project + ", EntityType=" + EntityType +
                            ", EntityID=" + EntityID + ", xmlBody=" + xmlBody + ", UpdateWithLock=" + UpdateWithLock + 
                            ", UpdateWithVersioning=" + UpdateWithVersioning + ", CheckOutParams Comment=" + chkOutParams.Comment +
                            ", CheckOutParams Version=" + chkOutParams.Version + ", CheckInParams Comment=" + chkInParams.Comment + 
                            ", CheckInParams OverrideLastVersion=" + chkInParams.OverrideLastVersion, true);

                HPALM_Helper HPHelper = new HPALM_Helper();
                HP_Entity output = new HP_Entity();
                string urlAction = "qcbin/rest/domains/" + Domain + "/projects/" + Project + "/" + EntityType + "/" + EntityID;
                string responseJSON = "";

                Logger.Write("HP_ALM_Update: urlAction=" + urlAction, true);

                HPHelper.getAuthToken();
                HPHelper.getSessionToken();

                if (UpdateWithVersioning)
                {
                    Logger.Write("HP_ALM_Update: Updating with Versioning - checking out.", true);
                    //check-out
                    HPHelper.postToALM(urlAction + "/versions/check-out",
                                        "<CheckOutParameters><Comment>" + chkOutParams.Comment + "</Comment><Version>" + chkOutParams.Version + "</Version></CheckOutParameters>",
                                        false, false);
                    //do the update
                    Logger.Write("HP_ALM_Update: Updating with Versioning - updating the record.", true);
                    responseJSON = HPHelper.postToALM(urlAction, Server.UrlDecode(xmlBody), false, true);
                    Logger.Write("HP_ALM_Update: Updating with Versioning - responseJSON=" + responseJSON, true);
                    //check in
                    Logger.Write("HP_ALM_Update: Updating with Versioning - checking in.", true);
                    HPHelper.postToALM(urlAction + "/versions/check-in",
                                       "<CheckInParameters><Comment>" + chkInParams.Comment + "</Comment><OverrideLastVersion>" + chkInParams.OverrideLastVersion + "</OverrideLastVersion></CheckInParameters>",
                                       false, false);
                }
                else
                {
                    if (UpdateWithLock)   //are we updating with lock?
                    {
                        Logger.Write("HP_ALM_Update: Updating with Lock - locking.", true);                          
                        HPHelper.getFromALM(urlAction + "/lock");                                                   //get the lock
                        Logger.Write("HP_ALM_Update: Updating with Lock - updating the record.", true);
                        responseJSON = HPHelper.postToALM(urlAction, Server.UrlDecode(xmlBody), false, true);       //do the update
                        Logger.Write("HP_ALM_Update: Updating with Lock - responseJSON=" + responseJSON, true);
                        Logger.Write("HP_ALM_Update: Updating with Lock - unlocking.", true); 
                        HPHelper.deleteFromALM(urlAction + "/lock");                                                //unlock
                    }
                    else
                    {
                        Logger.Write("HP_ALM_Update: Updating with no Lock.", true);
                        responseJSON = HPHelper.postToALM(urlAction, Server.UrlDecode(xmlBody), false, true);       //do the update without lock
                        Logger.Write("HP_ALM_Update: Updating with no Lock - responseJSON=" + responseJSON, true);
                    }
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                output = serializer.Deserialize<HP_Entity>(responseJSON);
                return new HPALM_Post_Response(output, HPHelper.getAuth(), HPHelper.getSession());

            }
            catch (Exception e)
            {
                Logger.Write("HP_ALM_Update: exception occured. " +  e.Message + e.StackTrace, false);
                return new HPALM_Post_Response();
            }
        }

        [WebMethod()]
        public HPALM_Post_Response HP_ALM_Create(string domain, string project, string entityType, string xmlBody) 
        {
            try
            {
                Logger.Write("HP_ALM_Create command received.  Domain=" + domain + ", Project=" + project + ", EntityType=" + entityType +
                            ", xmlBody=" + xmlBody, true);

                HPALM_Helper HPHelper = new HPALM_Helper();
                HPHelper.getAuthToken();
                HPHelper.getSessionToken();
                HP_Entity output = new HP_Entity();
                string urlAction = "qcbin/rest/domains/" + domain + "/projects/" + project + "/" + entityType;
                Logger.Write("HP_ALM_Create: urlAction=" + urlAction, true);

                string responseJSON = HPHelper.postToALM(urlAction, Server.UrlDecode(xmlBody), false, false);
                Logger.Write("HP_ALM_Create: responseJSON=" + responseJSON, true);
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                output = serializer.Deserialize<HP_Entity>(responseJSON);
                return new HPALM_Post_Response(output, HPHelper.getAuth(), HPHelper.getSession());
            }
            catch (Exception e)
            {
                Logger.Write("HP_ALM_Create: exception occured. " + e.Message + e.StackTrace, false);
                return new HPALM_Post_Response();
            }
            
        }

        [WebMethod()]
        public string GetAuthTokensString()  {
            try
            {
                Logger.Write("GetAuthTokensString command received.", true);
                HPALM_Helper HPHelper = new HPALM_Helper();
                HPHelper.getAuthToken();
                HPHelper.getSessionToken();
                Logger.Write("GetAuthTokensString: output=" + HPHelper.getAuth() + ";" + HPHelper.getSession(), true);
                return HPHelper.getAuth() + ";" + HPHelper.getSession();
            }
            catch (Exception e)
            {
                Logger.Write("GetAuthTokensString: exception occured. " + e.Message + e.StackTrace, true);
                return "Error: " + e.Message;
            }
           
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
