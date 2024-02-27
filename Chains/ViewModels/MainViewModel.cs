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
            s = new States().CreateInitialState("Root");
            s.Enter(() => Debug.WriteLine(">>Root enter action"))
             .Update(() => Debug.WriteLine(">>Root update action"))
             .Leave(() => Debug.WriteLine(">>Root leave action"))
            // .If(_trValue > 5, () => )
              .OnState("for2")
              .Enter(() => Debug.WriteLine(">>Enter to 2 state"))
              .Update(() => Debug.WriteLine(">>Update to 2 state"))
              .Leave(() => Debug.WriteLine(">>Leave to 2 state"));
            
            
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
