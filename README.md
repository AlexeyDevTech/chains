# Simple state machine v2
Вариант простой машины состояний для образования цепочки дейтсвий, (необязательно, но желательно) привязанной к какому-либо параметру.

# Использование

```c#
   var sm = new BaseStateManager<TestStates>();
   sm.From(TestStates.Idle)
       .Enter(x => { /*something action to enter "Idle" state*/})
       .Update(x => {/*something action to update "Idle" state*/ })
       .Exit(x => { /*something action to exit "Idle" state*/})
       .If(() => true /*something predicate to transition "Normal" state*/, TestStates.Normal);
   sm.From(TestStates.Normal);

   sm.Set(TestStates.Idle); //установка начального состояния
   sm.Start(); //запуск машины

   //в качестве примера ставлю таймер для обновления "машины"
   new Timer(state =>
   {
       sm.Update();
   }).Change(1000, 1000);
```
