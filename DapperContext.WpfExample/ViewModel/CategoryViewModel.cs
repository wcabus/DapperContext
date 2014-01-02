using System.Collections.ObjectModel;
using DapperContext.WpfExample.Model;
using DapperContext.WpfExample.Repository;
using GalaSoft.MvvmLight;

namespace DapperContext.WpfExample.ViewModel
{
    public class CategoryViewModel : ViewModelBase
    {
        private readonly ICategoryRepository _repository;
        private ObservableCollection<Category> _categories; 

        public CategoryViewModel(ICategoryRepository repository)
        {
            _repository = repository;
            PopulateCategories();
        }

        public ObservableCollection<Category> Categories
        {
            get { return _categories; }
            set { 
                _categories = value;
                RaisePropertyChanged(() => Categories);
            }
        }

        public void PopulateCategories()
        {
            if (Categories == null)
                Categories = new ObservableCollection<Category>();
            else
                Categories.Clear();

            foreach (var category in _repository.GetCategories())
            {
                Categories.Add(category);
            }
        }
    }
}