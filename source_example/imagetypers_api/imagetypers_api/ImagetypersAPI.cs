using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace ImageTypers
{
    public class ImagetypersAPI
    {
        // consts
        private static string CAPTCHA_ENDPOINT = "http://captchatypers.com/Forms/UploadFileAndGetTextNEW.ashx";
        private static string RECAPTCHA_SUBMIT_ENDPOINT = "http://captchatypers.com/captchaapi/UploadRecaptchaV1.ashx";
        private static string RECAPTCHA_RETRIEVE_ENDPOINT = "http://captchatypers.com/captchaapi/GetRecaptchaText.ashx";
        private static string BALANCE_ENDPOINT = "http://captchatypers.com/Forms/RequestBalance.ashx";
        private static string BAD_IMAGE_ENDPOINT = "http://captchatypers.com/Forms/SetBadImage.ashx";
        private static string USER_AGENT = "csharpAPI1.0";      // user agent used in requests

        private string _username;
        private string _password;
        private string _ref_id = "0";
        private int _timeout = 120000;

        private Captcha _captcha;
        private Recaptcha _recaptcha = null;

        private string _error = "";

        /// <summary>
        /// Takes account username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public ImagetypersAPI(string username, string password)
        {
            this._username = username;
            this._password = password;
        }

        /// <summary>
        /// Takes account username, password and reference ID
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="ref_id"></param>
        public ImagetypersAPI(string username, string password, string ref_id)
        {
            this._username = username;
            this._password = password;
            this._ref_id = ref_id;
        }

        #region captcha & recaptcha
        /// <summary>
        /// Solve normal captcha
        /// </summary>
        /// <param name="captcha_file">captcha image file location</param>
        /// <param name="case_sensitive">case sensitive mode (true or false)</param>
        /// <returns></returns>
        public string solve_captcha(string captcha_file, bool case_sensitive = false)
        {
            // check if file/captcha image exists
            if (!File.Exists(captcha_file))
            {
                throw new Exception(string.Format("captcha image file does not exist: {0}", captcha_file));
            }

            Byte[] bytes = File.ReadAllBytes(captcha_file);
            
            string file_data = Convert.ToBase64String(bytes);

            // use name value collection in this case
            var d = new Dictionary<string, string>();
            d.Add("action", "UPLOADCAPTCHA");
            d.Add("username", this._username);
            d.Add("password", this._password);
            d.Add("chkCase", (case_sensitive) ? "1" : "0");
            d.Add("refid", this._ref_id);
            d.Add("file",  System.Web.HttpUtility.UrlEncode(file_data));

            // string fro dict
            var post_data = "";
            foreach (var e in d)
            {
                post_data += string.Format("{0}={1}&", e.Key, e.Value);
            }

            post_data =post_data.Substring(0, post_data.Length - 1);
            string response = Utils.POST(CAPTCHA_ENDPOINT, post_data, USER_AGENT, this._timeout);       // make request
            if (response.Contains("ERROR:"))
            {
                var response_err = response.Split(new string[] { "ERROR:" }, StringSplitOptions.None)[1].Trim();
                this._error = response_err;
                throw new Exception(response_err);
            }

            response = response.Replace("Uploading file...", "");

            var c = new Captcha(response);      // create captcha obj
            this._captcha = c;
            return this._captcha.text();        // return captcha text
        }

        #region recaptcha
        /// <summary>
        /// Submit recaptcha and get it's captcha ID
        /// Check API docs for more info on how to get page_url and sitekey
        /// </summary>
        /// <param name="page_url">page url (check API docs)</param>
        /// <param name="sitekey">site key (check API docs)</param>
        /// <param name="proxy">IP:Port (optional)</param>
        /// <param name="proxy_type">ProxyType (optional)</param>
        /// <returns></returns>
        public string submit_recaptcha(string page_url, string sitekey, string proxy = "", string proxy_type = "")
        {
            // check given vars
            if (string.IsNullOrWhiteSpace(page_url))
            {
                throw new Exception("page_url variable is null or empty");
            }
            if (string.IsNullOrWhiteSpace(sitekey))
            {
                throw new Exception("sitekey variable is null or empty");
            }

            // create dict (params)
            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("action", "UPLOADCAPTCHA");
            d.Add("username", this._username);
            d.Add("password", this._password);
            d.Add("pageurl", page_url);
            d.Add("googlekey", sitekey);
            d.Add("refid", this._ref_id);
            // add proxy params if given
            if (!string.IsNullOrWhiteSpace(proxy) && !string.IsNullOrWhiteSpace(proxy_type))
            {
                d.Add("proxy", proxy);
                d.Add("proxy_type", proxy_type);
            }

            var post_data = Utils.list_to_params(d);        // transform dict to params
            string response = Utils.POST(RECAPTCHA_SUBMIT_ENDPOINT, post_data, USER_AGENT, this._timeout);       // make request
            if (response.Contains("ERROR:"))
            {
                var response_err = response.Split(new string[] { "ERROR:" }, StringSplitOptions.None)[1].Trim();
                this._error = response_err;
                throw new Exception(response_err);
            }

            // set as recaptcha [id] response and return
            var r = new Recaptcha(response);
            this._recaptcha = r;
            return this._recaptcha.captcha_id();        // return captcha id
        }

        /// <summary>
        /// Retrieve recaptcha response using captcha ID
        /// </summary>
        /// <param name="captcha_id"></param>
        /// <returns></returns>
        public string retrieve_captcha(string captcha_id)
        {
            // check if ID is OK
            if (string.IsNullOrWhiteSpace(captcha_id))
            {
                throw new Exception("captcha ID is null or empty");
            }

            // init params object
            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("action", "GETTEXT");
            d.Add("username", this._username);
            d.Add("password", this._password);
            d.Add("captchaid", captcha_id);
            d.Add("refid", this._ref_id);

            var post_data = Utils.list_to_params(d);        // transform dict to params
            string response = Utils.POST(RECAPTCHA_RETRIEVE_ENDPOINT, post_data, USER_AGENT, this._timeout);       // make request
            if (response.Contains("ERROR:"))
            {
                var response_err = response.Split(new string[] { "ERROR:" }, StringSplitOptions.None)[1].Trim();
                // in this case, if we get NOT_DECODED, we don't need it saved to obj
                // because it's used by bool in_progress(int captcha_id) as well
                if (!response_err.Contains("NOT_DECODED"))
                {
                    this._error = response_err;
                }
                throw new Exception(response_err);
            }

            // set as recaptcha response and return
            this._recaptcha = new Recaptcha(captcha_id);
            this._recaptcha.set_response(response);
            return this._recaptcha.response();        // return captcha text
        }

        /// <summary>
        /// Tells if the recaptcha is still in progress (still being decoded)
        /// </summary>
        /// <param name="captcha_id"></param>
        /// <returns></returns>
        public bool in_progress(string captcha_id)
        {
            try
            {
                this.retrieve_captcha(captcha_id);          // try to retrieve captcha
                return false;       // no error, we're good
            }
            catch (Exception ex)
            {
                // if "known" error, still in progress
                if (ex.Message.Contains("NOT_DECODED"))
                {
                    return true;
                }
                // otherwise throw exception (if different error)
                throw ex;
            }
        }
        #endregion
        #endregion

        #region others
        /// <summary>
        /// Get account balance
        /// </summary>
        /// <returns></returns>
        public string account_balance()
        {
            // create dict with params
            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("action", "REQUESTBALANCE");
            d.Add("username", this._username);
            d.Add("password", this._password);
            d.Add("submit", "Submit");

            var post_data = Utils.list_to_params(d);        // transform dict to params
            string response = Utils.POST(BALANCE_ENDPOINT, post_data, USER_AGENT, this._timeout);       // make request
            if (response.Contains("ERROR:"))
            {
                var response_err = response.Split(new string[] { "ERROR:" }, StringSplitOptions.None)[1].Trim();
                this._error = response_err;
                throw new Exception(response_err);
            }

            return string.Format("${0}", response);        // return response/balance
        }

        /// <summary>
        /// Set captcha as bad
        /// </summary>
        /// <param name="captcha_id"></param>
        /// <returns></returns>
        public string set_captcha_bad(string captcha_id)
        {
            // check if captcha is OK
            if (string.IsNullOrWhiteSpace(captcha_id))
            {
                throw new Exception("catpcha ID is null or empty");
            }

            // create dict with params
            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("action", "SETBADIMAGE");
            d.Add("username", this._username);
            d.Add("password", this._password);
            d.Add("imageid", captcha_id);
            d.Add("submit", "Submissssst");

            var post_data = Utils.list_to_params(d);        // transform dict to params
            string response = Utils.POST(BAD_IMAGE_ENDPOINT, post_data, USER_AGENT, this._timeout);       // make request
            if (response.Contains("ERROR:"))
            {
                var response_err = response.Split(new string[] { "ERROR:" }, StringSplitOptions.None)[1].Trim();
                this._error = response_err;
                throw new Exception(response_err);
            }

            return response;
        }
        #endregion

        #region misc
        /// <summary>
        /// Set timeout (in seconds)
        /// </summary>
        /// <param name="timeout"></param>
        public void set_timeout(int timeout)
        {
            this._timeout = timeout * 1000;     // requests timeout is in milliseconds, multiply by 1k
        }

        /// <summary>
        /// Get last solved captcha text
        /// </summary>
        /// <returns></returns>
        public string captcha_text()
        {
            if (this._captcha == null)      // check if captcha is null
            {
                return "";
            }
            return this._captcha.text();        // return last solved captcha text
        }
        /// <summary>
        /// Get last solved captcha id
        /// </summary>
        /// <returns></returns>
        public string captcha_id()
        {
            if (this._captcha == null)      // check if captcha is null
            {
                return "";
            }
            return this._captcha.captcha_id();        // return last solved captcha text
        }

        /// <summary>
        /// Get last solved recaptcha response
        /// </summary>
        /// <returns></returns>
        public string recaptcha_response()
        {
            if (this._recaptcha == null)
            {
                return "";
            }
            return this._recaptcha.response();      // return response
        }
        /// <summary>
        /// Get last solved recaptcha id
        /// </summary>
        /// <returns></returns>
        public string recaptcha_id()
        {
            if (this._recaptcha == null)        // check if recaptcha is null
            {
                return "";
            }
            return this._recaptcha.captcha_id();        // return recaptcha id
        }

        /// <summary>
        /// Get last error
        /// </summary>
        /// <returns></returns>
        public string error()
        {
            return this._error;
        }
        #endregion
    }
}
