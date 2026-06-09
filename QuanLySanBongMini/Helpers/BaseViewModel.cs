using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLySanBongMini.Helpers
{
    // Lớp cơ sở để hỗ trợ binding dữ liệu lên giao diện
    // Tất cả ViewModel đều kế thừa từ lớp này
    public class BaseViewModel : INotifyPropertyChanged
    {
        // Sự kiện này sẽ thông báo cho giao diện khi có property thay đổi
        public event PropertyChangedEventHandler PropertyChanged;

        // Gọi hàm này mỗi khi muốn cập nhật giao diện
        protected void OnPropertyChanged([CallerMemberName] string tenProperty = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(tenProperty));
        }
    }
}
