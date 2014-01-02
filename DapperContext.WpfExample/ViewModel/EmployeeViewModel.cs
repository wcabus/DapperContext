using System.Collections.ObjectModel;
using DapperContext.WpfExample.Model;
using DapperContext.WpfExample.Repository;
using GalaSoft.MvvmLight;

namespace DapperContext.WpfExample.ViewModel
{
    public class EmployeeViewModel : ViewModelBase
    {
        private readonly IEmployeeRepository _repository;
        private ObservableCollection<Employee> _employees;

        public EmployeeViewModel(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public ObservableCollection<Employee> Employees
        {
            get { return _employees; }
            set
            {
                _employees = value;
                RaisePropertyChanged(() => Employees);
            }
        }

        public void PopulateEmployees()
        {
            if (Employees == null)
                Employees = new ObservableCollection<Employee>();
            else 
                Employees.Clear();

            foreach (var employee in _repository.GetEmployees())
            {
                Employees.Add(employee);
            }
        }
    }
}