using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;
using Newtonsoft.Json;
using BIM = Bentley.Interop.MicroStationDGN;

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
        private static string m_userName { get; set; }
        private static string m_pwd{get;set;}

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
                    m_App.CadInputQueue.SendKeyin("mdl silentload WPAddin");
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
        public static void SetLoginInfo()
        {
            DialogResult res;
            WebLoginForm webForm = new WebLoginForm();
           // System.Windows.Forms.Application.Run(webForm);
            res = webForm.ShowDialog(null);
            if (webForm.status == DialogResult.OK)
            {
                m_userName = webForm.userName;
                m_pwd = webForm.pwd;
            }
            webForm.Dispose();
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
                    return;

                ProcessStartInfo psi = new ProcessStartInfo(app.FullPath);//(@"C:\Program Files (x86)\Bentley\MicroStationV8iSS3\MicroStation\ustation.exe");
                psi.UseShellExecute = true;
                psi.Arguments = "\"" + fullFileName + "\"";
                m_hostApp = Process.Start(psi);
            }
            else
            {
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
        /// gets the file from PW store using the WSG api. 
        /// </summary>
        /// <param name="url">The PW URL for the file includes GUID</param>
        /// <param name="client">The client that called this so that the information can be 
        /// passed back.</param>
        /// <returns>the name of the file that was pulled.</returns>
        public static string GetFileFromWSGByGUID(string url,WPWebSocketsCmd.Server.WPSWebSocketService client)
        {
            string status="";
            string fileName="";
            try
            {

                if ((null == m_userName)||(m_userName.Length < 1))
                    SetLoginInfo();

                MyWebRequest myWebRequest = new MyWebRequest();

                myWebRequest.SetUserInfo(m_userName, m_pwd);
                
                //this will get the file information as a JSON Object Array
                status = myWebRequest.SendRequest(url, "GET", ""," ");

                JavaScriptSerializer oJS = new JavaScriptSerializer();
                var stuff = oJS.Deserialize<dynamic>(status);
                fileName = stuff["instances"][0]["properties"]["FileName"];
                
                if (fileName.Length > 0)
                    client.Send("fetching file " + fileName);
                else
                    client.Send("File Not Found");

                if(fileName.Length>0)
                    status = myWebRequest.SendRequest(url, "GET", "/$file", @"c:\temp\" + fileName);

                //open the file in mstn: stuff["instances"][0]["properties"]["FileName"]
                if(fileName.Length>0)
                    OpenFileCmd("MSTN", @"c:\temp\" + fileName, false);
            }
            catch (Exception e) 
            { 
                Debug.Print(e.Message);
                Debug.Print("the server returned " + status);
            }

            if (fileName.Length > 2)
                return fileName;
            else
                return status;
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
