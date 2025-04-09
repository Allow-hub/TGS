using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    // コマンドインターフェース
    public interface ICommand
    {
        void Execute();
        void Undo();
        bool IsFinished { get; }

    }
}
