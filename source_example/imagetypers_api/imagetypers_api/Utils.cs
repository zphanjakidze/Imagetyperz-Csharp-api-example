﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace ImageTypers
{
    static class Utils
    {
        /// <summary>
        /// List to string
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string list_to_params(Dictionary<string, string> p)
        {
            string s = "";
            foreach (var e in p)
            {
                s += string.Format("{0}={1}&", e.Key, e.Value);
            }
            s = s.Trim('&');
            return s;
        }
        /// <summary>
        /// POST request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="post_data"></param>
        /// <param name="user_agent"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static string POST(string url, string post_data, string user_agent, int timeout)
        {
            // validate url
            if (!url.StartsWith("http"))
            {
                url = "http://" + url;
            }

            var request = (HttpWebRequest)WebRequest.Create(url);

            var data = Encoding.ASCII.GetBytes(post_data);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            // set user agent and timeout
            request.UserAgent = user_agent;
            request.Timeout = timeout;
            request.ReadWriteTimeout = timeout;

            request.Accept = "*/*";
            //request.ServicePoint.Expect100Continue = false;
            //request.AllowAutoRedirect = false;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            HttpWebResponse response = null;
            response = (HttpWebResponse)request.GetResponse();
            string s = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return s;
        }
    }

}