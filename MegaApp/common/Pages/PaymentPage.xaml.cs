using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;

namespace MegaApp.Pages
{
    public partial class PaymentPage : MegaPhoneApplicationPage
    {
        private readonly PaymentViewModel _paymentViewModel;
        private List<Tuple<string, string>> countries;

        public PaymentPage()
        {
            _paymentViewModel = new PaymentViewModel(App.MegaSdk);
            this.DataContext = _paymentViewModel;

            InitializeComponent();

            SetApplicationBar();

            SetListPickers();            
        }

        private void SetApplicationBar()
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Accept.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.Cancel.ToLower();
        }

        private void SetListPickers()
        {
            // Countries List Picker            
            countries = new List<Tuple<string, string>>();
            
            countries.Add(new Tuple<string,string>("US", "United States"));
            countries.Add(new Tuple<string,string>("GB", "United Kingdom"));
            countries.Add(new Tuple<string,string>("CA", "Canada"));
            countries.Add(new Tuple<string,string>("AX", "Aaland Islands"));
            countries.Add(new Tuple<string,string>("AF", "Afghanistan"));
            countries.Add(new Tuple<string,string>("AP", "African Regional Ind.Property organization"));
            countries.Add(new Tuple<string,string>("AL", "Albania"));
            countries.Add(new Tuple<string,string>("DZ", "Algeria"));
            countries.Add(new Tuple<string,string>("AS", "American Samoa"));
            countries.Add(new Tuple<string,string>("AD", "Andorra"));
            countries.Add(new Tuple<string,string>("AO", "Angola"));
            countries.Add(new Tuple<string,string>("AI", "Anguilla"));
            countries.Add(new Tuple<string,string>("AQ", "Antarctica"));
            countries.Add(new Tuple<string,string>("AG", "Antigua and Barbuda"));
            countries.Add(new Tuple<string,string>("AR", "Argentina"));
            countries.Add(new Tuple<string,string>("AM", "Armenia"));
            countries.Add(new Tuple<string,string>("AW", "Aruba"));
            countries.Add(new Tuple<string,string>("AU", "Australia"));
            countries.Add(new Tuple<string,string>("AT", "Austria"));
            countries.Add(new Tuple<string,string>("AZ", "Azerbaijan"));
            countries.Add(new Tuple<string,string>("BS", "Bahamas"));
            countries.Add(new Tuple<string,string>("BH", "Bahrain"));
            countries.Add(new Tuple<string,string>("BD", "Bangladesh"));
            countries.Add(new Tuple<string,string>("BB", "Barbados"));
            countries.Add(new Tuple<string,string>("BY", "Belarus"));
            countries.Add(new Tuple<string,string>("BE", "Belgium"));
            countries.Add(new Tuple<string,string>("BZ", "Belize"));
            countries.Add(new Tuple<string,string>("BJ", "Benin"));
            countries.Add(new Tuple<string,string>("BM", "Bermuda"));
            countries.Add(new Tuple<string,string>("BT", "Bhutan"));
            countries.Add(new Tuple<string,string>("BO", "Bolivia"));
            countries.Add(new Tuple<string,string>("BA", "Bosnia And Herzegowina"));
            countries.Add(new Tuple<string,string>("BW", "Botswana"));
            countries.Add(new Tuple<string,string>("BV", "Bouvet Island"));
            countries.Add(new Tuple<string,string>("BR", "Brazil"));
            countries.Add(new Tuple<string,string>("IO", "British Indian Ocean Territory"));
            countries.Add(new Tuple<string,string>("BN", "Brunei Darussalam"));
            countries.Add(new Tuple<string,string>("BG", "Bulgaria"));
            countries.Add(new Tuple<string,string>("BF", "Burkina Faso"));
            countries.Add(new Tuple<string,string>("BI", "Burundi"));
            countries.Add(new Tuple<string,string>("KH", "Cambodia"));
            countries.Add(new Tuple<string,string>("CM", "Cameroon"));
            countries.Add(new Tuple<string,string>("CV", "Cape Verde"));
            countries.Add(new Tuple<string,string>("KY", "Cayman Islands"));
            countries.Add(new Tuple<string,string>("CF", "Central African Republic"));
            countries.Add(new Tuple<string,string>("TD", "Chad"));
            countries.Add(new Tuple<string,string>("CL", "Chile"));
            countries.Add(new Tuple<string,string>("CN", "China"));
            countries.Add(new Tuple<string,string>("CX", "Christmas Island"));
            countries.Add(new Tuple<string,string>("CC", "Cocos(Keeling)Islands"));
            countries.Add(new Tuple<string,string>("CO", "Colombia"));
            countries.Add(new Tuple<string,string>("KM", "Comoros"));
            countries.Add(new Tuple<string,string>("CG", "Congo"));
            countries.Add(new Tuple<string,string>("CD", "Congo-Kinshasa"));
            countries.Add(new Tuple<string,string>("CK", "Cook Islands"));
            countries.Add(new Tuple<string,string>("CR", "Costa Rica"));
            countries.Add(new Tuple<string,string>("CI", "Cote D'Ivoire"));
            countries.Add(new Tuple<string,string>("HR", "Croatia"));
            countries.Add(new Tuple<string,string>("CU", "Cuba"));
            countries.Add(new Tuple<string,string>("CY", "Cyprus"));
            countries.Add(new Tuple<string,string>("CZ", "Czech Republic"));
            countries.Add(new Tuple<string,string>("DK", "Denmark"));
            countries.Add(new Tuple<string,string>("DJ", "Djibouti"));
            countries.Add(new Tuple<string,string>("DM", "Dominica"));
            countries.Add(new Tuple<string,string>("DO", "Dominican Republic"));
            countries.Add(new Tuple<string,string>("TL", "East Timor"));
            countries.Add(new Tuple<string,string>("EC", "Ecuador"));
            countries.Add(new Tuple<string,string>("EG", "Egypt"));
            countries.Add(new Tuple<string,string>("SV", "El Salvador"));
            countries.Add(new Tuple<string,string>("GQ", "Equatorial Guinea"));
            countries.Add(new Tuple<string,string>("ER", "Eritrea"));
            countries.Add(new Tuple<string,string>("EE", "Estonia"));
            countries.Add(new Tuple<string,string>("ET", "Ethiopia"));
            countries.Add(new Tuple<string,string>("FK", "Falkland Islands"));
            countries.Add(new Tuple<string,string>("FO", "Faroe Islands"));
            countries.Add(new Tuple<string,string>("FJ", "Fiji"));
            countries.Add(new Tuple<string,string>("FI", "Finland"));
            countries.Add(new Tuple<string,string>("FR", "France"));
            countries.Add(new Tuple<string,string>("FX", "France Metropolitan"));
            countries.Add(new Tuple<string,string>("GF", "French Guiana"));
            countries.Add(new Tuple<string,string>("PF", "French Polynesia"));
            countries.Add(new Tuple<string,string>("TF", "French Southern Territories"));
            countries.Add(new Tuple<string,string>("GA", "Gabon"));
            countries.Add(new Tuple<string,string>("GM", "Gambia"));
            countries.Add(new Tuple<string,string>("GE", "Georgia"));
            countries.Add(new Tuple<string,string>("DE", "Germany"));
            countries.Add(new Tuple<string,string>("GH", "Ghana"));
            countries.Add(new Tuple<string,string>("GI", "Gibraltar"));
            countries.Add(new Tuple<string,string>("GR", "Greece"));
            countries.Add(new Tuple<string,string>("GL", "Greenland"));
            countries.Add(new Tuple<string,string>("GD", "Grenada"));
            countries.Add(new Tuple<string,string>("GP", "Guadeloupe"));
            countries.Add(new Tuple<string,string>("GU", "Guam"));
            countries.Add(new Tuple<string,string>("GG", "Guernsey"));
            countries.Add(new Tuple<string,string>("GT", "Guatemala"));
            countries.Add(new Tuple<string,string>("GN", "Guinea"));
            countries.Add(new Tuple<string,string>("GW", "Guinea-Bissau"));
            countries.Add(new Tuple<string,string>("GY", "Guyana"));
            countries.Add(new Tuple<string,string>("HT", "Haiti"));
            countries.Add(new Tuple<string,string>("HN", "Honduras"));
            countries.Add(new Tuple<string,string>("HK", "Hong Kong"));
            countries.Add(new Tuple<string,string>("HU", "Hungary"));
            countries.Add(new Tuple<string,string>("IS", "Iceland"));
            countries.Add(new Tuple<string,string>("IN", "India"));
            countries.Add(new Tuple<string,string>("ID", "Indonesia"));
            countries.Add(new Tuple<string,string>("IR", "Iran"));
            countries.Add(new Tuple<string,string>("IQ", "Iraq"));
            countries.Add(new Tuple<string,string>("IE", "Ireland"));
            countries.Add(new Tuple<string,string>("IM", "Isle of Man"));
            countries.Add(new Tuple<string,string>("IL", "Israel"));
            countries.Add(new Tuple<string,string>("IT", "Italy"));
            countries.Add(new Tuple<string,string>("JM", "Jamaica"));
            countries.Add(new Tuple<string,string>("JP", "Japan"));
            countries.Add(new Tuple<string,string>("JE", "Jersey"));
            countries.Add(new Tuple<string,string>("JO", "Jordan"));
            countries.Add(new Tuple<string,string>("KZ", "Kazakhstan"));
            countries.Add(new Tuple<string,string>("KE", "Kenya"));
            countries.Add(new Tuple<string,string>("KI", "Kiribati"));
            countries.Add(new Tuple<string,string>("KW", "Kuwait"));
            countries.Add(new Tuple<string,string>("KG", "Kyrgyzstan"));
            countries.Add(new Tuple<string,string>("LA", "Lao People's Republic"));
            countries.Add(new Tuple<string,string>("LV", "Latvia"));
            countries.Add(new Tuple<string,string>("LB", "Lebanon"));
            countries.Add(new Tuple<string,string>("LS", "Lesotho"));
            countries.Add(new Tuple<string,string>("LR", "Liberia"));
            countries.Add(new Tuple<string,string>("LY", "Libyan Arab Jamahiriya"));
            countries.Add(new Tuple<string,string>("LI", "Liechtenstein"));
            countries.Add(new Tuple<string,string>("LT", "Lithuania"));
            countries.Add(new Tuple<string,string>("LU", "Luxembourg"));
            countries.Add(new Tuple<string,string>("MO", "Macau"));
            countries.Add(new Tuple<string,string>("MK", "Macedonia"));
            countries.Add(new Tuple<string,string>("MG", "Madagascar"));
            countries.Add(new Tuple<string,string>("MW", "Malawi"));
            countries.Add(new Tuple<string,string>("MY", "Malaysia"));
            countries.Add(new Tuple<string,string>("MV", "Maldives"));
            countries.Add(new Tuple<string,string>("ML", "Mali"));
            countries.Add(new Tuple<string,string>("MT", "Malta"));
            countries.Add(new Tuple<string,string>("MH", "Marshall Islands"));
            countries.Add(new Tuple<string,string>("MQ", "Martinique"));
            countries.Add(new Tuple<string,string>("MR", "Mauritania"));
            countries.Add(new Tuple<string,string>("MU", "Mauritius"));
            countries.Add(new Tuple<string,string>("YT", "Mayotte"));
            countries.Add(new Tuple<string,string>("MX", "Mexico"));
            countries.Add(new Tuple<string,string>("FM", "Micronesia"));
            countries.Add(new Tuple<string,string>("MD", "Moldova"));
            countries.Add(new Tuple<string,string>("MC", "Monaco"));
            countries.Add(new Tuple<string,string>("MN", "Mongolia"));
            countries.Add(new Tuple<string,string>("ME", "Montenegro"));
            countries.Add(new Tuple<string,string>("MS", "Montserrat"));
            countries.Add(new Tuple<string,string>("MA", "Morocco"));
            countries.Add(new Tuple<string,string>("MZ", "Mozambique"));
            countries.Add(new Tuple<string,string>("MM", "Myanmar"));
            countries.Add(new Tuple<string,string>("NA", "Namibia"));
            countries.Add(new Tuple<string,string>("NR", "Nauru"));
            countries.Add(new Tuple<string,string>("NP", "Nepal"));
            countries.Add(new Tuple<string,string>("NL", "Netherlands"));
            countries.Add(new Tuple<string,string>("AN", "Netherlands Antilles"));
            countries.Add(new Tuple<string,string>("NC", "New Caledonia"));
            countries.Add(new Tuple<string,string>("NZ", "New Zealand"));
            countries.Add(new Tuple<string,string>("NI", "Nicaragua"));
            countries.Add(new Tuple<string,string>("NE", "Niger"));
            countries.Add(new Tuple<string,string>("NG", "Nigeria"));
            countries.Add(new Tuple<string,string>("NU", "Niue"));
            countries.Add(new Tuple<string,string>("NF", "Norfolk Island"));
            countries.Add(new Tuple<string,string>("KP", "North Korea"));
            countries.Add(new Tuple<string,string>("MP", "Northern Mariana Islands"));
            countries.Add(new Tuple<string,string>("NO", "Norway"));
            countries.Add(new Tuple<string,string>("OM", "Oman"));
            countries.Add(new Tuple<string,string>("PK", "Pakistan"));
            countries.Add(new Tuple<string,string>("PW", "Palau"));
            countries.Add(new Tuple<string,string>("PS", "Palestinian territories"));
            countries.Add(new Tuple<string,string>("PA", "Panama"));
            countries.Add(new Tuple<string,string>("PG", "Papua New Guinea"));
            countries.Add(new Tuple<string,string>("PY", "Paraguay"));
            countries.Add(new Tuple<string,string>("PE", "Peru"));
            countries.Add(new Tuple<string,string>("PH", "Philippines"));
            countries.Add(new Tuple<string,string>("PN", "Pitcairn"));
            countries.Add(new Tuple<string,string>("PL", "Poland"));
            countries.Add(new Tuple<string,string>("PT", "Portugal"));
            countries.Add(new Tuple<string,string>("PR", "Puerto Rico"));
            countries.Add(new Tuple<string,string>("QA", "Qatar"));
            countries.Add(new Tuple<string,string>("RE", "Reunion"));
            countries.Add(new Tuple<string,string>("RO", "Romania"));
            countries.Add(new Tuple<string,string>("RU", "Russian Federation"));
            countries.Add(new Tuple<string,string>("RW", "Rwanda"));
            countries.Add(new Tuple<string,string>("MF", "Saint Martin"));
            countries.Add(new Tuple<string,string>("KN", "Saint Kitts And Nevis"));
            countries.Add(new Tuple<string,string>("LC", "Saint Lucia"));
            countries.Add(new Tuple<string,string>("VC", "Saint Vincent And The Grenadines"));
            countries.Add(new Tuple<string,string>("WS", "Samoa"));
            countries.Add(new Tuple<string,string>("SM", "San Marino"));
            countries.Add(new Tuple<string,string>("ST", "Sao Tome And Principe"));
            countries.Add(new Tuple<string,string>("SA", "Saudi Arabia"));
            countries.Add(new Tuple<string,string>("SN", "Senegal"));
            countries.Add(new Tuple<string,string>("RS", "Serbia"));
            countries.Add(new Tuple<string,string>("SC", "Seychelles"));
            countries.Add(new Tuple<string,string>("SL", "Sierra Leone"));
            countries.Add(new Tuple<string,string>("SG", "Singapore"));
            countries.Add(new Tuple<string,string>("SK", "Slovakia"));
            countries.Add(new Tuple<string,string>("SI", "Slovenia"));
            countries.Add(new Tuple<string,string>("SB", "Solomon Islands"));
            countries.Add(new Tuple<string,string>("SO", "Somalia"));
            countries.Add(new Tuple<string,string>("ZA", "South Africa"));
            countries.Add(new Tuple<string,string>("GS", "South Georgia/South Sandwich Islands"));
            countries.Add(new Tuple<string,string>("KR", "South Korea(Republic Of Korea)"));
            countries.Add(new Tuple<string,string>("SS", "South Sudan"));
            countries.Add(new Tuple<string,string>("ES", "Spain"));
            countries.Add(new Tuple<string,string>("LK", "Sri Lanka"));
            countries.Add(new Tuple<string,string>("SH", "St Helena"));
            countries.Add(new Tuple<string,string>("PM", "St Pierre and Miquelon"));
            countries.Add(new Tuple<string,string>("SD", "Sudan"));
            countries.Add(new Tuple<string,string>("SR", "Suriname"));
            countries.Add(new Tuple<string,string>("SJ", "Svalbard And Jan Mayen Islands"));
            countries.Add(new Tuple<string,string>("SZ", "Swaziland"));
            countries.Add(new Tuple<string,string>("SE", "Sweden"));
            countries.Add(new Tuple<string,string>("CH", "Switzerland"));
            countries.Add(new Tuple<string,string>("SY", "Syrian Arab Republic"));
            countries.Add(new Tuple<string,string>("TW", "Taiwan"));
            countries.Add(new Tuple<string,string>("TJ", "Tajikistan"));
            countries.Add(new Tuple<string,string>("TZ", "Tanzania"));
            countries.Add(new Tuple<string,string>("TH", "Thailand"));
            countries.Add(new Tuple<string,string>("TG", "Togo"));
            countries.Add(new Tuple<string,string>("TK", "Tokelau"));
            countries.Add(new Tuple<string,string>("TO", "Tonga"));
            countries.Add(new Tuple<string,string>("TT", "Trinidad And Tobago"));
            countries.Add(new Tuple<string,string>("TN", "Tunisia"));
            countries.Add(new Tuple<string,string>("TR", "Turkey"));
            countries.Add(new Tuple<string,string>("TM", "Turkmenistan"));
            countries.Add(new Tuple<string,string>("TC", "Turks And Caicos Islands"));
            countries.Add(new Tuple<string,string>("TV", "Tuvalu"));
            countries.Add(new Tuple<string,string>("UG", "Uganda"));
            countries.Add(new Tuple<string,string>("UA", "Ukraine"));
            countries.Add(new Tuple<string,string>("AE", "United Arab Emirates"));
            countries.Add(new Tuple<string,string>("UM", "U.S.Minor Outlying Islands"));
            countries.Add(new Tuple<string,string>("UY", "Uruguay"));
            countries.Add(new Tuple<string,string>("UZ", "Uzbekistan"));
            countries.Add(new Tuple<string,string>("VU", "Vanuatu"));
            countries.Add(new Tuple<string,string>("VA", "Vatican City State"));
            countries.Add(new Tuple<string,string>("VE", "Venezuela"));
            countries.Add(new Tuple<string,string>("VN", "Viet Nam"));
            countries.Add(new Tuple<string,string>("VG", "Virgin Islands(British)"));
            countries.Add(new Tuple<string,string>("VI", "Virgin Islands(U.S.)"));
            countries.Add(new Tuple<string,string>("WF", "Wallis And Futuna Islands"));
            countries.Add(new Tuple<string,string>("EH", "Western Sahara"));
            countries.Add(new Tuple<string,string>("YE", "Yemen"));
            countries.Add(new Tuple<string,string>("ZR", "Zaire"));
            countries.Add(new Tuple<string,string>("ZM", "Zambia"));
            countries.Add(new Tuple<string,string>("ZW", "Zimbabwe"));

            List<string> countryNames = new List<string>();
            foreach (var country in countries)
                countryNames.Add(country.Item2);

            this.LstCountries.ItemsSource = countryNames;

            // Months List Picker
            List<string> months = new List<string>();
            for (int i = 1; i <= 12; i++)
                months.Add(i.ToString("D2"));

            this.LstMonths.ItemsSource = months;
            this.LstMonths.SelectedIndex = DateTime.Now.Month - 1;

            // Years List Picker
            List<string> years = new List<string>();
            int currentYear = DateTime.Now.Year;
            for (int i = DateTime.Now.Year; i <= (currentYear + 50); i++)
                years.Add(i.ToString("D4"));

            this.LstYears.ItemsSource = years;
        }
                
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (App.AppInformation.IsStartupModeActivate)
            {
                if (!App.AppInformation.HasPinLockIntroduced && SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled))
                {
                    NavigateService.NavigateTo(typeof(PasswordPage), NavigationParameter.Normal, this.GetType());
                    return;
                }
                
                App.AppInformation.IsStartupModeActivate = false;
            }

            _paymentViewModel.Plan = (ProductBase)PhoneApplicationService.Current.State["SelectedPlan"];
            _paymentViewModel.ProductMonthly = (Product)PhoneApplicationService.Current.State["SelectedPlanMonthly"];
            _paymentViewModel.ProductAnnualy = (Product)PhoneApplicationService.Current.State["SelectedPlanAnnualy"];

            switch (_paymentViewModel.Plan.AccountType)
            {                
                case MAccountType.ACCOUNT_TYPE_LITE:
                    PageTitle.Text = String.Format(UiResources.SelectedPlan.ToUpper(), UiResources.AccountTypeLite.ToUpper());
                    break;
                case MAccountType.ACCOUNT_TYPE_PROI:
                    PageTitle.Text = String.Format(UiResources.SelectedPlan.ToUpper(), UiResources.AccountTypePro1.ToUpper());
                    break;
                case MAccountType.ACCOUNT_TYPE_PROII:
                    PageTitle.Text = String.Format(UiResources.SelectedPlan.ToUpper(), UiResources.AccountTypePro2.ToUpper());
                    break;
                case MAccountType.ACCOUNT_TYPE_PROIII:
                    PageTitle.Text = String.Format(UiResources.SelectedPlan.ToUpper(), UiResources.AccountTypePro3.ToUpper());
                    break;
            }
        }        

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // If a product has been selected, deselect it
            if (_paymentViewModel.CreditCardPaymentIsEnabled)
            {
                PageSubtitle.Text = UiResources.ChoosePaymentMethod;
                _paymentViewModel.ProductSelectionIsEnabled = false;
                _paymentViewModel.PaymentMethodSelectionIsEnabled = true;
            }
            else if (_paymentViewModel.PaymentMethodSelectionIsEnabled)
            {
                PageSubtitle.Text = UiResources.ChooseRenewalPeriod;
                _paymentViewModel.SelectedProduct = null;
                _paymentViewModel.ProductSelectionIsEnabled = true;
                _paymentViewModel.PaymentMethodSelectionIsEnabled = false;
            }
            // If not, come back to the update account pivot
            else if (_paymentViewModel.ProductSelectionIsEnabled)
            {
                ((PhoneApplicationFrame)Application.Current.RootVisual).RemoveBackEntry();
                NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal, new Dictionary<string, string> { { "Pivot", "1" } });                
            }

            _paymentViewModel.CreditCardPaymentIsEnabled = false;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                        
            e.Cancel = true;
        }

        private void OnMonthlyTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PageSubtitle.Text = UiResources.ChoosePaymentMethod;

            _paymentViewModel.SelectedProduct = _paymentViewModel.ProductMonthly;
            LstPaymentMethods.ItemsSource = _paymentViewModel.SelectedProduct.PaymentMethods;

            _paymentViewModel.ProductSelectionIsEnabled = false;
            _paymentViewModel.PaymentMethodSelectionIsEnabled = true;
            _paymentViewModel.CreditCardPaymentIsEnabled = false;
        }

        private void OnAnnualyTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PageSubtitle.Text = UiResources.ChoosePaymentMethod;

            _paymentViewModel.SelectedProduct = _paymentViewModel.ProductAnnualy;
            LstPaymentMethods.ItemsSource = _paymentViewModel.SelectedProduct.PaymentMethods;

            _paymentViewModel.ProductSelectionIsEnabled = false;
            _paymentViewModel.PaymentMethodSelectionIsEnabled = true;
            _paymentViewModel.CreditCardPaymentIsEnabled = false;
        }

        private void OnAcceptClick(object sender, EventArgs e)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            _paymentViewModel.DoPayment();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            PageSubtitle.Text = UiResources.ChooseRenewalPeriod;

            _paymentViewModel.ProductSelectionIsEnabled = true;
            _paymentViewModel.PaymentMethodSelectionIsEnabled = false;
            _paymentViewModel.CreditCardPaymentIsEnabled = false;

            NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal, new Dictionary<string, string> { { "Pivot", "1" } });
        }

        private void LstCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _paymentViewModel.BillingDetails.CountryCode = countries.ElementAt(this.LstCountries.SelectedIndex).Item1;
        }

        private void OnSelectedPaymentMethod(object sender, Telerik.Windows.Controls.ListBoxItemTapEventArgs e)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            switch(((PaymentMethod)LstPaymentMethods.SelectedItem).PaymentMethodType)
            {
                case MPaymentMethod.PAYMENT_METHOD_CENTILI:
                case MPaymentMethod.PAYMENT_METHOD_FORTUMO:
                    App.MegaSdk.getPaymentId(_paymentViewModel.SelectedProduct.Handle, new GetPaymentUrlRequestListener(((PaymentMethod)LstPaymentMethods.SelectedItem).PaymentMethodType));
                    break;

                case MPaymentMethod.PAYMENT_METHOD_CREDIT_CARD:
                    PageSubtitle.Text = UiResources.EnterPaymentDetails;
                    _paymentViewModel.ProductSelectionIsEnabled = false;
                    _paymentViewModel.PaymentMethodSelectionIsEnabled = false;
                    _paymentViewModel.CreditCardPaymentIsEnabled = true;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
                    break;
            }
        }
    }
}