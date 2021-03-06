﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using HPALM_SBM_Helper.Objects;

namespace HPALM_SBM_Helper
{
    /// <summary>
    /// HPALM REST Helper.  Provides an SBM friendly way to make REST calls to HP ALM
    /// </summary>
    public class HPALM_REST : IHttpHandler
    {
        string HPQuery = null;
        HPALM_Helper HPHelper = new HPALM_Helper();
        JSONFormatter jsonHelper = new JSONFormatter();

        public void ProcessRequest(HttpContext context)  {

            try
            {
                HPHelper = new HPALM_Helper();
                jsonHelper = new JSONFormatter();

                //get the query
                HPQuery = context.Request.Params["hpurl"];
                Logger.Write("HPALM_REST: hpurl:" + HPQuery, true);

                bool hasParams = false;
                string jsonResponse = "";

                //build the HP Query string based on input parameters (fields, query, order-by are all optional)
                if ((context.Request.Params["fields"] != null))
                {
                    HPQuery = HPQuery + "?fields=" + context.Request.Params["fields"];
                    Logger.Write("HPALM_REST: 'fields' parameter exists:" + HPQuery, true);
                    hasParams = true;
                }
                if ((context.Request.Params["query"] != null))
                {
                    if (hasParams)
                    {
                        HPQuery = HPQuery + "&query=" + context.Request.Params["query"];
                        Logger.Write("HPALM_REST: 'query' parameter exists:" + HPQuery, true);
                    }
                    else
                    {
                        HPQuery = HPQuery + "?query=" + context.Request.Params["query"];
                        hasParams = true;
                    }
                }
                if ((context.Request.Params["order-by"] != null))
                {
                    if (hasParams)
                    {
                        HPQuery = HPQuery + "&order-by=" + context.Request.Params["order-by"];
                        Logger.Write("HPALM_REST: 'order-by' parameter exists:" + HPQuery, true);
                    }
                    else
                    {
                        HPQuery = HPQuery + "?=order-by" + context.Request.Params["order-by"];
                    }
                }

                Logger.Write("HPALM_REST: Completed URL: " + HPQuery, true);

                //get the Auth and Session tokens
                HPHelper.getAuthToken();
                HPHelper.getSessionToken();
                jsonResponse = HPHelper.getFromALM(HPQuery);
                Logger.Write("HPALM_REST: jsonResponse: " + jsonResponse, true);

                context.Response.ContentType = "application/json";
                context.Response.Write(jsonHelper.formatJSON(jsonResponse));
            }
            catch (Exception e)
            {
                Logger.Write("HPALM_REST: Error - " + e.Message + e.StackTrace, false);
                context.Response.Write("Error: " + e.Message);
            }
            
        }

        public bool IsReusable   {
            get {
                return false;
            }
        }
    }
}




		

