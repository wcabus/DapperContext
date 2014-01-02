using DapperContext.WpfExample.Model;
using GalaSoft.MvvmLight.Messaging;

namespace DapperContext.WpfExample.Messages
{
    public class CategoryChangedMessage : MessageBase
    {
        public CategoryChangedMessage(Category category)
        {
            Category = category;
        }

        public Category Category { get; private set; }
    }
}