using System;
using System.IO;
using System.Net;
using System.Net.Security;

namespace WPWebSocketsCmd
{
    public class MyWebRequest
    {
        private WebRequest request;
        private string m_userName { get; set; }
        private string m_password { get; set; }


        public String Status { get; set; }
        public MyWebRequest() { }
        public MyWebRequest(string url)
        {
        }

        public MyWebRequest(string url, string method)
            : this(url)
        {

            if (method.Equals("GET") || method.Equals("POST"))
            {
                // Set the Method property of the request to POST.
                request.Method = method;
            }
            else
            {
                throw new Exception("Invalid Method Type");
            }
        }
        public void SetUserInfo(string user, string pwd)
        {
            m_userName = user;
            m_password = pwd;
        }
        /// <summary>
        /// a utility to debug the user name and password passing.
        /// </summary>
        /// <param name="base64Encodeddata"></param>
        /// <returns></returns>
        private static string Base64Decode(string base64Encodeddata)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64Encodeddata);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

        }
        /// <summary>
        /// gets the authorization string from the user name and password
        /// </summary>
        /// <param name="authType"></param>
        /// <returns></returns>
        private string getAuthorization(string authType)
        {
            string authorization;// = "dwg_conv" + "xscdvf!";
            authorization = m_userName + ":" + m_password;
            byte[] binaryAuthorization = System.Text.Encoding.UTF8.GetBytes(authorization);
            authorization = Convert.ToBase64String(binaryAuthorization);
            //            authorization = "Basic " + authorization;
            //authorization = "Basic TkFccXp4Y2R5Ok5vdmE4OENhdHMh";// "Basic ZHdnX2NvbnY6eHNjZHZmIQ==";
            return authType + " " + authorization;
        }
        public MyWebRequest(string url, string method, string data)
            : this(url, method)
        {
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = 0;// byteArray.Length;
            request.Headers.Add("Authorization", getAuthorization("Basic"));
        }
        public string SendRequest(string url, string method, string data,string destination)
        {
            string rtnMessage = "";

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new
                RemoteCertificateValidationCallback(delegate
                {
                    return true;
                });//hack for  my certificate challenged installation.

                request = WebRequest.Create(url + data);
                request.Timeout = -1;
                request.ContentType = "appplication/json";

                request.Method = method;
                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";

                // Set the ContentLength property of the WebRequest.
                request.ContentLength = 0;// byteArray.Length;

                request.Headers.Add("Authorization", getAuthorization("Basic"));
                return GetResponse(data.Length > 0,destination);
            }
            catch (WebException we) { rtnMessage = we.Message; }
            catch (IOException ie) { rtnMessage = ie.Message; }
            catch (Exception e) { rtnMessage = e.Message; }
            return rtnMessage;
        }
        void CopyTo(StreamReader from, FileStream to)
        {
            char[] buffer = new char[0x10000];

            int read = from.Read(buffer, 0, buffer.Length);
            while (read > 0)
            {
                to.Write(System.Text.Encoding.UTF8.GetBytes(buffer), 0, read);
                read = from.Read(buffer, 0, buffer.Length);
            }
            to.Close();
        }

        /// <summary>
        /// Copies the contents of input to output. Doesn't close either stream.
        /// </summary>
        public static int CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            int bytesCopied = 0;

            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
                bytesCopied += len;
            }
            return bytesCopied;
        }

        public string GetResponse(bool saveToFile,string destination)
        {
            // Get the original response.
            WebResponse response = request.GetResponse();
            //string data = "c:\\temp\\temp.dgn";
            this.Status = ((HttpWebResponse)response).StatusDescription;

            // Read the content fully up to the end.
            string responseFromServer;// = reader.ReadToEnd();

            using (Stream reader = response.GetResponseStream())
            {
                if (saveToFile)
                {
                    int copied;
                    using (Stream file = File.Create(destination))
                    {
                        copied = CopyStream(reader, file);
                    }
                    responseFromServer = copied.ToString();
                }
                else
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    responseFromServer = sr.ReadToEnd();
                    sr.Close();
                }
                //Console.WriteLine(s);
                reader.Close();
            }
            response.Close();

            return responseFromServer;
        }

    }
}
