using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Diagnostics;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

//
// Helper class to reformat HP ALM JSON to an SBM friendly format.
//
//
// HP returns entities with name/value pairs for fields, i.e. 
//
//   "Fields": [{"Name": "id",
//			"values": [{
//				"value": "2"}]},
//		{"Name": "detected-by",
//			"values": [{
//				"value": "alice_alm"}]},
//		{"Name": "status",
//		    "values": [{
//				"value": "Reopen"}]	},....
//
// This helper function reformats it to a more SBM friendly format

namespace HPALM_SBM_Helper.Objects
{
    public class JSONFormatter
    {
        public string formatJSON(string input)
        {
            //object jsonOBJ = null;
            //object entity = null;
            //object field = null;
            Dictionary<string, string> fldDict = default(Dictionary<string, string>);
            List<Dictionary<string, string>> recordList = new List<Dictionary<string, string>>();

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            dynamic jsonOBJ = serializer.DeserializeObject(input);

            // this is the array of entities 
            // object item in (Array)stories
            foreach (dynamic entity in (Array)jsonOBJ["entities"])
            {
                fldDict = new Dictionary<string, string>();
                // this is the list of fields in the entity
                foreach (dynamic field in (Array)entity["Fields"])
                    fldDict.Add(field["Name"], field["values"][0]["value"]);
                //add the dictionary to the list
                recordList.Add(fldDict);
            }

            //serialize the dictionary
            string json = JsonConvert.SerializeObject(recordList, new KeyValuePairConverter());

            fldDict = null;
            recordList = null;

            return json;
        }
    }
}
