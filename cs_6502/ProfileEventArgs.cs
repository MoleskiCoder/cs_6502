namespace Simulator
{
    using System;

    public class ProfileEventArgs : EventArgs
    {
        private string output;

        public ProfileEventArgs(string output)
        {
            this.output = output;
        }

        public string Output
        {
            get
            {
                return this.output;
            }
        }
    }
}
