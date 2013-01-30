using System;

namespace Whitespace
{
    public interface ICommand
    {
        // Stack Manipulation
        void Push(int n);
        void Duplicate();
        void Copy(int index);
        void Swap();
        void Discard();
        void Slide(int n);

        // Arithmetic
        void Add();
        void Sub();
        void Mul();
        void Div();
        void Mod();

        // Heap Access
        void Store();
        void Retrieve();

        // Flow Control
        bool? Test(Func<int, bool> f);
        void End();

        // IO
        void OutputCharacter();
        void OutputNumber();
        void ReadCharacter();
        void ReadNumber();

        // Debug
        void ShowStack();
        void ShowHeap();
    }
}

