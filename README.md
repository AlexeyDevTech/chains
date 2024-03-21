# Simple state machine v2
Вариант простой машины состояний для образования цепочки дейтсвий, (необязательно, но желательно) привязанной к какому-либо параметру.

# Использование

```c#
   var sm = new BaseStateManager<TestStates>();
   sm.From(TestStates.Idle)
       .Enter(x => { /*некоторое действие после входа в "Idle" состояние*/})
       .Update(x => {/*тело "Idle" состояния. Выполняется при вызове метода Update() "StateManager"*/ })
       .Exit(x => { /*некоторое действие после выхода из "Idle" состояния*/})
       .If(() => true /*некоторое условие для перехода в "Normal" состояние, указанное вторым аргументом*/, TestStates.Normal);
   sm.From(TestStates.Normal); //создает "Normal" состояние

   sm.Set(TestStates.Idle); //установка начального состояния
   sm.Start(); //запуск машины

   //в качестве примера ставлю таймер для обновления "машины"
   new Timer(state =>
   {
       sm.Update();
   }).Change(1000, 1000);


 public enum TestStates
 {
     Idle,
     Normal,
 }


```
