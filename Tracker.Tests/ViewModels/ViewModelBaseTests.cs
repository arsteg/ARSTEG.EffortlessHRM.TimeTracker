using System.ComponentModel;
using TimeTracker.ViewModels;
using Xunit;

namespace Tracker.Tests.ViewModels
{
    // Test implementation of ViewModelBase for testing purposes
    public class TestableViewModel : ViewModelBase
    {
        private string _testProperty;
        public string TestProperty
        {
            get => _testProperty;
            set
            {
                _testProperty = value;
                OnPropertyChanged();
            }
        }

        private int _numericProperty;
        public int NumericProperty
        {
            get => _numericProperty;
            set
            {
                _numericProperty = value;
                OnPropertyChanged(nameof(NumericProperty));
            }
        }

        // Expose OnPropertyChanged for testing
        public void TriggerPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }

    public class ViewModelBaseTests
    {
        [Fact]
        public void OnPropertyChanged_WithCallerMemberName_RaisesEventWithCorrectPropertyName()
        {
            // Arrange
            var viewModel = new TestableViewModel();
            var raisedPropertyName = string.Empty;
            viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

            // Act
            viewModel.TestProperty = "new value";

            // Assert
            Assert.Equal(nameof(TestableViewModel.TestProperty), raisedPropertyName);
        }

        [Fact]
        public void OnPropertyChanged_WithExplicitPropertyName_RaisesEventWithCorrectPropertyName()
        {
            // Arrange
            var viewModel = new TestableViewModel();
            var raisedPropertyName = string.Empty;
            viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

            // Act
            viewModel.NumericProperty = 42;

            // Assert
            Assert.Equal(nameof(TestableViewModel.NumericProperty), raisedPropertyName);
        }

        [Fact]
        public void OnPropertyChanged_WithNoSubscribers_DoesNotThrow()
        {
            // Arrange
            var viewModel = new TestableViewModel();

            // Act & Assert - Should not throw
            var exception = Record.Exception(() => viewModel.TestProperty = "value");
            Assert.Null(exception);
        }

        [Fact]
        public void PropertyChanged_MultipleSubscribers_AllAreNotified()
        {
            // Arrange
            var viewModel = new TestableViewModel();
            var subscriber1Notified = false;
            var subscriber2Notified = false;

            viewModel.PropertyChanged += (sender, args) => subscriber1Notified = true;
            viewModel.PropertyChanged += (sender, args) => subscriber2Notified = true;

            // Act
            viewModel.TestProperty = "test";

            // Assert
            Assert.True(subscriber1Notified);
            Assert.True(subscriber2Notified);
        }

        [Fact]
        public void PropertyChanged_Sender_IsTheViewModel()
        {
            // Arrange
            var viewModel = new TestableViewModel();
            object? capturedSender = null;
            viewModel.PropertyChanged += (sender, args) => capturedSender = sender;

            // Act
            viewModel.TestProperty = "value";

            // Assert
            Assert.Same(viewModel, capturedSender);
        }

        [Fact]
        public void PropertyChanged_Args_ContainsCorrectPropertyName()
        {
            // Arrange
            var viewModel = new TestableViewModel();
            PropertyChangedEventArgs? capturedArgs = null;
            viewModel.PropertyChanged += (sender, args) => capturedArgs = args;

            // Act
            viewModel.NumericProperty = 100;

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Equal(nameof(TestableViewModel.NumericProperty), capturedArgs.PropertyName);
        }

        [Fact]
        public void PropertyChanged_SettingSameValue_StillRaisesEvent()
        {
            // Arrange
            var viewModel = new TestableViewModel();
            viewModel.TestProperty = "initial";

            var eventCount = 0;
            viewModel.PropertyChanged += (sender, args) => eventCount++;

            // Act
            viewModel.TestProperty = "initial"; // Same value

            // Assert
            Assert.Equal(1, eventCount);
        }

        [Fact]
        public void OnPropertyChanged_WithCustomPropertyName_RaisesCorrectEvent()
        {
            // Arrange
            var viewModel = new TestableViewModel();
            var raisedPropertyName = string.Empty;
            viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

            // Act
            viewModel.TriggerPropertyChanged("CustomProperty");

            // Assert
            Assert.Equal("CustomProperty", raisedPropertyName);
        }

        [Fact]
        public void OnPropertyChanged_WithNullPropertyName_RaisesEventWithNull()
        {
            // Arrange
            var viewModel = new TestableViewModel();
            string? raisedPropertyName = "not-null";
            viewModel.PropertyChanged += (sender, args) => raisedPropertyName = args.PropertyName;

            // Act
            viewModel.TriggerPropertyChanged(null!);

            // Assert
            Assert.Null(raisedPropertyName);
        }

        [Fact]
        public void ImplementsINotifyPropertyChanged()
        {
            // Arrange
            var viewModel = new TestableViewModel();

            // Assert
            Assert.IsAssignableFrom<INotifyPropertyChanged>(viewModel);
        }
    }
}
