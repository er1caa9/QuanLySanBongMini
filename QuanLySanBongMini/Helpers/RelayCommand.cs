using System;
using System.Windows.Input;

namespace QuanLySanBongMini.Helpers
{
    // Lớp này giúp tạo ra các lệnh (Command) để binding với Button trong XAML
    // Thay vì viết event click trực tiếp, ta dùng Command cho sạch hơn
    public class RelayCommand : ICommand
    {
        private Action<object> _hamThucHien;       // Hàm thực hiện khi bấm nút
        private Predicate<object> _hamKiemTraDuoc; // Hàm kiểm tra nút có được bấm không

        public RelayCommand(Action<object> hamThucHien, Predicate<object> hamKiemTraDuoc = null)
        {
            _hamThucHien = hamThucHien;
            _hamKiemTraDuoc = hamKiemTraDuoc;
        }

        // WPF sẽ gọi hàm này để biết nút có được bấm không (enabled/disabled)
        public bool CanExecute(object parameter)
        {
            if (_hamKiemTraDuoc == null)
                return true; // Mặc định là luôn được bấm
            return _hamKiemTraDuoc(parameter);
        }

        // WPF sẽ gọi hàm này khi người dùng bấm nút
        public void Execute(object parameter)
        {
            _hamThucHien(parameter);
        }

        // WPF dùng sự kiện này để biết khi nào cần kiểm tra lại CanExecute
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Gọi hàm này để ép WPF kiểm tra lại CanExecute ngay lập tức
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
