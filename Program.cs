using System;

namespace CCommentDFA
{
    /// </summary>
    internal enum State
    {
        S0, ///          - start, nothing read yet
        S1,///           - just read the opening '/'
        S2,///           - inside the comment body, last char was NOT a pending '*'
        S3, ///          - inside the comment body, last char WAS a '*' (pending close)
        S4_Accept, ///   - just closed the comment by reading "*/"
        Dead///          - trap state, input can never be accepted from here
    }

    internal static class CommentDfa
    {
        // Collapse any character that is not '*' or '/' onto the placeholder symbol 'a' -- matches the slide's alphabet {a, *, /}.
        private static char ToSymbol(char c) => (c == '*' || c == '/') ? c : 'a';

        // The transition function delta(state, symbol) -> state.
        private static State Step(State current, char symbol)
        {
            switch (current)
            {
                case State.S0:
                    return symbol == '/' ? State.S1 : State.Dead;

                case State.S1:
                    return symbol == '*' ? State.S2 : State.Dead;

                case State.S2:
                    if (symbol == '*') return State.S3;
                    return State.S2; // 'a' or '/' -> still inside, no pending star

                case State.S3:
                    if (symbol == '/') return State.S4_Accept; // "*/" closes it
                    if (symbol == '*') return State.S3;         // "**" stays pending
                    return State.S2; // star wasn't followed by '/', back inside

                case State.S4_Accept:
                    return State.Dead; // nothing may follow a completed comment

                case State.Dead:
                default:
                    return State.Dead;
            }
        }

        /// Runs the DFA over the whole string and reports accept/reject.
        public static bool IsValidComment(string input)
        {
            State state = State.S0;
            foreach (char c in input)
            {
                state = Step(state, ToSymbol(c));
                if (state == State.Dead) return false; // early exit
            }
            return state == State.S4_Accept;
        }
    }

    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine();
            Console.WriteLine("Enter your own strings to test (blank line to quit):");
            string? line;
            while (!string.IsNullOrEmpty(line = Console.ReadLine()))
            {
                Report(line);
            }
        }

        private static void Report(string s)
        {
            bool ok = CommentDfa.IsValidComment(s);
            Console.WriteLine($"{s,-16} is {(ok ? "ACCEPTED" : "REJECTED")}");
        }
    }
}