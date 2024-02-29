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
        public StateManager<TStates> SManager { get; set; }
        public MainViewModel()
        {
            SManager = new BaseStateManager<TStates>();
            SManager.From(TStates.Idle, true)
                .Enter(x => Debug.WriteLine(">>Idle enter action"))
                .Update(x => Debug.WriteLine(">>Idle update action"))
                .Exit(x => Debug.WriteLine(">>Idle exit action"))
              

            
        }

        public double TRValue 
        { 
            get => _trValue;
            set
            {
                SetProperty(ref _trValue, Math.Round(value, 2));
                Debug.WriteLine($"{_trValue}");
                SManager.Update();
            }
        }
    }

    public enum TStates
    {
        Idle,
        Normal,
        Error
    }
}
