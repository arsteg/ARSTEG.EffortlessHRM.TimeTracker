using System.Collections.ObjectModel;
using Moq;
using TimeTracker.Models;
using TimeTracker.Services.Interfaces;
using TimeTracker.ViewModels;
using Xunit;

namespace Tracker.Tests.ViewModels
{
    public class ProductivityAppsSettingsViewModelTests
    {
        private readonly Mock<IRestService> _mockRestService;

        public ProductivityAppsSettingsViewModelTests()
        {
            _mockRestService = new Mock<IRestService>();
            SetupGlobalSetting();
            SetupDefaultMocks();
        }

        private void SetupGlobalSetting()
        {
            GlobalSetting.Instance.LoginResult = new LoginResult
            {
                token = "test-token",
                data = new LoginresultData
                {
                    user = new Login { id = "test-user-id", email = "test@example.com" }
                }
            };
        }

        private void SetupDefaultMocks()
        {
            _mockRestService
                .Setup(x => x.GetProductivityApps(It.IsAny<string>()))
                .ReturnsAsync(new ProductivityAppResult
                {
                    data = new List<ProductivityModel>()
                });
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRestService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductivityAppsSettingsViewModel(null!));
        }

        [Fact]
        public void Constructor_InitializesProductivityAppsCollection()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            Assert.NotNull(viewModel.ProductivityApps);
        }

        [Fact]
        public void Constructor_InitializesRunningProcessesCollection()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            Assert.NotNull(viewModel.RunningProcesses);
        }

        [Fact]
        public void Constructor_InitializesCloseCommand()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            Assert.NotNull(viewModel.CloseCommand);
        }

        [Fact]
        public void Constructor_InitializesDeleteApplicationCommand()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            Assert.NotNull(viewModel.DeleteApplication);
        }

        [Fact]
        public void Constructor_InitializesReloadProcessesCommand()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            Assert.NotNull(viewModel.ReloadProcessesCommand);
        }

        [Fact]
        public void Constructor_InitializesAddApplicationCommand()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            Assert.NotNull(viewModel.AddApplication);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void ProductivityApps_SetValue_RaisesPropertyChanged()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ProductivityAppsSettingsViewModel.ProductivityApps))
                    propertyChangedRaised = true;
            };

            viewModel.ProductivityApps = new ObservableCollection<ProductivityModel>();

            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void RunningProcesses_SetValue_RaisesPropertyChanged()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ProductivityAppsSettingsViewModel.RunningProcesses))
                    propertyChangedRaised = true;
            };

            viewModel.RunningProcesses = new ObservableCollection<ProductivityModel>();

            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void ProductivityApps_GetterSetter_WorkCorrectly()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            var newCollection = new ObservableCollection<ProductivityModel>
            {
                new ProductivityModel { key = "test" }
            };

            viewModel.ProductivityApps = newCollection;

            Assert.Same(newCollection, viewModel.ProductivityApps);
            Assert.Single(viewModel.ProductivityApps);
        }

        #endregion

        #region Command Tests

        [Fact]
        public void DeleteApplication_CanExecute_ReturnsTrue()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            Assert.True(viewModel.DeleteApplication.CanExecute("app-id"));
        }

        [Fact]
        public void AddApplication_CanExecute_ReturnsTrue()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            Assert.True(viewModel.AddApplication.CanExecute("app-key"));
        }

        [Fact]
        public void ReloadProcessesCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            Assert.True(viewModel.ReloadProcessesCommand.CanExecute(null));
        }

        [Fact]
        public void CloseCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            Assert.True(viewModel.CloseCommand.CanExecute(null));
        }

        #endregion

        #region GetRunningApplications Tests

        [Fact]
        public void GetRunningApplications_ReturnsApplicationList()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);

            var result = viewModel.GetRunningApplications();

            Assert.NotNull(result);
            Assert.IsType<List<ProductivityModel>>(result);
        }

        #endregion

        #region Collection Tests

        [Fact]
        public void ProductivityApps_IsObservableCollection()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            Assert.IsType<ObservableCollection<ProductivityModel>>(viewModel.ProductivityApps);
        }

        [Fact]
        public void RunningProcesses_IsObservableCollection()
        {
            var viewModel = new ProductivityAppsSettingsViewModel(_mockRestService.Object);
            Assert.IsType<ObservableCollection<ProductivityModel>>(viewModel.RunningProcesses);
        }

        #endregion
    }
}
