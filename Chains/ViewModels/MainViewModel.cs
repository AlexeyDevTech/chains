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
        State s;
        public MainViewModel()
        {
            s = new StateManager().CreateInitialState("Root");
            s.Enter(() => Debug.WriteLine(">>Root enter action"))
             .Update(() => Debug.WriteLine(">>Root update action"))
             .Leave(() => Debug.WriteLine(">>Root leave action"));
            s.Active();
            
        }

        public double TRValue 
        { 
            get => _trValue;
            set
            {
                SetProperty(ref _trValue, Math.Round(value, 2));
                s?.UpdateState();
                Debug.WriteLine($"{_trValue}");
            }
        }
    }
}
