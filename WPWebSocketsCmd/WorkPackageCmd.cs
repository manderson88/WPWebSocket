using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;
using Newtonsoft.Json;
using BIM = Bentley.Interop.MicroStationDGN;
using System.IO;
using System.Net;
using System.Web;
using System.Net.Security;
using System.Text;
using System.Threading;

namespace WPWebSocketsCmd
{
    /// <summary>
    /// a class that will communicate to microstation.  currently hardwired for my desk top.
    /// This requires the ecapiexample app to support many of the actions.
    /// </summary>
    class WorkPackageCmd
    {
        private static BIM.Application m_App;
        private static Process m_hostApp { get; set; }
        //private static string m_userName { get; set; }
        //private static string m_pwd{get;set;}
        private static string m_authCode { get; set; }
        private static Queue<string> m_messageQueue = new Queue<string>();
        public static WPWebSocketsCmd.Server.WPSWebSocketService MessageClient { get; set; }
        //private static string s_activeFileName;
        /// <summary>
        /// gets the running instance of the host application
        /// </summary>
        /// <returns></returns>
        private static BIM.Application GetMSApp()
        {
            if (null == m_App)
            {
                Object o = System.Activator.CreateInstance(Type.GetTypeFromProgID("MicroStationDGN.Application.1"));

                BIM.Application _ustn = (BIM.Application)o;
                _ustn.Visible = true;
                m_App = _ustn;
                m_App.CadInputQueue.SendKeyin("mdl silentload WorkPackageAddin");
                return m_App;
            }
            else
            {
                try
                {
                    m_App.CadInputQueue.SendKeyin("mdl silentload WorkPackageAddin");
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message);
                    m_App = null;
                    GetMSApp();
                }
                return m_App;
            }
        }
        private static string GenerateAuthCode(string user,string pwd)
        {
            string authorization;// = "dwg_conv" + "xscdvf!";
            authorization = user + ":" + pwd;
            byte[] binaryAuthorization = System.Text.Encoding.UTF8.GetBytes(authorization);
            authorization = Convert.ToBase64String(binaryAuthorization);
            return authorization;
        }

        public static void SetLoginInfo()
        {
            DialogResult res;
            WebLoginForm webForm = new WebLoginForm();
           // System.Windows.Forms.Application.Run(webForm);
            res = webForm.ShowDialog(null);
            if (webForm.status == DialogResult.OK)
            {
                string userName = webForm.userName;
                string pwd = webForm.pwd;
                m_authCode = GenerateAuthCode(userName, pwd);
            }
            webForm.Dispose();
        }
        public static void AddToMessageQueue(string msg)
        {
            m_messageQueue.Enqueue(msg);
        }
        public static string GetFromMessageQueue()
        {
            string msg = m_messageQueue.Dequeue();
            return msg;
        }
        /// <summary>
        /// Handles the open call.  Pass in the App Id to open the file.
        /// </summary>
        /// <param name="appID">The app Id string ABD, OOPM, USTN, IMC</param>
        /// <param name="fullFileName">the location of the file to open</param>
        /// <param name="readOnly">true opens the file readonly.</param>
        public static void  OpenFileCmd(string appID, string fullFileName, bool readOnly)
        {
            if (m_hostApp == null)
            {
                List<BentleyProductInfo> apps = GetBentleyApps();
                BentleyProductInfo app = GetPath(appID, apps);

                if (app == null)
                    app = GetPath("MSTN", apps);

                //there is no mstn on the box we quit..
                if (app == null)
                    return;

                ProcessStartInfo psi = new ProcessStartInfo(app.FullPath);//(@"C:\Program Files (x86)\Bentley\MicroStationV8iSS3\MicroStation\ustation.exe");
                psi.UseShellExecute = true;
                psi.Arguments = "\"" + fullFileName + "\"";
                m_hostApp = Process.Start(psi);
                m_hostApp.WaitForInputIdle();
                Thread.Sleep(10 * 1000);
            }
            else
            {
                Thread.Sleep(10 * 1000);
                GetMSApp().OpenDesignFile(fullFileName);
            }
         
        }
        public static void CloseHost()
        {
            if (m_hostApp != null)
            {
                GetMSApp().Quit();
                m_hostApp = null;
            }
        }
        
        /// <summary>
        /// this will take in the file being downloaded and attach it
        /// to a working file that is the  same name just _ in front.
        /// </summary>
        /// <param name="fileName"></param>
        static void CreateAndOpenWorkFile(string fileName)
        {
            BIM.Application msApp = GetMSApp();
            BIM.Workspace wkspc = msApp.ActiveWorkspace;
            string workPath = wkspc.ExpandConfigurationVariable("$(MS_DEF)");
            string workFileName = System.IO.Path.GetFileName(fileName);
            string workFilePathName = workPath + "_" + workFileName;
            string seedFileName = wkspc.ConfigurationVariableValue("MS_DESIGNSEED", true);
            BIM.DesignFile workFile = msApp.CreateDesignFile(seedFileName, workFilePathName,true);
            //BIM.DesignFile wkFile = msApp.OpenDesignFileForProgram(workFilePathName, true);

            BIM.Attachment att = workFile.DefaultModelReference.Attachments.AddCoincident(fileName, null, "BaseInformation", "Data", true);
            att.DisplayAsNested = true;
            att.NestLevel = 99;
            att.Rewrite();
            workFile.Save();
            //OpenFileCmd("OPM", wkFile.FullName,true);
        }

        public static void LoadIWPCommand(string jsonString)
        {
            JavaScriptSerializer oJS = new JavaScriptSerializer();
            var jsInfo = oJS.Deserialize<dynamic>(jsonString);

                try
                {
                    //set the appid for the return information.
                    GetMSApp().CadInputQueue.SendKeyin("Wpaddin itemset appid " + jsInfo["AppID"]);
                    
                    string dataString = jsInfo["DataFile"]["ProjID"] + ":" + jsInfo["DataFile"]["appUUID"] + ":" + jsInfo["DataFile"]["dsName"] + ":" + jsInfo["DataFile"]["docGUID"] + ":" +jsInfo["DataFile"]["fileName"];
                    GetMSApp().CadInputQueue.SendKeyin("wpaddin itemset datafile " + dataString);

                    foreach (var oComponent in jsInfo["Components"])
                    {
                        GetMSApp().CadInputQueue.SendKeyin("itemset create " + oComponent["TagID"]); //the tag id is not good for the available group...

                        foreach (var element in oComponent["Elements"])
                            GetMSApp().CadInputQueue.SendKeyin("WPAddin itemset build " + oComponent["TagID"] + ":" + "GUID" + ":" + element["GUID"] + ":" + element["EC_SCHEMA_NAME"]);

                        //client.Send("created set " + name);
                    }
                }
                catch (Exception e)
                {
                    Debug.Print("exception " + e.Message);
                }
                finally
                {
                   // client.Send("finished populating the Available items set");
                    GetMSApp().CadInputQueue.SendKeyin("WPAddin itemset close");
                }
                MessageClient.Send("loaded the items");
        }
        public static string ProcessIWPJSON(string jsonString,WPWebSocketsCmd.Server.WPSWebSocketService client)
        {
            HttpWebResponse status = null;
            string rtnString = "";
            JavaScriptSerializer oJS = new JavaScriptSerializer();
            var jsInfo = oJS.Deserialize<dynamic>(jsonString);
            //Debug.Print(jsInfo.ToString());
            MessageClient = client;
            StringBuilder sb = new StringBuilder(@"https://localhost:3000/loginToPW");
            sb.Append("?ProjID=" + jsInfo["DataFile"]["ProjID"]);
            sb.Append("&appUUID=" + jsInfo["DataFile"]["appUUID"]);
            sb.Append("&dsName=" + jsInfo["DataFile"]["dsName"]);
            //the doc guid could be null so don't put a null  in the url.
            string docGUID = jsInfo["DataFile"]["docGUID"];
            if(docGUID!=null)
                sb.Append("&docGUID=" + docGUID);
            string fileNameString = jsInfo["DataFile"]["fileName"];
            string encodedName = HttpUtility.UrlEncode(fileNameString);
            sb.Append("&fileName=" + encodedName);

            //sb.Append("52574814-03ac-4345-bb8f-ac4f1e777f4b");
            
            //int nPos = fileNameString.LastIndexOf('/');
            //string rootName = fileNameString.Substring(nPos);
            string rootName2 = Path.GetFileName(fileNameString);
            
            Debug.Print(sb.ToString());

            MyWebRequest myWebRequest = new MyWebRequest();

            status = myWebRequest.SendRequest(sb.ToString(), "GET", "", @"c:\temp\" + rootName2);

            if ((status.StatusCode == HttpStatusCode.OK) && (status.ContentType.Equals("application/octet-stream")))
            {
                AddToMessageQueue("WPS>WORK>" + @"c:\temp\" + rootName2);
                AddToMessageQueue("WPS>LOAD>" + jsonString);

                OpenFileCmd("OPM", @"c:\temp\" + rootName2, true);

            }
            return rtnString;
        }
        /// <summary>
        /// gets the file from PW store using the WSG api. 
        /// </summary>
        /// <param name="url">The PW URL for the file includes GUID</param>
        /// <param name="client">The client that called this so that the information can be 
        /// passed back.</param>
        /// <returns>the name of the file that was pulled.</returns>
        public static string GetFileFromWSGByGUID(string url,WPWebSocketsCmd.Server.WPSWebSocketService client,string targetName)
        {
            HttpWebResponse status=null;
            string fileName="";
            try
            {

               // if (null == m_authCode)
               //     SetLoginInfo();

                MyWebRequest myWebRequest = new MyWebRequest();
                //only going to send the auth code as Base64 string 
                myWebRequest.SetAuthCode(m_authCode);
                
                //this will get the file information as a JSON Object Array
                status = myWebRequest.SendRequest(url, "GET", "",targetName);

                if (status.StatusCode == HttpStatusCode.OK)
                {
                    JavaScriptSerializer oJS = new JavaScriptSerializer();
                    if (status.ContentType.Equals("application/json"))
                    {
                        string s;
                        StreamReader sr = new StreamReader(status.GetResponseStream());
                        s = sr.ReadToEnd();
                        sr.Close();

                        var stuff = oJS.Deserialize<dynamic>(s);
                        fileName = stuff["instances"][0]["properties"]["FileName"];

                        if (fileName.Length > 0)
                            client.Send("fetching file " + fileName);
                        else
                            client.Send("File Not Found");

                        if (fileName.Length > 0)
                            status = myWebRequest.SendRequest(url, "GET", "/$file", @"c:\temp\" + fileName);

                        //open the file in mstn: stuff["instances"][0]["properties"]["FileName"]
                        if (fileName.Length > 0)
                            OpenFileCmd("MSTN", @"c:\temp\" + fileName, false);
                    }
                }
            }
            catch (Exception e) 
            { 
                Debug.Print(e.Message);
                Debug.Print("the server returned " + status.ToString());
            }

            if (fileName.Length > 2)
                return fileName;
            else
                return status.ToString();
        }
        /// <summary>
        /// move components from one group to another.  This is mostly used to put things
        /// into an IWP from the Available group.  This is done by using a keyin in the 
        /// host product.
        /// </summary>
        /// <param name="source">Where to pull elements</param>
        /// <param name="target">Where to put elements</param>
        /// <param name="prop">The key property</param>
        /// <param name="value">The key value to search for</param>
        public static void MoveToGroup(string source, string target, string prop, string value)
        {
            GetMSApp().CadInputQueue.SendKeyin(source + ":" + target + ":" + prop + ":" + value);
        }
        /// <summary>
        /// calls the element analyze command to select and element by id.  demo code.
        /// </summary>
        /// <param name="elID">The id of element</param>
        public static void SelectElementById(string elID)
        {
            GetMSApp().CadInputQueue.SendKeyin("analyze element byid " + elID);
        }

        /// <summary>
        /// sends a keyin string across to the running MicroStation instance.
        /// </summary>
        /// <param name="unparsed"></param>
        public static void SendKeyin(string unparsed)
        {
            GetMSApp().CadInputQueue.SendKeyin(unparsed);
        }
        /// <summary>
        /// run the rd= command to change the active file
        /// </summary>
        /// <param name="unparsed">The full path name of the new file</param>
        public static void rdEquals(string unparsed)
        {
            GetMSApp().CadInputQueue.SendKeyin("rd=" + unparsed);
        }
        /// <summary>
        /// makes the work file which will contain the current target file...
        /// </summary>
        /// <param name="unparsed"></param>
        public static void OpenForWork(string unparsed)
        {
            GetMSApp().CadInputQueue.SendKeyin("WPAddin itemset OpenWork " + unparsed);
        }
        /// <summary>
        /// send the command to build a list of elements.  the name is the  list
        /// name.  the element list is a json array that will be deserialized.
        /// </summary>
        /// <param name="name">name of the list</param>
        /// <param name="elementList">JSON array of elements to load.</param>
        public static void ProcessElementList(string name, string elementList, WPWebSocketsCmd.Server.WPSWebSocketService client)
        {
            try
            {
                string output = JsonConvert.SerializeObject(elementList);
                var lo = JsonConvert.DeserializeObject<dynamic>(elementList);
                GetMSApp().CadInputQueue.SendKeyin("itemset create " + name);

                foreach (var d in lo["data"])
                    GetMSApp().CadInputQueue.SendKeyin("WPAddin itemset build " + name + ":" + "GUID" + ":" + d["GUID"]);
                    
                client.Send("created set " + name);

                GetMSApp().CadInputQueue.SendKeyin("WPAddin itemset close");
               
            }
            catch (Exception e)
            {
                Debug.Print("exception " + e.Message);
            }
            return;
        }
        /// <summary>
        /// gets the elements in list based on the property specified.
        /// </summary>
        /// <param name="name">the element list</param>
        /// <param name="prop">the property to key</param>
        public static void GetElementList(string name,string prop)
        {
            GetMSApp().CadInputQueue.SendKeyin("WPAddin itemset get " + name + ":" + prop);
        }
        /// <summary>
        /// register a connection....
        /// </summary>
        /// <param name="serviceID"></param>
        public static void RegConnection(string serviceID)
        {

        }
        /// <summary>
        /// get the product identity from the long name
        /// </summary>
        /// <param name="name">full product name</param>
        /// <returns></returns>
        private static string IdentFromName(string name)
        {
            string sName = "";
            switch (name)
            {
                case "AECOsimBuildingDesigner":
                    sName = "ABD";
                    break;
                case "MicroStation":
                    sName = "MSTN";
                    break;
                case "OpenPlantModeler":
                    sName = "OPM";
                    break;
                case "imodelComposer":
                    sName = "IMC";
                    break;
                default:
                    sName = name;
                    break;
            }
            return sName;
        }

        /// <summary>
        /// get a list of the installed Bentley products.
        /// </summary>
        /// <returns>List of type BentleyProductInfo </returns>
        private static List<BentleyProductInfo> GetBentleyApps()
        {
            List<BentleyProductInfo> appPaths = new List<BentleyProductInfo>();
            BentleyProductInfo prodInfo;
            string appPath = @"SOFTWARE\Bentley\Installed_Products";
            RegistryKey rkBase = null;
            rkBase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
 
            using (RegistryKey rk = rkBase.OpenSubKey(appPath))
            {
                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                        try
                        {
                            prodInfo = new BentleyProductInfo();

                            if (sk.GetValue("DisplayIcon") != null)
                            {
                                prodInfo.FullPath = sk.GetValue("DisplayIcon").ToString();
                            }
                            if (sk.GetValue("ProductName") != null)
                            {
                                prodInfo.FullName = sk.GetValue("ProductName").ToString();
                                prodInfo.Ident = IdentFromName(prodInfo.FullName);
                            }
                            appPaths.Add(prodInfo);
                        }
                        catch (Exception ex)
                        {
                            Debug.Print(ex.Message);
                        }
                }
            }
            return appPaths;
        }
        /// <summary>
        /// get the BentleyProductInfo for a specific application.
        /// The application ids are:
        /// ABD - AECOsimBuildingDesigner
        /// OPM - OpenPlantModeler
        /// USTN - MicroStation
        /// IMC - ImodelComposer
        /// </summary>
        /// <param name="appID">App Id String</param>
        /// <param name="appPaths">List of Application paths</param>
        /// <returns>a BentleyProductInfo</returns>
        private static BentleyProductInfo GetPath(string appID,List<BentleyProductInfo> appPaths)
        {
            foreach (BentleyProductInfo product in appPaths)
                if (product.Ident.Equals(appID))
                    return product;

            return null;
        }
        private string ParseJSON(string jString)
        {
            string rtnString="";
            
            
            return rtnString;
        }

        internal static string ProcessJSON(string jsonString)
        {
            JavaScriptSerializer oJS = new JavaScriptSerializer();
            var jsInfo = oJS.Deserialize<dynamic>(jsonString);
            return "processed";
        }
    }
    /// <summary>
    /// a helper class that can help with JSON conversions
    /// </summary>
    class BentleyProductInfo
    {
        /// <summary>
        /// this is the  identifier string that will be used to call the applicaiton
        /// usually ABD, IMC, OPM, or MSTN
        /// </summary>
        public string Ident { get; set; }
        /// <summary>
        /// the full formal name for the product.
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// the full path to the executable file
        /// </summary>
        public string FullPath { get; set; }
    }
    /// <summary>
    /// a helper class for converting a JSON to an object.
    /// </summary>
    public class FileDataList
    {
        public List<FileDataItem> items { get; set; }
    }
    /// <summary>
    /// a helper class for the conversion of JSON to objects.
    /// </summary>
    public class FileDataItem
    {
        public IDictionary<string, object> item { get; set; }
    }
    /// <summary>
    /// helper class for element data.  This is a list that will
    /// be used by the deserializer to extract JSON formated information
    /// </summary>
    public class ElmList
    {
        public List<ElmData> data { get; set; }
    }
    /// <summary>
    /// helper class for  element data.  This is assuming that the element
    /// data is an id.
    /// </summary>
    public class ElmData
    {
        public IDictionary<string, string> item { get; set; }
    }
}
