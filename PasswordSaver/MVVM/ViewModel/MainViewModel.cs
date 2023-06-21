using PasswordSaver.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordSaver.MVVM.ViewModel
{
    public class MainViewModel : NotifyPropertyChangedImpl
    {
        private ObservableCollection<object> templateViews;
        public ObservableCollection<object> TemplateViews
        {
            get { return templateViews; }
            set
            {
                templateViews = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            TemplateViews = new ObservableCollection<object>();
            TemplateViews.Add(new LogersPageViewModel());
        }
    }
}
