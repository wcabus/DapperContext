using System;
using System.Windows;
using System.Windows.Input;
using DapperContext.WpfExample.Messages;
using DapperContext.WpfExample.Model;
using DapperContext.WpfExample.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace DapperContext.WpfExample.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IOrderService _service;

        private Order _currentOrder;
        private Category _selectedCategory;
        private Product _selectedProduct;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IOrderService service)
        {
            _service = service;
            AddToOrderCommand = new RelayCommand(AddProductToOrder);
            SaveCommand = new RelayCommand(SaveOrder);
        }

        public Order CurrentOrder
        {
            get { return _currentOrder; }
            set
            {
                _currentOrder = value;
                RaisePropertyChanged(() => CurrentOrder);
            }
        }

        public Category SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (_selectedCategory == value)
                    return;

                _selectedCategory = value;
                RaisePropertyChanged(() => SelectedCategory);
                MessengerInstance.Send(new CategoryChangedMessage(_selectedCategory));
            }
        }

        public Product SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                RaisePropertyChanged(() => SelectedProduct);
            }
        }

        public ICommand AddToOrderCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        private void AddProductToOrder()
        {
            if (SelectedProduct == null)
                return;

            if (CurrentOrder == null)
                CurrentOrder = _service.CreateOrder(null); //Let the service use a default customer

            _service.CreateOrderDetail(CurrentOrder, SelectedProduct);
            RaisePropertyChanged(() => CurrentOrder); //To indicate that the number of Lines has changed
        }

        private void SaveOrder()
        {
            if (CurrentOrder == null)
                return;

            try
            {
                _service.SaveOrder(CurrentOrder);
                MessageBox.Show("Your order has been saved.");
                
                CurrentOrder = null; //reset
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("An error occurred while saving your order: {0}", e.Message));
            }
        }
    }
}