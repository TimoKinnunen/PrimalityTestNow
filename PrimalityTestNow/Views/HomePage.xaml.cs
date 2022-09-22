using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using PrimalityTestNow.Helpers;
using PrimalityTestNow.Models;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace PrimalityTestNow.Views
{
    public sealed partial class HomePage : Page
    {
        private readonly MainPage mainPage;

        private readonly ObservableCollection<Prime> primesObservableCollection;

        private List<Prime> primesList;

        public HomePage()
        {
            InitializeComponent();

            Loaded += HomePage_Loaded;

            primesObservableCollection = new ObservableCollection<Prime>();

            mainPage = MainPage.CurrentMainPage;
        }

        private async void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                primesList = await App.SqlPrimeRepo.GetAllPrimesAsync().ConfigureAwait(false);

                foreach (Prime prime in primesList)
                {
                    primesObservableCollection.Add(prime);
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PrimesListView.ItemsSource = primesObservableCollection;
                });
            }
            catch (OutOfMemoryException ex)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mainPage.NotifyUser($"OutOfMemoryException: {ex.Message}", NotifyType.ErrorMessage);
                });
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            CountOfPrimenumbersTextBlock.Text = $"{await App.SqlPrimeRepo.GetPrimesCountAsync()}";
            BiggestPrimenumberTextBlock.Text = $"{await App.SqlPrimeRepo.GetBiggestPrimenumberAsStringAsync()}";
            // code here
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        private void SearchPrimenumberTextChanged(object sender, TextChangedEventArgs e)
        {
            string trimmedPrimenumber = SearchPrimenumberTextBox.Text.Trim();
            if (BigInteger.TryParse(trimmedPrimenumber, out BigInteger primeNumberCandidateAsBigInteger))
            {
                Prime prime = primesObservableCollection.FirstOrDefault(p => BigInteger.Parse(p.Primenumber) >= primeNumberCandidateAsBigInteger);

                if (prime != null)
                {
                    if (prime.Primenumber == trimmedPrimenumber)
                    {
                        PrimesListView.SelectedItem = prime;
                    }
                    PrimesListView.ScrollIntoView(prime, ScrollIntoViewAlignment.Leading);
                }
            }
        }

        private async void ExportDataAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile file = await HelpFileSavePicker.GetStorageFileForJsonAsync("PrimalityTestNow");
            if (file != null)
            {
                ExportDataAppBarButton.IsEnabled = false;
                ExportDataProgressRing.IsEnabled = true;
                ExportDataProgressRing.IsActive = true;
                ExportDataProgressRing.Visibility = Visibility.Visible;

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mainPage.NotifyUser($"Exporting data. Please wait.", NotifyType.StatusMessage);
                });

                List<Prime> primes = await App.SqlPrimeRepo.GetAllPrimesAsync().ConfigureAwait(false);

                string jsonData = await Task.Run(() => JsonConvert.SerializeObject(primes)).ConfigureAwait(false);

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);
                    // write to file
                    await FileIO.WriteTextAsync(file, jsonData);
                    // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                    // Completing updates may require Windows to ask for user input.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == FileUpdateStatus.Complete)
                    {
                        mainPage.NotifyUser($"File {file.Name} was saved.", NotifyType.StatusMessage);
                    }
                    else
                    {
                        mainPage.NotifyUser($"File {file.Name} couldn't be saved.", NotifyType.StatusMessage);
                    }

                    ExportDataAppBarButton.IsEnabled = true;
                    ExportDataProgressRing.IsEnabled = false;
                    ExportDataProgressRing.IsActive = false;
                    ExportDataProgressRing.Visibility = Visibility.Collapsed;

                });
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mainPage.NotifyUser($"Operation cancelled.", NotifyType.StatusMessage);
                });
            }
        }
    }
}
