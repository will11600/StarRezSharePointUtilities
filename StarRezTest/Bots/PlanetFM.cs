using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using StarRezTest.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.Bots
{
    public class PlanetFM : ChromeBot
    {
        private const string LogAJobURL = "NonPMJobs/LogACall";

        public override string BaseUrl => Secrets.GetProperty("planetFmUrl");

        public Form SignInForm;

        private Report _OperatingOn = default!;
        public Report OperatingOn
        {
            get => _OperatingOn;
            set
            {
                if (OperatingOn?.Room?.Location?.PlanetFMLogin.Username != value.Room.Location?.PlanetFMLogin.Username)
                {
                    Deauthenticate();
                    Thread.Sleep(400);
                }

                _OperatingOn = value;
                Driver.Navigate().Refresh();
            }
        }

        public PlanetFM() : base()
        {
            CreateForms();
        }

        public override void Submit()
        {
            RunJSOnPage("$($('form')[0]).submit();");
        }

        public override void Deauthenticate()
        {
            RunJSOnPage("navBarClickSub(9, '/PlanetPortal/Account/LogOff')");
        }

        public override void Authenticate()
        {
            bool success = false;
            for (int i = 0; i < 3; i++)
            {
                success = TryCompleteForm(SignInForm, false);
                if (success) { break; }
                Thread.Sleep(400);
            }
            Thread.Sleep(Timeout);
        }

        public bool SetCategory()
        {
            EnsureIsURL(LogAJobURL);

            if (TryFindElement(By.CssSelector("#jtpName_comboBox > svg"), out var categoryField))
            {
                if (!TryClickElement(categoryField!)) { return false; }
            }
            if (TryFindElement(By.Id(OperatingOn.Category), out var selectedCategory))
            {
                if (!TryClickElement(selectedCategory!)) { return false; }
            }

            Thread.Sleep(200);

            ClickNextPageButton();

            return true;
        }

        protected bool ConfirmLocation()
        {
            EnsureIsURL(LogAJobURL);

            if (TryFindElement(By.ClassName("MobileLocationPickerOption"), out var locationPicker))
            {
                if (TryClickElement(locationPicker!))
                {
                    Thread.Sleep(200);
                    ClickNextPageButton();
                    return true;
                }
            }

            return false;
        }

        public void CreateForms()
        {
            CallDetails = new Form(
                LogAJobURL,
                ClickNextPageButton,
                new Field(By.CssSelector("#Call_npmJobDesc"), () => OperatingOn.Title),
                new Field(By.CssSelector("#Call_npmJobDetails"), () => OperatingOn.Title));

            PersonalDetails = new Form(
                LogAJobURL,
                ClickNextPageButton,
                new Field(By.CssSelector("#Call_npmCallerFirstName"), () => "William"),
                new Field(By.CssSelector("#Call_npmCallerLastName"), () => "Brocklesby"),
                new Field(By.CssSelector("#Call_npmTelephone"), () => OperatingOn.Room.Location!.Phone),
                new Field(By.CssSelector("#Call_npmEmail"), () => "w.brocklesby@ucl.ac.uk"));

            Location = new Form(
                LogAJobURL,
                ConfirmLocation,
                new Field(By.CssSelector("#pnlDetail > div.NoAssets.HasMobileLocationPicker > div.MobileLocationPickerContainerArea > div.MobileLocationPickerSearchContainer > div > input"), () => OperatingOn.Room.PlanetReference));

            SignInForm = new Form(
                "Account/Login?ReturnUrl=%2FPlanetPortal%2F",
                () =>
                {
                    if (TryFindElement(By.Id("btnSignIn"), out var element))
                    {
                        return TryClickElement(element!);
                    }
                    return false;
                },
                new Field(By.Id("Username"), () => OperatingOn.Room.Location!.PlanetFMLogin.Username),
                new Field(By.Id("Password"), () => OperatingOn.Room.Location!.PlanetFMLogin.Password));
        }
        public Form CallDetails;
        public Form PersonalDetails;
        public Form Location;

        public bool ClickNextPageButton()
        {
            EnsureIsURL(LogAJobURL);

            return RunJSOnPage("LogACallMessageClick()");
        }
    }
}
