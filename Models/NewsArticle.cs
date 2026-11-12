using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyPersonalNewspaper.Models
{
    public class NewsArticle : INotifyPropertyChanged
    {
        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        private string _url;
        public string Url
        {
            get => _url;
            set { _url = value; OnPropertyChanged(); }
        }

        private string _imageUrl;
        public string ImageUrl
        {
            get => _imageUrl;
            set { _imageUrl = value; OnPropertyChanged(); }
        }

        private string _sourceName;
        public string SourceName
        {
            get => _sourceName;
            set { _sourceName = value; OnPropertyChanged(); }
        }

        private string _publishedAt;
        public string PublishedAt
        {
            get => _publishedAt;
            set { _publishedAt = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}