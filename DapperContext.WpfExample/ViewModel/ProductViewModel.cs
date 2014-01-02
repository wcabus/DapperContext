using System.Collections.ObjectModel;
using System.Windows;
using DapperContext.WpfExample.Messages;
using DapperContext.WpfExample.Model;
using DapperContext.WpfExample.Repository;
using GalaSoft.MvvmLight;

namespace DapperContext.WpfExample.ViewModel
{
    public class ProductViewModel : ViewModelBase
    {
        private readonly IProductRepository _repository;
        private ObservableCollection<Product> _products;


        public ProductViewModel(IProductRepository repository)
        {
            _repository = repository;
            MessengerInstance.Register(this, 
                (CategoryChangedMessage msg) => FindProductsByCategory(msg.Category));
        }

        public ObservableCollection<Product> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                RaisePropertyChanged(() => Products);
            }
        }

        public void PopulateProducts()
        {
            InitializeCollection();
            foreach (var product in _repository.GetProducts())
            {
                Products.Add(product);
            }
        }

        public void FindProducts(string name)
        {
            InitializeCollection();
            if (string.IsNullOrEmpty(name))
                return;

            foreach (var product in _repository.GetProducts(name))
            {
                Products.Add(product);
            }
        }

        public void FindProductsByCategory(Category category)
        {
            InitializeCollection();
            if (category == null)
                return;

            foreach (var product in _repository.GetProductsByCategory(category))
            {
                Products.Add(product);
            }
        }

        private void InitializeCollection()
        {
            if (Products == null)
                Products = new ObservableCollection<Product>();
            else
                Products.Clear();
        }
    }
}