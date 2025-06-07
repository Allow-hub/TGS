using System;
using System.Threading;

namespace TechC
{
    /// <summary>
    /// スレッドの起動・停止・ループ処理を抽象化した基底クラス。
    /// サブクラスでThreadMainを実装し、Init()でスレッド開始、Stop()/Dispose()で停止・終了できる。
    /// </summary>
    public abstract class ThreadRunner : IDisposable
    {
        protected Thread _thread;                // 実行用スレッド
        private volatile bool _running;        // スレッドの実行フラグ

        /// <summary>
        /// スレッドを生成するが、開始はしない。
        /// </summary>
        public ThreadRunner()
        {
            _thread = new Thread(ThreadLoop);
            _running = false;
        }

        /// <summary>
        /// スレッドを開始する。すでに開始済みの場合は何もしない。
        /// </summary>
        public void Init()
        {
            if (_running) return;
            _running = true;
            _thread.Start();
        }

        /// <summary>
        /// スレッドの停止を要求する（即時停止ではない）。
        /// </summary>
        public void Stop()
        {
            _running = false;
        }

        /// <summary>
        /// スレッドのメインループ。OnInit→ThreadMain(ループ)→OnStopの順で呼ばれる。
        /// </summary>
        private void ThreadLoop()
        {
            OnInit();

            while (_running)
            {
                ThreadMain();
            }

            OnStop();
        }

        /// <summary>
        /// スレッドループ内で毎回呼ばれる処理をサブクラスで実装する。
        /// </summary>
        protected abstract void ThreadMain();

        /// <summary>
        /// スレッド開始時に一度だけ呼ばれる。必要に応じてオーバーライド。
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// スレッド終了時に一度だけ呼ばれる。必要に応じてオーバーライド。
        /// </summary>
        protected virtual void OnStop() { }

        /// <summary>
        /// スレッドを停止し、終了まで待機する。IDisposable実装。
        /// </summary>
        public void Dispose()
        {
            Stop();
            if (_thread.IsAlive)
                _thread.Join();
        }
    }
}
