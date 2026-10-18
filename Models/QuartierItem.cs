using System.ComponentModel;

namespace AquaAlerte.Models;

public class QuartierItem : INotifyPropertyChanged
{
    public string Name { get; set; } = "";
    public string Ville { get; set; } = "";

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}