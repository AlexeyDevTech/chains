using Chains.Core;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chains.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private double _trValue;
     
        public MainViewModel()
        {
           
            
            
        }

        public double TRValue 
        { 
            get => _trValue;
            set
            {
                SetProperty(ref _trValue, Math.Round(value, 2));
                Debug.WriteLine($"{_trValue}");
            }
        }
    }
}
