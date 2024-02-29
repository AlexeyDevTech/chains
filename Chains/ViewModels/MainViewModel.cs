﻿using Chains.Core;
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
        private double _oldtr;

        public StateManager<TStates> SManager { get; set; }
        public MainViewModel()
        {
            SManager = new BaseStateManager<TStates>();
            SManager.From(TStates.Idle, true)
                .Enter(x => Debug.WriteLine(">>Idle enter action"))
                .Update(x => Debug.WriteLine(">>Idle update action"))
                .Exit(x => Debug.WriteLine(">>Idle exit action"))
                .If(() => {
                    return TRValue > 5;
                    }, TStates.Normal);

            SManager.From(TStates.Normal)
                .Enter(x => {
                    Debug.WriteLine(">>Normal enter action");
                    })
                .Update(x =>
                {
                    Debug.WriteLine(">>Normal update action");
                    if(TRValue > 5.2)
                    {
                        x.To(TStates.Error);
                    }
                })
                .Exit(x => Debug.WriteLine(">> Normal exit action"));

            SManager.From(TStates.Normal)
                .If(() => TRValue <= 4.5, TStates.Idle);

            SManager.From(TStates.Error)
                .Enter(x => Debug.WriteLine(">>ERROR enter"))
                .Exit(x => Debug.WriteLine(">>ERROR exit"))
                .If(() => TRValue < 5.05, TStates.Normal);
            SManager.Start();
            
        }

        public double TRValue 
        { 
            get => _trValue;
            set
            {
                if(_oldtr != Math.Round(value, 2))
                {
                    SetProperty(ref _trValue, Math.Round(value, 2));
                    Debug.WriteLine($"{_trValue}");
                    SManager.Update();
                    _oldtr = Math.Round(value, 2);
                }
                
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
