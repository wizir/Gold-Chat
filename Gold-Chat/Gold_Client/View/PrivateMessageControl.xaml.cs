﻿using Gold_Client.ViewModel;
using System.Windows.Controls;

namespace Gold_Client.View
{
    public partial class PrivateMessageControl : UserControl
    {
        public PrivateMessageControl()
        {
            DataContext = new PrivateMessagePresenter();
            InitializeComponent();
        }
    }
}
