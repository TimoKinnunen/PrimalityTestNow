using PrimalityTestNow.Models;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace PrimalityTestNow.Views
{
    public sealed partial class PrimalityTestPage : Page
    {
        CancellationTokenSource calculateCancellationTokenSource { get; set; }

        CancellationToken calculateCancellationToken { get; set; }

        readonly MainPage mainPage;

        BigInteger numberToTest { get; set; }

        public PrimalityTestPage()
        {
            InitializeComponent();

            mainPage = MainPage.CurrentMainPage;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            // code here
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // code here
            if (calculateCancellationToken.CanBeCanceled)
            {
                calculateCancellationTokenSource.Cancel();
            }
            if (calculateCancellationTokenSource != null)
            {
                calculateCancellationTokenSource.Dispose();
            }
            // code here
        }

        #region MenuAppBarButton
        private void HomeAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            mainPage.GoToHomePage();
        }
        #endregion MenuAppBarButton

        private void NumberToTestTextChanged(object sender, TextChangedEventArgs e)
        {
            string trimmedPrimenumber = NumberToTestTextBox.Text.Trim();
            if (BigInteger.TryParse(trimmedPrimenumber, out BigInteger primeNumberCandidateAsBigInteger))
            {
                mainPage.NotifyUser($"{string.Empty}", NotifyType.StatusMessage);
                numberToTest = primeNumberCandidateAsBigInteger;
            }
            else
            {
                numberToTest = 0;
                mainPage.NotifyUser($"Enter number to test, please. Try again.", NotifyType.ErrorMessage);
            }
        }

        private async void CalculateDataButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CalculateDataButton.Content.ToString() == "Cancel")
            {
                if (calculateCancellationToken.CanBeCanceled)
                {
                    calculateCancellationTokenSource.Cancel();
                }
                CalculateDataButton.Content = "Primality test";
                return;
            }

            if (CalculateDataButton.Content.ToString() == "Primality test")
            {
                if (numberToTest > 0)
                {
                    CalculateDataButton.Content = "Cancel";
                    mainPage.NotifyUser($"Please wait or cancel.", NotifyType.StatusMessage);

                    StartCalculateDataProgressRing();

                    try
                    {
                        calculateCancellationTokenSource = new CancellationTokenSource();
                        calculateCancellationToken = calculateCancellationTokenSource.Token;

                        //add cancellationToken to this task
                        await Task.Run(async () => await CalculateDataAsync(), calculateCancellationToken).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            mainPage.NotifyUser($"Operation was cancelled.", NotifyType.ErrorMessage);
                        });
                    }
                    catch (OutOfMemoryException ex)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            mainPage.NotifyUser($"OutOfMemoryException: {ex.Message}", NotifyType.ErrorMessage);
                        });
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            mainPage.NotifyUser($"{ex.Message}", NotifyType.ErrorMessage);
                        });
                    }
                    finally
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            CalculateDataButton.Content = "Primality test";

                            StopCalculateDataProgressRing();
                        });
                    }
                }
                else
                {
                    mainPage.NotifyUser($"Enter number to test, please. Try again.", NotifyType.ErrorMessage);
                }
            }
        }

        private async Task CalculateDataAsync()
        {
            BigInteger primenumberCandidate = numberToTest;

            if (primenumberCandidate.IsEven)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    IsPrimeTextBlock.Text = $"Number is even. Try again.";
                    mainPage.NotifyUser($"Number is even. Try again.", NotifyType.StatusMessage);
                });
                return;
            }

            bool isPrime = true;

            try
            {
                for (BigInteger divisor = 3; divisor <= (BigInteger)Math.Sqrt((double)primenumberCandidate); divisor += 2)
                {
                    if (calculateCancellationTokenSource.IsCancellationRequested)
                    {
                        isPrime = false;
                        break;
                    }
                    if (primenumberCandidate % divisor == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                for (BigInteger divisor = 3; divisor <= (primenumberCandidate + 1) / 2; divisor += 2)
                {
                    if (calculateCancellationTokenSource.IsCancellationRequested)
                    {
                        isPrime = false;
                        break;
                    }
                    if (primenumberCandidate % divisor == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
            }

            //var tt = primenumberCandidate % 3;

            if (isPrime)
            {
                Prime prime = await App.SqlPrimeRepo.SearchPrimeAsync(primenumberCandidate.ToString());
                if (prime == null)
                {
                    int newPrimeId = await App.SqlPrimeRepo.UpsertPrimeAsync(new Prime { Primenumber = primenumberCandidate.ToString() });
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        IsPrimeTextBlock.Text = $"Number is primenumber and was added to database table.";
                        mainPage.NotifyUser($"Added number as primenumber to database table.", NotifyType.StatusMessage);
                    });
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        IsPrimeTextBlock.Text = $"Number is primenumber.";
                        mainPage.NotifyUser($"Number is primenumber.", NotifyType.StatusMessage);
                    });
                }
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (calculateCancellationTokenSource.IsCancellationRequested)
                    {
                        IsPrimeTextBlock.Text = string.Empty;
                        mainPage.NotifyUser("Operation was cancelled.", NotifyType.ErrorMessage);
                    }
                    else
                    {
                        IsPrimeTextBlock.Text = $"Number is not primenumber.";
                        mainPage.NotifyUser($"Number is not primenumber.", NotifyType.ErrorMessage);
                    }
                });
            }
        }

        private void StartCalculateDataProgressRing()
        {
            CalculateDataProgressRing.IsActive = true;
            CalculateDataProgressRing.Visibility = Visibility.Visible;
        }

        private void StopCalculateDataProgressRing()
        {
            CalculateDataProgressRing.IsActive = false;
            CalculateDataProgressRing.Visibility = Visibility.Collapsed;
        }
    }
}