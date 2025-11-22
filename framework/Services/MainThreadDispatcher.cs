using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.Services
{
    public sealed partial class MainThreadDispatcher : Node
    {
        private readonly Queue<Action> _queue = new();

        public void Enqueue(Action a)
        {
            lock (_queue)
            {
                _queue.Enqueue(a);
            }
        }

        public override void _Ready()
        {
            base._Ready();

            ObjectAccessor.MainThreadDispatcher = this;
        }

        public override void _Process(double delta)
        {
            while (true)
            {
                Action action;
                lock (_queue)
                {
                    if (_queue.Count == 0)
                        break;

                    action = _queue.Dequeue();
                }
                action();
            }
        }
    }
}
