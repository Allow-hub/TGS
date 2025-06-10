using System.Threading;

namespace TechC
{
    /// <summary>
    /// STA（Single Thread Apartment）用のスレッド基底クラス。
    /// ThreadRunnerを継承し、スレッドのApartmentStateをSTAに設定する。
    /// COMコンポーネントやWinForms、WPFなどSTAが必要な用途向け。
    /// </summary>
    public abstract class StaThreadRunner : ThreadRunner
    {
        /// <summary>
        /// スレッドのApartmentStateをSTAに設定する。
        /// </summary>
        public StaThreadRunner() : base()
        {
            _thread.SetApartmentState(ApartmentState.STA);
        }
    }
}