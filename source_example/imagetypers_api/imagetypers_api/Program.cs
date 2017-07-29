using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ImageTypers;

namespace ImageTypers
{
    class Program
    {
        /// <summary>
        /// Test API
        /// </summary>
        static void test_api()
        {
            // change to your own username & password
            // -----------------------------------------
            string username = "user name here";
            string password = "password here";

            // init imagetypersAPI obj with username and password
            ImagetypersAPI i = new ImagetypersAPI(username, password);
            // balance
            // ------------
            string balance = i.account_balance();
            Console.WriteLine(string.Format("Balance: {0}", balance));
            // ==========================================================================================
            // captcha
            // ----------
            Console.WriteLine("Waiting for captcha to be solved ...");
            string captcha_text = i.solve_captcha("captcha.jpg", true);       // solve normal captcha
            Console.WriteLine(string.Format("Captcha text: {0}", captcha_text));
            // ==========================================================================================
            // recaptcha
            // ----------
            // submit
            // -------
            // check http://www.imagetyperz.com/Forms/recaptchaapi.aspx for more details 
            // about how to get the page_url and sitekey
            string page_url = "page url here";
            string sitekey = "your site key here";

            string captcha_id = i.submit_recaptcha(page_url, sitekey);
            Console.WriteLine("Waiting for recaptcha to be solved ...");

            // retrieve
            // ---------
            while (i.in_progress(captcha_id))       // check if it's still being decoded
            {
                System.Threading.Thread.Sleep(10000);      // sleep for 10 seconds
            }

            // we got a response at this point
            // ---------------------------------
            string recaptcha_response = i.retrieve_captcha(captcha_id);     // get the response
            Console.WriteLine(string.Format("Recaptcha response: {0}", recaptcha_response));

            // Other examples
            // ----------------
            // ImagetypersAPI i = new ImagetypersAPI(username, password, 123);     // init with refid
            // i.set_timeout(10);                                                  // set timeout to 10 seconds
            // Console.WriteLine(i.set_captcha_bad(captcha_id));                   // set captcha bad
            // i.submit_recaptcha(page_url, sitekey, "127.0.0.1:1234", "HTTP");    // solve recaptcha with proxy

            // Console.WriteLine(i.captcha_id());                       // last captcha solved id
            // Console.WriteLine(i.captcha_text());                     // last captcha solved text
            // Console.WriteLine(i.recaptcha_id());                     // last recaptcha solved id
            // Console.WriteLine(i.recaptcha_response());               // last recaptcha solved response
            // Console.WriteLine(i.error());                            // last error
        }

        /// <summary>
        /// Command line method
        /// </summary>
        /// <param name="args"></param>
        static void command_line(string[] args)
        {
            new CommandLineController(args).run();      // run command-line controller
        }

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                Program.test_api();          // test API
                //Program.command_line(args);     // run command-line controller
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error occured: {0}", ex.Message));
            }
            finally
            {
                // disabled for command-line mode
                Console.WriteLine("FINISHED ! Press ENTER to close window ...");
                Console.ReadLine();
            }
        }
    }
}