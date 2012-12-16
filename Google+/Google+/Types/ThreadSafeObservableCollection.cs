using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace Google_Plus.Types
{
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>, INotifyCollectionChanged
    {
        public ThreadSafeObservableCollection()
            : base()
        {
            Clear();
        }
        
        public new event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
                App.UIDispatcher.BeginInvoke(CollectionChanged, this, e);
        }
    }
}
