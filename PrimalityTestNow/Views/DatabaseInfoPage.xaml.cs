using System;
using PrimalityTestNow.Helpers;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace PrimalityTestNow.Views
{
    public sealed partial class DatabaseInfoPage : Page
    {
        private readonly MainPage mainPage;

        public DatabaseInfoPage()
        {
            InitializeComponent();

            SizeChanged += DatabaseInfoPage_SizeChanged;

            Loaded += DatabaseInfoPage_Loaded;

            mainPage = MainPage.CurrentMainPage;
        }

        private void DatabaseInfoPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetPageContentStackPanelWidth();
        }

        private void DatabaseInfoPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetPageContentStackPanelWidth();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            #region PrimalityTestNow.db
            CountOfPrimenumbersTextBlock.Text = $"Count of primenumbers is {await App.SqlPrimeRepo.GetPrimesCountAsync()}.";
            BiggestPrimenumberTextBlock.Text = $"Biggest primenumber is {await App.SqlPrimeRepo.GetBiggestPrimenumberAsStringAsync()}.";

            StorageFile filePrimalityTestNow = await StorageFile.GetFileFromPathAsync(App.DatabasePath);
            if (filePrimalityTestNow != null)
            {
                BasicProperties basicPropertiesPrimalityTestNow = await filePrimalityTestNow.GetBasicPropertiesAsync();
                PrimalityTestNowFileSize.Text = $"File PrimalityTestNow.db's size is {HelpToFileSize.ToFileSize(basicPropertiesPrimalityTestNow.Size)}.";
                PrimalityTestNowFilePath.Text = @filePrimalityTestNow.Path;
            }
            else
            {
                PrimalityTestNowFileSize.Text = "File PrimalityTestNow.db is missing.";
            }
            #endregion PrimalityTestNow.db
            // code here
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // code here
            // code here
        }

        private void SetPageContentStackPanelWidth()
        {
            PageContentStackPanel.Width = ActualWidth -
                PageContentScrollViewer.Margin.Left -
                PageContentScrollViewer.Padding.Right;
        }

        #region MenuAppBarButton
        private void HomeAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            mainPage.GoToHomePage();
        }
        #endregion MenuAppBarButton

        private async void DeleteDatabaseTableAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // ask for permission to delete database table
            bool bryt = false;
            MessageDialog messageDialog = new MessageDialog("Do you want to delete database table with primenumbers?\nPrimenumbers will be deleted.", "Delete database table and all records in it.");

            messageDialog.Commands.Add(new UICommand("Delete", (command) =>
            {
            }));
            messageDialog.Commands.Add(new UICommand("Cancel", (command) =>
            {
                bryt = true;
            }));
            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 1;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            await messageDialog.ShowAsync();

            if (bryt)
            {
                mainPage.NotifyUser($"Operation cancelled.", NotifyType.ErrorMessage);

                return;
            }

            DeleteDatabaseTableProgressRing.IsActive = true;
            DeleteDatabaseTableProgressRing.IsEnabled = true;

            #region delete database table
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await App.SqlPrimeRepo.DeleteDatabaseTableAsync();

                    await App.SqlPrimeRepo.CreateTableAsync();

                    CountOfPrimenumbersTextBlock.Text = $"Count of primenumbers is {await App.SqlPrimeRepo.GetPrimesCountAsync()}.";
                    BiggestPrimenumberTextBlock.Text = $"Biggest primenumber is {await App.SqlPrimeRepo.GetBiggestPrimenumberAsStringAsync()}.";

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
                    DeleteDatabaseTableProgressRing.IsActive = false;
                    DeleteDatabaseTableProgressRing.IsEnabled = false;
                    mainPage.NotifyUser($"Database table with primenumbers was deleted.", NotifyType.StatusMessage);
                });
            }
            #endregion delete database table     
        }
    }
}
