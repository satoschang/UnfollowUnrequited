using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.Threading;

namespace UnfollowUnrequited
{
    class Program
    {
        static int _remoteDebuggingPort;
        static string _userID;
        static void Main(string[] args)
        {
            try
            {
                Execute(args);
            }
            catch
            {

            }
        }
        static void Execute(string[] args)
        {
            string chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            string userDataDirectory = @"C:\Users\satos\AppData\Local\Google\Chrome\UnfollowUnrequited\User Data";

            for (int remoteDebuggingPort = 1000; remoteDebuggingPort < 10000; remoteDebuggingPort++)
            {
                try
                {
                    // Start Chrome as a new process with the specified command
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = chromePath,
                        Arguments = $"--remote-debugging-port={remoteDebuggingPort} --user-data-dir=\"{userDataDirectory}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    });

                    _remoteDebuggingPort = remoteDebuggingPort;
                    Console.WriteLine("Chrome started with remote debugging enabled.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error starting Chrome: {ex.Message}");
                }
            }

            var driver = new ChromeDriver(new ChromeOptions
            {
                DebuggerAddress = $"127.0.0.1:{_remoteDebuggingPort}"
            });

            driver.Navigate().GoToUrl("https://twitter.com/home");

            IWebElement btnProfile = null;
            try
            {
                btnProfile = driver.FindElement(By.CssSelector("a[aria-label='プロフィール']"));
            }
            catch
            {

            }

            if (btnProfile == null)
            {
                Console.WriteLine("Please log in manually to Twitter and press Enter when ready...");
                Console.ReadLine();

                driver.Navigate().GoToUrl("https://twitter.com/home");

                btnProfile = driver.FindElement(By.CssSelector("a[aria-label='プロフィール']"));
            }

            var userUrl = btnProfile.GetAttribute("href");

            // Find the last slash
            int lastSlashIndex = userUrl.LastIndexOf('/');

            // Extract the string after the last slash
            if (lastSlashIndex >= 0 && lastSlashIndex < userUrl.Length - 1)
            {
                _userID = userUrl.Substring(lastSlashIndex + 1);
            }
            else
            {
                Console.WriteLine("No slash found or it's at the end of the URL.");
                throw new Exception("No slash found or it's at the end of the URL.");
            }

            driver.Navigate().GoToUrl($"https://twitter.com/{_userID}/followers");

            // You may need to scroll through the following list if it's long
            // You can use JavaScript to scroll:
             ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

            // Locate and unfollow accounts that don't follow you back
            //var followingList = driver.FindElements(By.XPath("//div[@data-testid='UserCell']"));

            //foreach (var user in followingList)
            //{
            //    var followStatus = user.FindElement(By.XPath(".//span[contains(@data-testid, 'follows-you-badge')]"));

            //    // Check if the account follows you back
            //    if (followStatus == null)
            //    {
            //        // If they don't follow you back, unfollow them
            //        var unfollowButton = user.FindElement(By.XPath(".//span[text()='Following']/.."));
            //        unfollowButton.Click();

            //        // You may encounter a confirmation dialog; handle it as needed
            //        // Usually, it involves clicking another "Unfollow" button

            //        // Wait for a short time between unfollowing accounts
            //        Thread.Sleep(1000);
            //    }
            //}

            // Close the browser window
            driver.Quit();
        }
    }
}