//using UnityEngine;
//using IceMilkTea.StateMachine;
//namespace TechC
//{
//    partial class GameCharacter
//    {
//        private class StateCollection
//        {
//            public readonly StateMachine.State Idle = new();
//            public readonly StateMachine.State Dead = new();
//            public readonly StateMachine.State Damage = new();
//        }

//        private StateMachine _stateMachine = new();
//        private StateCollection States { get; } = new();

//        private void InitStateMachine()
//        {
//            // 各状態の処理を設定
//            States.Idle.OnEnter = OnIdleEnter;
//            States.Idle.OnUpdate = OnIdleUpdate;

//            States.Dead.OnEnter = OnDeadEnter;
//            States.Dead.OnUpdate = OnDeadUpdate;
//            States.Dead.OnExit = OnDeadExit;

//            States.Damage.OnEnter = OnDamageEnter;
//            States.Damage.OnUpdate = OnDamageUpdate;

//            // 初期状態を Idle に
//            _stateMachine.ChangeState(States.Idle);
//        }

//        private void Update()
//        {
//            _stateMachine.Update();
//        }

//        // 状態遷移を行うメソッドを追加
//        public void ChangeStateToIdle() => _stateMachine.ChangeState(States.Idle);
//        public void ChangeStateToDead() => _stateMachine.ChangeState(States.Dead);
//        public void ChangeStateToDamage() => _stateMachine.ChangeState(States.Damage);



//        // Dead の処理
//        private void OnDeadEnter() => Debug.Log("Dead Enter");
//        private void OnDeadUpdate() => Debug.Log("Dead Update");
//        private void OnDeadExit() => Debug.Log("Dead Exit");

//        private void OnDamageEnter() => Debug.Log("Goal Enter");
//        private void OnDamageUpdate() => Debug.Log("Goal Update");
//    }
//}
