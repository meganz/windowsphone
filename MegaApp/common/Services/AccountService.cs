using System;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Resources;

namespace MegaApp.Services
{
    public static class AccountService
    {
        public static event EventHandler GetAccountDetailsFinish;

        private static AccountDetailsViewModel _accountDetails;
        public static AccountDetailsViewModel AccountDetails
        {
            get
            {
                if (_accountDetails != null) return _accountDetails;
                _accountDetails = new AccountDetailsViewModel() { UserEmail = SdkService.MegaSdk.getMyEmail() };
                return _accountDetails;
            }
        }

        private static UpgradeAccountViewModel _upgradeAccount;
        public static UpgradeAccountViewModel UpgradeAccount
        {
            get
            {
                if (_upgradeAccount != null) return _upgradeAccount;
                _upgradeAccount = new UpgradeAccountViewModel();
                return _upgradeAccount;
            }
        }

        /// <summary>
        /// Check if should show the password reminder dialog and show it in that case
        /// </summary>
        public static async Task<bool> ShouldShowPasswordReminderDialogAsync()
        {
            var passwordReminderDialogListener = new ShouldShowPasswordReminderDialogRequestListenerAsync();
            return await passwordReminderDialogListener.ExecuteAsync(() =>
                SdkService.MegaSdk.shouldShowPasswordReminderDialog(false, passwordReminderDialogListener));
        }

        public static void ClearAccountDetails()
        {
            _accountDetails = null;
        }

        public static void GetAccountDetails()
        {
            SdkService.MegaSdk.getAccountDetails(new GetAccountDetailsRequestListener(GetAccountDetailsFinish));
        }

        /// <summary>
        /// Gets all pricing details info.
        /// </summary>
        /// <param name="pricingDetails">Details about pricing plans</param>
        public static void GetPricingDetails(MPricing pricingDetails)
        {
            Deployment.Current.Dispatcher.BeginInvoke(async () =>
            {
                AccountService.UpgradeAccount.Products.Clear();
                AccountService.UpgradeAccount.Plans.Clear();

                if (App.AppInformation.IsNewlyActivatedAccount)
                {
                    var freePlan = new ProductBase
                    {
                        AccountType = MAccountType.ACCOUNT_TYPE_FREE,
                        Name = AppResources.AccountTypeFree,
                        ProductColor = Color.FromArgb(255, 19, 224, 60),
                        ProductPathData = VisualResources.CrestFreeAccountPathData
                    };

                    AccountService.UpgradeAccount.Plans.Add(freePlan);
                    App.AppInformation.IsNewlyActivatedAccount = false;
                }

                int numberOfProducts = pricingDetails.getNumProducts();

                for (int i = 0; i < numberOfProducts; i++)
                {
                    var accountType = (MAccountType)Enum.Parse(typeof(MAccountType),
                        pricingDetails.getProLevel(i).ToString());

                    var product = new Product
                    {
                        AccountType = accountType,
                        Amount = pricingDetails.getAmount(i),
                        FormattedPrice = string.Format("{0:N} {1}", (double)pricingDetails.getAmount(i) / 100, AccountService.GetCurrencySymbol(pricingDetails.getCurrency(i))),
                        Currency = GetCurrencySymbol(pricingDetails.getCurrency(i)),
                        GbStorage = pricingDetails.getGBStorage(i),
                        GbTransfer = pricingDetails.getGBTransfer(i),
                        Months = pricingDetails.getMonths(i),
                        Handle = pricingDetails.getHandle(i)
                    };

                    // Try get the local pricing details from the store
                    var storeProduct = await LicenseService.GetProductAsync(product.MicrosoftStoreId);
                    if (storeProduct != null)
                    {
                        product.FormattedPrice = storeProduct.FormattedPrice;
                        product.Currency = AccountService.GetCurrencySymbol(
                            AccountService.GetCurrencyFromFormattedPrice(storeProduct.FormattedPrice));
                    }

                    switch (accountType)
                    {
                        case MAccountType.ACCOUNT_TYPE_FREE:
                            product.Name = AppResources.AccountTypeFree;
                            product.ProductPathData = VisualResources.CrestFreeAccountPathData;
                            break;

                        case MAccountType.ACCOUNT_TYPE_LITE:
                            product.Name = AppResources.AccountTypeLite;
                            product.ProductColor = Color.FromArgb(255, 255, 165, 0);
                            product.ProductPathData = VisualResources.CrestLiteAccountPathData;

                            // If Centili payment method is active, and product is LITE monthly include it into the product
                            if (AccountService.UpgradeAccount.CentiliPaymentMethodAvailable && product.Months == 1)
                            {
                                var centiliPaymentMethod = new PaymentMethod
                                {
                                    PaymentMethodType = MPaymentMethod.PAYMENT_METHOD_CENTILI,
                                    Name = String.Format("Centili - " + UiResources.PhoneBill + " (" + UiResources.Punctual.ToLower() + ")"),
                                    PaymentMethodPathData = VisualResources.PhoneBillingPathData
                                };
                                product.PaymentMethods.Add(centiliPaymentMethod);
                            }

                            // If Fortumo payment method is active, and product is LITE monthly include it into the product
                            if (AccountService.UpgradeAccount.FortumoPaymentMethodAvailable && product.Months == 1)
                            {
                                var fortumoPaymentMethod = new PaymentMethod
                                {
                                    PaymentMethodType = MPaymentMethod.PAYMENT_METHOD_FORTUMO,
                                    Name = String.Format("Fortumo - " + UiResources.PhoneBill + " (" + UiResources.Punctual.ToLower() + ")"),
                                    PaymentMethodPathData = VisualResources.PhoneBillingPathData
                                };
                                product.PaymentMethods.Add(fortumoPaymentMethod);
                            }
                            break;

                        case MAccountType.ACCOUNT_TYPE_PROI:
                            product.Name = AppResources.AccountTypePro1;
                            product.ProductColor = Color.FromArgb(255, 217, 0, 7);
                            product.ProductPathData = VisualResources.CrestProIAccountPathData;
                            break;

                        case MAccountType.ACCOUNT_TYPE_PROII:
                            product.Name = AppResources.AccountTypePro2;
                            product.ProductColor = Color.FromArgb(255, 217, 0, 7);
                            product.ProductPathData = VisualResources.CrestProIIAccountPathData;
                            break;

                        case MAccountType.ACCOUNT_TYPE_PROIII:
                            product.Name = AppResources.AccountTypePro3;
                            product.ProductColor = Color.FromArgb(255, 217, 0, 7);
                            product.ProductPathData = VisualResources.CrestProIIIAccountPathData;
                            break;

                        default:
                            product.ProductColor = Color.FromArgb(255, 217, 0, 7);
                            break;
                    }

                    // If CC payment method is active, include it into the product
                    if (AccountService.UpgradeAccount.CreditCardPaymentMethodAvailable)
                    {
                        var creditCardPaymentMethod = new PaymentMethod
                        {
                            PaymentMethodType = MPaymentMethod.PAYMENT_METHOD_CREDIT_CARD,
                            Name = String.Format(UiResources.CreditCard + " (" + UiResources.Recurring.ToLower() + ")"),
                            PaymentMethodPathData = VisualResources.CreditCardPathData
                        };
                        product.PaymentMethods.Add(creditCardPaymentMethod);
                    }

                    // If in-app payment method is active, include it into the product
                    if (AccountService.UpgradeAccount.InAppPaymentMethodAvailable)
                    {
                        var inAppPaymentMethod = new PaymentMethod
                        {
                            PaymentMethodType = MPaymentMethod.PAYMENT_METHOD_WINDOWS_STORE,
                            Name = String.Format(UiResources.UI_InAppPurchase + " - " + UiResources.PhoneBill + " (" + UiResources.Punctual.ToLower() + ")"),
                            PaymentMethodPathData = VisualResources.PhoneBillingPathData
                        };
                        product.PaymentMethods.Add(inAppPaymentMethod);
                    }

                    AccountService.UpgradeAccount.Products.Add(product);

#if DEBUG
                    // Fill the Mocking IAP product listing with actual MEGA product id's
                    LicenseService.AddProductToMockIap(product);
#endif

                    // Plans show only the information off the annualy plans
                    if (pricingDetails.getMonths(i) == 12)
                    {
                        var plan = new ProductBase
                        {
                            AccountType = accountType,
                            Name = product.Name,
                            Amount = product.Amount,
                            FormattedPrice = product.FormattedPrice,
                            Currency = product.Currency,
                            GbStorage = product.GbStorage,
                            GbTransfer = product.GbTransfer / 12,
                            ProductPathData = product.ProductPathData,
                            ProductColor = product.ProductColor,
                            IsNewOffer = product.IsNewOffer
                        };

                        AccountService.UpgradeAccount.Plans.Add(plan);

                        // Check if the user has a product/plan already purchased and fill the structure to show it
                        if (accountType == AccountService.AccountDetails.AccountType && pricingDetails.getMonths(i) == 12)
                        {
                            AccountService.UpgradeAccount.ProductPurchased = product;
                            AccountService.UpgradeAccount.ProductPurchased.GbTransfer = pricingDetails.getGBTransfer(i) / 12;
                            AccountService.UpgradeAccount.ProductPurchased.IsNewOffer = false;
                            AccountService.UpgradeAccount.ProductPurchased.Purchased = true;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Gets the currency symbol corresponding to a currency ISO code
        /// </summary>
        /// <param name="currencyCode">Currency ISO code</param>
        /// <returns>Currency symbol associated with the curreny ISO code.</returns>
        private static string GetCurrencySymbol(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode)) return string.Empty;

            switch (currencyCode)
            {
                case "EUR": return "€";
                case "USD": return "$";
                default: return currencyCode;
            }
        }

        /// <summary>
        /// Gets the currency (ISO code or symbol) from a formatted price string
        /// </summary>
        /// <param name="formattedPrice">String with the price and the currency.</param>
        /// <returns>Currency ISO code or symbol of the formatted price string</returns>
        private static string GetCurrencyFromFormattedPrice(string formattedPrice)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(formattedPrice)) return string.Empty;

                char[] charsToTrim = { '0','1','2','3','4','5','6','7','8','9',' ','.',',' };
                return formattedPrice.Trim(charsToTrim);
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR,
                    string.Format("Failure getting currency from {0}", formattedPrice), e);
                return "n/a";
            }
        }
    }
}
