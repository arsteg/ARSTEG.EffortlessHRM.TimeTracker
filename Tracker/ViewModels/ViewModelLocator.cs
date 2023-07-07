using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Text;
using GalaSoft.MvvmLight.Views;

namespace TimeTracker.ViewModels
{
    public class ViewModelLocator
    {         
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);            
            SimpleIoc.Default.Register<TimeTrackerViewModel>();
            SimpleIoc.Default.Register<LoginViewModel>();
            SimpleIoc.Default.Register<ProductivityAppsSettingsViewModel>();
        }

        public TimeTrackerViewModel TimeTracker
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TimeTrackerViewModel>();
            }
        }

        public LoginViewModel Login
        {
            get
            {
                return ServiceLocator.Current.GetInstance<LoginViewModel>();
            }
        }

        public ProductivityAppsSettingsViewModel ProductivityAppsSettings
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProductivityAppsSettingsViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }        
    }
}
