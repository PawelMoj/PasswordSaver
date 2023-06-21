using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PasswordSaver.Core
{
    public class NotifyPropertyChangedImpl : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            var e = this.PropertyChanged;
            if (e == null)
            {
                return;
            }

            e(this, new PropertyChangedEventArgs(name));
        }
        public void RaiseAllPropertiesChanged()
        {
            var e = this.PropertyChanged;
            if (e == null)
            {
                return;
            }

            e(this, new PropertyChangedEventArgs(null));
        }

    }
}
