using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

public static class RaisePropertyChangedExtension
{
    public static void RaisePropertyChanged(this INotifyPropertyChanged @this, [CallerMemberName] string propertyName = null)
    {
        var declaringType = @this.GetType().GetEvent(nameof(INotifyPropertyChanged.PropertyChanged)).DeclaringType;
        var propertyChangedFieldInfo = declaringType.GetField(nameof(INotifyPropertyChanged.PropertyChanged), BindingFlags.Instance | BindingFlags.NonPublic);

        if (propertyChangedFieldInfo == null)
        {
            throw new System.Exception($"You tried to raise PropertyChangedEvent for control which does not implement {nameof(INotifyPropertyChanged)} interface");
        }

        var propertyChangedEventHandler = propertyChangedFieldInfo.GetValue(@this) as PropertyChangedEventHandler;
        propertyChangedEventHandler?.Invoke(@this, new PropertyChangedEventArgs(propertyName));
    }
}