/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:DapperContext.WpfExample"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using Dapper;
using DapperContext.WpfExample.Repository;
using DapperContext.WpfExample.Repository.Sql;
using DapperContext.WpfExample.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace DapperContext.WpfExample.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //Repositories
            SimpleIoc.Default.Register<ICategoryRepository, CategoryRepository>();
            SimpleIoc.Default.Register<ICustomerRepository, CustomerRepository>();
            SimpleIoc.Default.Register<IEmployeeRepository, EmployeeRepository>();
            SimpleIoc.Default.Register<IProductRepository, ProductRepository>();
            SimpleIoc.Default.Register<IOrderRepository, OrderRepository>();

            SimpleIoc.Default.Register<IOrderService, OrderService>();

            //Viewmodels
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<CategoryViewModel>();
            SimpleIoc.Default.Register<EmployeeViewModel>();
            SimpleIoc.Default.Register<ProductViewModel>();

        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public CategoryViewModel Category
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CategoryViewModel>();
            }
        }

        public EmployeeViewModel Employee
        {
            get
            {
                return ServiceLocator.Current.GetInstance<EmployeeViewModel>();
            }
        }

        public ProductViewModel Product
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProductViewModel>();
            }
        }

        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}