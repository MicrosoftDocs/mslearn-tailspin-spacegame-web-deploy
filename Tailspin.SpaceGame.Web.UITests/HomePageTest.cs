using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections;

namespace UITests
{
    [TestFixture("Chrome")]
    [TestFixture("Firefox")]
    [TestFixture("IE")]
    public class HomePageTest
    {
        private string browser;
        private IWebDriver driver;

        public HomePageTest(string browser)
        {
            this.browser = browser;
        }

        [OneTimeSetUp]
        public void Setup()
        {
            try
            {
                // The NuGet package for each browser installs driver software
                // under the bin directory, alongside the compiled test code.
                // This tells the driver class where to find the underlying driver software.
                var cwd = Environment.CurrentDirectory;

                // Create the driver for the current browser.
                switch(browser)
                {
                  case "Chrome":
                    driver = new ChromeDriver(cwd);
                    break;
                  case "Firefox":
                    driver = new FirefoxDriver(cwd);
                    break;
                  case "IE":
                    driver = new InternetExplorerDriver(cwd);
                    break;
                  default:
                    throw new ArgumentException($"'{browser}': Unknown browser");
                }

                // Wait until the page is fully loaded on every page navigation or page reload.
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);

                // Navigate to the site.
                // The site name is stored in the SITE_URL environment variable to make 
                // the tests more flexible.
                string url = Environment.GetEnvironmentVariable("SITE_URL");
                driver.Navigate().GoToUrl(url + "/");

                // Wait for the page to be completely loaded.
                new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                    .Until(d => ((IJavaScriptExecutor) d)
                        .ExecuteScript("return document.readyState")
                        .Equals("complete"));
            }
            catch (DriverServiceNotFoundException)
            {
            }
            catch (WebDriverException)
            {
                Cleanup();
            }
        }
    
        [OneTimeTearDown]
        public void Cleanup()
        {
            if (driver != null)
            {
                driver.Quit();
            }
        }

        // Download game
        [TestCase(
            "/html/body/div/div/section[2]/div[2]/a",
            "//*[@id=\"pretend-modal\"]/div/div")]
        // Screen image
        [TestCase(
            "/html/body/div/div/section[3]/div/ul/li[1]/a",
            "/html/body/div[1]/div/div[2]")]
        // Top player on the leaderboard
        [TestCase(
            "/html/body/div/div/section[4]/div/div/div[1]/div[2]/div[2]/div/a/div",
            "//*[@id=\"profile-modal-1\"]/div/div")]
        public void ClickLinkByXPath_ShouldDisplayModalByXPath(string linkXPath, string modalXPath)
        {
            // Skip the test if the driver could not be loaded.
            // This happens when the underlying browser is not installed.
            if (driver == null)
            {
                Assert.Ignore();
                return;
            }

            // Locate the link by its XPath and then click the link.
            ClickElement(FindElement(By.XPath(linkXPath)));

            // Locate the resulting modal.
            IWebElement modal = FindElement(By.XPath(modalXPath));

            // Record whether the modal was successfully displayed.
            bool modalWasDisplayed = (modal != null && modal.Displayed);

            // Close the modal if it was displayed.
            if (modalWasDisplayed)
            {
                // Click the close button that's part of the modal.
                ClickElement(FindElement(By.ClassName("close"), modal));
                
                // Wait for the modal to close and for the main page to again be clickable.
                FindElement(By.TagName("body"));
            }

            // Assert that the modal was displayed successfully.
            // If it wasn't, this test will be recorded as failed.
            Assert.That(modalWasDisplayed, Is.True);
        }

        private IWebElement FindElement(By locator, IWebElement parent = null, int timeoutSeconds = 10)
        {
            // WebDriverWait enables us to wait for the specified condition to be true
            // within a given time period.
            return new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds))
                .Until(c => {
                    IWebElement element = null;
                    // If a parent was provided, find its child element.
                    if (parent != null)
                    {
                        element = parent.FindElement(locator);
                    }
                    // Otherwise, locate the element from the root of the DOM.
                    else
                    {
                        element = driver.FindElement(locator);
                    }
                    // Return true once the element is displayed and able to receive user input.
                    return (element != null && element.Displayed && element.Enabled) ? element : null;
                });
        }

        private void ClickElement(IWebElement element)
        {
            // We expect the driver to implement IJavaScriptExecutor.
            // IJavaScriptExecutor enables us to execute JavaScript code during the tests.
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;

            // Through JavaScript, run the click() method on the underlying HTML object.
            js.ExecuteScript("arguments[0].click();", element);
        }
    }
}