using System.Collections.ObjectModel;

namespace Alba.JamlTestApp
{
    public partial class TreeViewWindow
    {
        public ObservableCollection<ItemModel> ItemModels { get; private set; }

        public TreeViewWindow ()
        {
            InitializeComponent();

            ItemModels = new ObservableCollection<ItemModel> {
                new ItemModel("Root Item") {
                    Items = {
                        new ItemModel("Item 1") {
                            Items = {
                                new ItemModel("Item 1 1"),
                                new ItemModel("Item 1 2"),
                                new ItemModel("Item 1 3"),
                            }
                        },
                        new ItemModel("Item 2") {
                            Items = {
                                new ItemModel("Item 2 1") {
                                    Items = {
                                        new ItemModel("Item 2 1 1"),
                                    }
                                },
                            }
                        },
                        new ItemModel("Item 3"),
                        new ItemModel("Item 4"),
                        new ItemModel("Item 5"),
                    }
                },
            };
        }

        public class ItemModel
        {
            public string Text { get; set; }
            public ObservableCollection<ItemModel> Items { get; private set; }

            public ItemModel (string text)
            {
                Text = text;
                Items = new ObservableCollection<ItemModel>();
            }
        }
    }
}